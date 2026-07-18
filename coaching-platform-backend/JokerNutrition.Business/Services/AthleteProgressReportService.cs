using System.Security.Principal;
using JokerNutrition.Business.DTOs.Coach;
using JokerNutrition.Business.Reports;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IAthleteProgressReportService
{
    Task<AthleteProgressReportDto> GetReportAsync(
        int athleteId,
        int weeks,
        bool includeCoachNotes,
        bool includePhotos,
        CancellationToken cancellationToken = default);

    Task<byte[]> GeneratePdfAsync(
        AthleteProgressReportDto report,
        bool includeCoachNotes,
        bool includePhotos,
        string language,
        CancellationToken cancellationToken = default);
}

public class AthleteProgressReportService : _BaseService, IAthleteProgressReportService
{
    private static readonly int[] AllowedWeekRanges = [4, 8, 12];
    private readonly JokerNutritionContext _context;
    private readonly IProgressReportPdfGenerator _pdfGenerator;
    private readonly IBlobStorageService _blobStorage;

    public AthleteProgressReportService(
        IPrincipal principal,
        ILogger<AthleteProgressReportService> logger,
        JokerNutritionContext context,
        IProgressReportPdfGenerator pdfGenerator,
        IBlobStorageService blobStorage)
        : base(principal, logger)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
        _blobStorage = blobStorage;
    }

    public async Task<AthleteProgressReportDto> GetReportAsync(
        int athleteId,
        int weeks,
        bool includeCoachNotes,
        bool includePhotos,
        CancellationToken cancellationToken = default)
    {
        ValidateWeeks(weeks);

        var athleteQuery = _context.Athletes
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (LoggedInUser.Role != "Admin")
        {
            var coachId = await _context.Coaches
                .Where(c => c.UserId == LoggedInUser.Id && c.IsActive)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new UnauthorizedAccessException("Coach profile not found.");
            athleteQuery = athleteQuery.Where(a => a.AssignedCoachId == coachId);
        }

        var athlete = await athleteQuery.FirstOrDefaultAsync(a => a.Id == athleteId, cancellationToken)
            ?? throw new KeyNotFoundException("Athlete not found or does not belong to your roster.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var currentWeekMonday = today.AddDays(-daysSinceMonday);
        var periodStart = currentWeekMonday.AddDays(-(weeks - 1) * 7);
        var periodStartUtc = DateTime.SpecifyKind(periodStart.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var periodEndExclusiveUtc = DateTime.SpecifyKind(today.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var checkIns = await _context.ClientCheckIns
            .AsNoTracking()
            .Include(c => c.Photos)
            .Where(c => c.AthleteId == athleteId && c.WeekOf >= periodStart && c.WeekOf <= today)
            .OrderBy(c => c.WeekOf)
            .ToListAsync(cancellationToken);
        var workouts = await _context.WorkoutLogs
            .AsNoTracking()
            .Where(w => w.AthleteId == athleteId && w.Date >= periodStart && w.Date <= today)
            .ToListAsync(cancellationToken);
        var diaries = await _context.DailyDiaries
            .AsNoTracking()
            .Include(d => d.MealLogs)
            .Where(d => d.AthleteId == athleteId && d.Date >= periodStart && d.Date <= today)
            .ToListAsync(cancellationToken);
        var notes = includeCoachNotes
            ? await _context.CoachFeedbackNotes
                .AsNoTracking()
                .Include(n => n.Coach).ThenInclude(c => c.User)
                .Where(n => n.AthleteId == athleteId &&
                            n.CreatedAt >= periodStartUtc && n.CreatedAt < periodEndExclusiveUtc)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken)
            : [];

        decimal? earliestWeight = checkIns.Count >= 2 ? checkIns.First().WeightKg : null;
        var latestWeight = checkIns.LastOrDefault()?.WeightKg ?? athlete.WeightKg;
        var completedWorkouts = workouts.Count(w => w.Status == WorkoutStatus.Completed);
        var trackedDiaries = diaries.Where(d => d.MealLogs.Count > 0).ToList();

        var report = new AthleteProgressReportDto
        {
            AthleteId = athlete.Id,
            AthleteName = $"{athlete.User.FirstName} {athlete.User.LastName}".Trim(),
            AvatarUrl = athlete.User.ProfilePictureUrl,
            TargetGoal = athlete.TargetGoal,
            HeightCm = athlete.HeightCm,
            PeriodStart = periodStart.ToString("yyyy-MM-dd"),
            PeriodEnd = today.ToString("yyyy-MM-dd"),
            Weeks = weeks,
            GeneratedAt = DateTime.UtcNow.ToString("O"),
            Summary = new ProgressReportSummaryDto
            {
                StartingWeightKg = earliestWeight,
                CurrentWeightKg = latestWeight,
                WeightChangeKg = earliestWeight.HasValue && latestWeight.HasValue
                    ? Math.Round(latestWeight.Value - earliestWeight.Value, 1)
                    : null,
                LoggedWorkouts = workouts.Count,
                CompletedWorkouts = completedWorkouts,
                WorkoutCompletionPercent = Percentage(completedWorkouts, workouts.Count),
                NutritionTrackedDays = trackedDiaries.Count,
                AverageCalorieAdherencePercent = AverageTargetAdherence(
                    trackedDiaries,
                    d => d.MealLogs.Sum(m => m.Calories),
                    d => d.TargetCalories),
                AverageProteinAdherencePercent = AverageTargetAdherence(
                    trackedDiaries,
                    d => d.MealLogs.Sum(m => m.Protein),
                    d => d.TargetProtein),
                AverageStepsAdherencePercent = AverageGoalAttainment(
                    diaries,
                    d => d.StepsWalked,
                    d => d.StepsTarget),
                CheckInCount = checkIns.Count
            },
            CheckIns = checkIns
                .OrderByDescending(c => c.WeekOf)
                .Select(c => new ProgressReportCheckInDto
                {
                    Id = c.Id,
                    WeekOf = c.WeekOf.ToString("yyyy-MM-dd"),
                    WeightKg = c.WeightKg,
                    WaistCm = c.WaistCm,
                    ChestCm = c.ChestCm,
                    ThighCm = c.ThighCm,
                    SleepQuality = c.SleepQuality,
                    EnergyLevel = c.EnergyLevel,
                    GutHealth = c.GutHealth,
                    TrainingStress = c.TrainingStress,
                    ReviewNotes = includeCoachNotes ? c.CoachNotes : null,
                    ReviewedAt = includeCoachNotes ? c.CoachReviewedAt?.ToString("O") : null
                })
                .ToList(),
            CoachNotes = notes.Select(n => new ProgressReportNoteDto
            {
                Id = n.Id,
                Text = n.NoteText,
                CoachName = $"{n.Coach.User.FirstName} {n.Coach.User.LastName}".Trim(),
                CreatedAt = n.CreatedAt.ToString("O")
            }).ToList()
        };

        for (var index = 0; index < weeks; index++)
        {
            var weekStart = periodStart.AddDays(index * 7);
            var weekEnd = weekStart.AddDays(6);
            var weekCheckIns = checkIns.Where(c => c.WeekOf >= weekStart && c.WeekOf <= weekEnd).ToList();
            var weekWorkouts = workouts.Where(w => w.Date >= weekStart && w.Date <= weekEnd).ToList();
            var weekDiaries = diaries.Where(d => d.Date >= weekStart && d.Date <= weekEnd).ToList();
            var trackedWeekDiaries = weekDiaries.Where(d => d.MealLogs.Count > 0).ToList();
            var weekCompleted = weekWorkouts.Count(w => w.Status == WorkoutStatus.Completed);

            report.WeeklyProgress.Add(new ProgressReportWeekDto
            {
                WeekOf = weekStart.ToString("yyyy-MM-dd"),
                WeightKg = weekCheckIns.OrderBy(c => c.WeekOf).LastOrDefault()?.WeightKg,
                LoggedWorkouts = weekWorkouts.Count,
                CompletedWorkouts = weekCompleted,
                WorkoutCompletionPercent = Percentage(weekCompleted, weekWorkouts.Count),
                CalorieAdherencePercent = AverageTargetAdherence(
                    trackedWeekDiaries,
                    d => d.MealLogs.Sum(m => m.Calories),
                    d => d.TargetCalories),
                ProteinAdherencePercent = AverageTargetAdherence(
                    trackedWeekDiaries,
                    d => d.MealLogs.Sum(m => m.Protein),
                    d => d.TargetProtein),
                StepsAdherencePercent = AverageGoalAttainment(
                    weekDiaries,
                    d => d.StepsWalked,
                    d => d.StepsTarget),
                CheckInSubmitted = weekCheckIns.Count > 0
            });
        }

        if (includePhotos && checkIns.Count > 0)
        {
            var checkInsWithPhotos = checkIns.Where(c => c.Photos.Count > 0).ToList();
            var selectedCheckIns = checkInsWithPhotos.Count == 0
                ? []
                : new[] { checkInsWithPhotos.First(), checkInsWithPhotos.Last() }
                .DistinctBy(c => c.Id);
            report.ProgressPhotos = selectedCheckIns
                .SelectMany(c => c.Photos.Select(photo => new ProgressReportPhotoDto
                {
                    Id = photo.Id,
                    WeekOf = c.WeekOf.ToString("yyyy-MM-dd"),
                    Angle = photo.Angle.ToString(),
                    Url = photo.BlobUrl
                }))
                .Take(6)
                .ToList();

            foreach (var photo in report.ProgressPhotos)
                photo.Url = await _blobStorage.GetReadUrlAsync(photo.Url, TimeSpan.FromMinutes(10));
        }

        return report;
    }

    public async Task<byte[]> GeneratePdfAsync(
        AthleteProgressReportDto report,
        bool includeCoachNotes,
        bool includePhotos,
        string language,
        CancellationToken cancellationToken = default)
    {
        return await _pdfGenerator.GenerateAsync(report, includeCoachNotes, includePhotos, language, cancellationToken);
    }

    private static void ValidateWeeks(int weeks)
    {
        if (!AllowedWeekRanges.Contains(weeks))
            throw new ArgumentException("Report range must be 4, 8, or 12 weeks.");
    }

    private static double? Percentage(decimal numerator, decimal denominator) =>
        denominator > 0 ? Math.Round((double)(numerator / denominator) * 100.0, 1) : null;

    private static double? AverageTargetAdherence<T>(
        IEnumerable<T> items,
        Func<T, decimal> consumed,
        Func<T, decimal> target)
    {
        var values = items
            .Where(item => target(item) > 0)
            .Select(item => Math.Clamp(
                100.0 - Math.Abs((double)(consumed(item) - target(item)) / (double)target(item) * 100.0),
                0.0,
                100.0))
            .ToList();
        return values.Count > 0 ? Math.Round(values.Average(), 1) : null;
    }

    private static double? AverageGoalAttainment<T>(
        IEnumerable<T> items,
        Func<T, int> consumed,
        Func<T, int> target)
    {
        var values = items
            .Where(item => target(item) > 0)
            .Select(item => Math.Clamp(consumed(item) / (double)target(item) * 100.0, 0.0, 100.0))
            .ToList();
        return values.Count > 0 ? Math.Round(values.Average(), 1) : null;
    }
}
