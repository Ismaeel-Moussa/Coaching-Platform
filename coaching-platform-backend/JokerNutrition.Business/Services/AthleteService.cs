using System.Security.Principal;
using System.Text.Json;
using JokerNutrition.Business.DTOs.Athletes;
using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Business.DTOs.Coach;
using JokerNutrition.Business.DTOs.NutritionPlans;
using JokerNutrition.Business.Forms.Athletes;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Business.DTOs.Supplements;

namespace JokerNutrition.Business.Services;

public interface IAthleteService
{
    Task<AthleteDashboardDto> GetDashboardAsync();
    Task<List<CoachFeedbackNoteDto>> GetFeedbackHistoryAsync();
    Task<MacroTargetDto> GetActiveTargetsAsync();
    Task<MacroTargetDto> GetTargetsForAthleteAsync(int athleteId);
    Task<MacroTargetDto> SetTargetsAsync(int athleteId, SetMacroTargetForm form);
    Task<DailyLogHistoryDto> GetDailyLogAsync(int athleteId, DateOnly date);
}

public class AthleteService : _BaseService, IAthleteService
{
    private readonly IAthleteRepository _athleteRepo;
    private readonly ICoachRepository _coachRepo;
    private readonly IMacroTargetRepository _macroTargetRepo;
    private readonly IMealLogRepository _mealLogRepo;
    private readonly IDailyDiaryRepository _diaryRepo;
    private readonly IWorkoutLogRepository _workoutLogRepo;
    private readonly IDiaryService _diaryService;
    private readonly INotificationService _notificationService;
    private readonly ICoachFeedbackNoteRepository _feedbackNoteRepo;
    private readonly IClientCheckInRepository _checkInRepo;
    private readonly ISupplementScheduleRepository _scheduleRepo;
    private readonly ISupplementLogRepository _supplementLogRepo;
    private readonly IExerciseSetLogRepository _setLogRepo;
    private readonly JokerNutritionContext _context;

    public AthleteService(
        IPrincipal principal,
        ILogger<AthleteService> logger,
        IAthleteRepository athleteRepo,
        ICoachRepository coachRepo,
        IMacroTargetRepository macroTargetRepo,
        IMealLogRepository mealLogRepo,
        IDailyDiaryRepository diaryRepo,
        IWorkoutLogRepository workoutLogRepo,
        IDiaryService diaryService,
        INotificationService notificationService,
        ICoachFeedbackNoteRepository feedbackNoteRepo,
        IClientCheckInRepository checkInRepo,
        ISupplementScheduleRepository scheduleRepo,
        ISupplementLogRepository supplementLogRepo,
        IExerciseSetLogRepository setLogRepo,
        JokerNutritionContext context)
        : base(principal, logger)
    {
        _athleteRepo = athleteRepo;
        _coachRepo = coachRepo;
        _macroTargetRepo = macroTargetRepo;
        _mealLogRepo = mealLogRepo;
        _diaryRepo = diaryRepo;
        _workoutLogRepo = workoutLogRepo;
        _diaryService = diaryService;
        _notificationService = notificationService;
        _feedbackNoteRepo = feedbackNoteRepo;
        _checkInRepo = checkInRepo;
        _scheduleRepo = scheduleRepo;
        _supplementLogRepo = supplementLogRepo;
        _setLogRepo = setLogRepo;
        _context = context;
    }

    public async Task<AthleteDashboardDto> GetDashboardAsync()
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .Include(a => a.User)
            .Include(a => a.AssignedCoach).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var diary = await _diaryService.GetOrCreateDiaryAsync(athlete.Id, today);

        var logs = await _mealLogRepo.Query()
            .Where(l => l.DailyDiaryId == diary.Id)
            .ToListAsync();

        var macroSummary = DiaryMapper.MapSummary(diary, logs);

        // Check today's workout status
        var todayWorkoutLog = await _workoutLogRepo.Query()
            .FirstOrDefaultAsync(w => w.AthleteId == athlete.Id && w.Date == today);

        var workoutStatus = todayWorkoutLog?.Status switch
        {
            WorkoutStatus.Completed => "Completed",
            WorkoutStatus.InProgress => "InProgress",
            WorkoutStatus.Missed => "Missed",
            _ => "NoProgram"
        };

        var coachName = athlete.AssignedCoach != null
            ? $"{athlete.AssignedCoach.User.FirstName} {athlete.AssignedCoach.User.LastName}"
            : "Coach";

        var recentFeedback = await GetMergedFeedbackNotesAsync(athlete.Id, coachName, 2);

        return new AthleteDashboardDto
        {
            Athlete = new AthleteInfoDto
            {
                Id = athlete.Id,
                FirstName = athlete.User.FirstName,
                LastName = athlete.User.LastName,
                CurrentStreak = athlete.CurrentStreak,
                LongestStreak = athlete.LongestStreak,
                TargetGoal = athlete.TargetGoal,
                ProfilePictureUrl = athlete.User.ProfilePictureUrl
            },
            Today = macroSummary,
            TodaysWorkoutStatus = workoutStatus,
            RecentFeedbackNotes = recentFeedback
        };
    }

    public async Task<List<CoachFeedbackNoteDto>> GetFeedbackHistoryAsync()
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .Include(a => a.AssignedCoach).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");

        var coachName = athlete.AssignedCoach != null
            ? $"{athlete.AssignedCoach.User.FirstName} {athlete.AssignedCoach.User.LastName}"
            : "Coach";

        return await GetMergedFeedbackNotesAsync(athlete.Id, coachName);
    }

    private async Task<List<CoachFeedbackNoteDto>> GetMergedFeedbackNotesAsync(int athleteId, string coachName, int? takeCount = null)
    {
        var feedbackQuery = _feedbackNoteRepo.Query()
            .Include(n => n.Coach).ThenInclude(c => c.User)
            .Where(n => n.AthleteId == athleteId)
            .OrderByDescending(n => n.CreatedAt);

        List<CoachFeedbackNote> feedbackNotes;
        if (takeCount.HasValue)
        {
            feedbackNotes = await feedbackQuery.Take(takeCount.Value).ToListAsync();
        }
        else
        {
            feedbackNotes = await feedbackQuery.ToListAsync();
        }

        var checkInQuery = _checkInRepo.Query()
            .Where(ci => ci.AthleteId == athleteId && ci.CoachReviewedAt != null && !string.IsNullOrEmpty(ci.CoachNotes))
            .OrderByDescending(ci => ci.CoachReviewedAt!.Value);

        List<ClientCheckIn> checkInReviews;
        if (takeCount.HasValue)
        {
            checkInReviews = await checkInQuery.Take(takeCount.Value).ToListAsync();
        }
        else
        {
            checkInReviews = await checkInQuery.ToListAsync();
        }

        var merged = feedbackNotes.Select(n => {
            var dto = CoachHubMapper.MapFeedbackNote(n);
            dto.Type = "General";
            return dto;
        }).ToList();

        foreach (var review in checkInReviews)
        {
            merged.Add(new CoachFeedbackNoteDto
            {
                Id = -review.Id,
                NoteText = review.CoachNotes!,
                CoachName = coachName,
                CreatedAt = review.CoachReviewedAt!.Value,
                Type = "CheckIn",
                WeekOf = review.WeekOf.ToString("yyyy-MM-dd")
            });
        }

        var sorted = merged.OrderByDescending(f => f.CreatedAt);
        if (takeCount.HasValue)
        {
            return sorted.Take(takeCount.Value).ToList();
        }
        return sorted.ToList();
    }

    public async Task<MacroTargetDto> GetActiveTargetsAsync()
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");

        return await GetTargetsForAthleteAsync(athlete.Id);
    }

    public async Task<MacroTargetDto> GetTargetsForAthleteAsync(int athleteId)
    {
        var target = await _macroTargetRepo.Query()
            .Include(t => t.SetByCoach).ThenInclude(c => c.User)
            .Where(t => t.AthleteId == athleteId && t.IsActive)
            .OrderByDescending(t => t.SetAt)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("No active macro target found for this athlete.");

        var coachName = $"{target.SetByCoach.User.FirstName} {target.SetByCoach.User.LastName}";
        return MacroTargetMapper.Map(target, coachName);
    }

    public async Task<MacroTargetDto> SetTargetsAsync(int athleteId, SetMacroTargetForm form)
    {
        var coachUserId = LoggedInUser.Id;
        var coach = await _coachRepo.Query()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == coachUserId)
            ?? throw new UnauthorizedAccessException("Coach profile not found.");

        // Deactivate existing targets
        var existingTargets = await _macroTargetRepo.Query()
            .Where(t => t.AthleteId == athleteId && t.IsActive)
            .ToListAsync();

        foreach (var t in existingTargets)
        {
            t.IsActive = false;
            _macroTargetRepo.Update(t);
        }

        var target = new MacroTarget
        {
            AthleteId = athleteId,
            SetByCoachId = coach.Id,
            SetByCoach = coach,
            TargetCalories = form.TargetCalories,
            TargetProtein = form.TargetProtein,
            TargetCarbs = form.TargetCarbs,
            TargetFat = form.TargetFat,
            WaterLitersTarget = form.WaterLitersTarget,
            StepsTarget = form.StepsTarget,
            IsActive = true,
            SetAt = DateTime.UtcNow
        };

        await _macroTargetRepo.CreateAsync(target);

        // Also update today's DailyDiary snapshot so the dashboard reflects
        // the new targets immediately (diary rows cache targets at creation time).
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayDiary = await _diaryRepo.Query()
            .FirstOrDefaultAsync(d => d.AthleteId == athleteId && d.Date == today);

        if (todayDiary is not null)
        {
            todayDiary.TargetCalories = form.TargetCalories;
            todayDiary.TargetProtein = form.TargetProtein;
            todayDiary.TargetCarbs = form.TargetCarbs;
            todayDiary.TargetFat = form.TargetFat;
            todayDiary.WaterLitersTarget = form.WaterLitersTarget;
            todayDiary.StepsTarget = form.StepsTarget;
            _diaryRepo.Update(todayDiary);
        }

        await _macroTargetRepo.SaveChangesAsync();

        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.Id == athleteId);
        if (athlete != null)
        {
            await _notificationService.CreateAndSendNotificationAsync(
                athlete.UserId,
                NotificationType.MacroAlert,
                $"Coach {coach.User.FirstName} updated your daily macro targets."
            );
        }

        var coachName = $"{coach.User.FirstName} {coach.User.LastName}";
        return MacroTargetMapper.Map(target, coachName);
    }

    public async Task<DailyLogHistoryDto> GetDailyLogAsync(int athleteId, DateOnly date)
    {
        // 1. Fetch Athlete info (including User & AssignedCoach for authorization) and Supplement logs in a single query (using SQL supplement projection)
        var athleteData = await _athleteRepo.Query()
            .AsNoTracking()
            .Where(a => a.Id == athleteId)
            .Select(a => new
            {
                Athlete = a,
                User = a.User,
                AssignedCoach = a.AssignedCoach,
                Supplements = a.SupplementSchedules
                    .Where(s => s.IsActive || s.Logs.Any(l => l.Date == date))
                    .OrderBy(s => s.Type)
                    .ThenBy(s => s.Name)
                    .Select(s => new SupplementDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Type = s.Type.ToString(),
                        Dosage = s.Dosage,
                        Notes = s.Notes,
                        IsTakenToday = s.Logs.Where(l => l.Date == date).Select(l => l.IsTaken).FirstOrDefault(),
                        TakenAt = s.Logs.Where(l => l.Date == date).Select(l => l.TakenAt).FirstOrDefault()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (athleteData is null)
        {
            throw new KeyNotFoundException("Athlete profile not found.");
        }

        var athlete = athleteData.Athlete;
        athlete.User = athleteData.User;
        athlete.AssignedCoach = athleteData.AssignedCoach;
        var supplementsDto = athleteData.Supplements;

        // Authorization check
        var currentUserId = LoggedInUser.Id;
        var role = LoggedInUser.Role;

        if (role == "Athlete")
        {
            if (athlete.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this athlete's logs.");
            }
        }
        else if (role == "Coach")
        {
            if (athlete.AssignedCoach == null || athlete.AssignedCoach.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("This athlete is not on your roster.");
            }
        }
        else if (role != "Admin")
        {
            throw new UnauthorizedAccessException("Invalid role permissions.");
        }

        // 2. Fetch Workout Log with eager collection loading
        var workoutLog = await _workoutLogRepo.Query()
            .AsNoTracking()
            .Include(w => w.Day).ThenInclude(d => d.Exercises).ThenInclude(e => e.Exercise)
            .Include(w => w.Sets).ThenInclude(s => s.Exercise)
            .AsSplitQuery()
            .FirstOrDefaultAsync(w => w.AthleteId == athleteId && w.Date == date);

        TodaysWorkoutDto? workoutDto = null;
        if (workoutLog is not null)
        {
            var loggedSets = workoutLog.Sets
                .OrderBy(s => s.ExerciseId)
                .ThenBy(s => s.SetNumber)
                .ToList();

            workoutDto = new TodaysWorkoutDto
            {
                WorkoutLogId = workoutLog.Id,
                Status = workoutLog.Status.ToString(),
                CompletedAt = workoutLog.CompletedAt,
                Day = workoutLog.Day is not null ? WorkoutMapper.MapDay(workoutLog.Day) : null,
                LoggedSets = loggedSets.Select(WorkoutMapper.MapSet).ToList()
            };
        }

        // 3. Fetch Nutrition Diary Log with eager collection loading
        var diary = await _diaryRepo.Query()
            .AsNoTracking()
            .Include(d => d.MealLogs).ThenInclude(l => l.Food)
            .Include(d => d.MealLogs).ThenInclude(l => l.Recipe)
            .FirstOrDefaultAsync(d => d.AthleteId == athleteId && d.Date == date);

        DailyDiaryDto? nutritionDto = null;
        if (diary is not null)
        {
            var logs = diary.MealLogs.OrderBy(l => l.LoggedAt).ToList();
            nutritionDto = DiaryMapper.Map(diary, logs);
        }

        var nutritionPlanAdherences = await GetNutritionPlanAdherencesAsync(
            athleteId,
            date,
            diary?.Id,
            workoutDto);

        return new DailyLogHistoryDto
        {
            Date = date,
            Workout = workoutDto,
            Nutrition = nutritionDto,
            NutritionPlanAdherences = nutritionPlanAdherences,
            Supplements = supplementsDto
        };
    }

    private async Task<List<NutritionPlanAdherenceDto>> GetNutritionPlanAdherencesAsync(
        int athleteId,
        DateOnly date,
        int? diaryId,
        TodaysWorkoutDto? workout)
    {
        var entries = diaryId.HasValue
            ? await _context.NutritionPlanDiaryEntries
                .AsNoTracking()
                .Where(entry => entry.DailyDiaryId == diaryId.Value)
                .OrderBy(entry => entry.LoggedAt)
                .ToListAsync()
            : new List<NutritionPlanDiaryEntry>();

        var loggedAssignmentIds = entries
            .Select(entry => entry.NutritionPlanAssignmentId)
            .Distinct()
            .ToList();
        var dayStart = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var nextDay = dayStart.AddDays(1);
        var assignments = await _context.NutritionPlanAssignments
            .AsNoTracking()
            .Where(item => item.AthleteId == athleteId &&
                           (loggedAssignmentIds.Contains(item.Id) ||
                            (item.StartDate < nextDay &&
                             (!item.EndDate.HasValue || item.EndDate.Value >= nextDay))))
            .OrderBy(item => item.StartDate)
            .ToListAsync();

        var result = new List<NutritionPlanAdherenceDto>();
        foreach (var assignment in assignments)
        {
            NutritionPlanDto? plan;
            try
            {
                plan = JsonSerializer.Deserialize<NutritionPlanDto>(
                    assignment.SnapshotJson,
                    NutritionPlanMapper.SnapshotJsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (plan is null)
                continue;

            result.Add(BuildNutritionPlanAdherence(
                assignment,
                plan,
                entries.Where(entry => entry.NutritionPlanAssignmentId == assignment.Id).ToList(),
                date,
                workout));
        }

        return result;
    }

    private static NutritionPlanAdherenceDto BuildNutritionPlanAdherence(
        NutritionPlanAssignment assignment,
        NutritionPlanDto plan,
        List<NutritionPlanDiaryEntry> entries,
        DateOnly date,
        TodaysWorkoutDto? workout)
    {
        var entriesByBlock = entries
            .GroupBy(entry => entry.NutritionMealBlockId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(entry => entry.LoggedAt).First());

        var hasConditionalBlocks = plan.MealBlocks.Any(block => block.TrainingDayOnly || block.RestDayOnly);
        var hasTrainingEntry = plan.MealBlocks.Any(block =>
            block.TrainingDayOnly && entriesByBlock.ContainsKey(block.Id));
        var hasRestEntry = plan.MealBlocks.Any(block =>
            block.RestDayOnly && entriesByBlock.ContainsKey(block.Id));

        var dayType = !hasConditionalBlocks
            ? "AllDays"
            : hasTrainingEntry
                ? "Training"
                : hasRestEntry
                    ? "Rest"
                    : workout?.Day is not null
                        ? workout.Day.IsRestDay ? "Rest" : "Training"
                        : "Unspecified";

        var visibleBlocks = plan.MealBlocks
            .Where(block => dayType switch
            {
                "Training" => !block.RestDayOnly,
                "Rest" => !block.TrainingDayOnly,
                _ => true
            })
            .OrderBy(block => block.OrderIndex)
            .ToList();
        var isPastDate = date < DateOnly.FromDateTime(DateTime.UtcNow);
        var isPartialDay = DateOnly.FromDateTime(assignment.StartDate) == date ||
                           (assignment.EndDate.HasValue && DateOnly.FromDateTime(assignment.EndDate.Value) == date);

        var blockDtos = visibleBlocks.Select(block =>
        {
            entriesByBlock.TryGetValue(block.Id, out var entry);
            var option = entry is null
                ? null
                : block.Options.FirstOrDefault(item => item.Id == entry.NutritionMealOptionId);
            var isConditionalDayUnknown = dayType == "Unspecified" &&
                                          (block.TrainingDayOnly || block.RestDayOnly);
            var isTrackable = block.Options.Any(OptionIsLoggable);
            var status = entry is not null
                ? "Completed"
                : !isTrackable || isConditionalDayUnknown || isPartialDay
                    ? "NotTracked"
                    : isPastDate ? "Missed" : "Pending";

            return new NutritionPlanBlockAdherenceDto
            {
                MealBlockId = block.Id,
                OrderIndex = block.OrderIndex,
                Label = block.Label,
                LabelAr = block.LabelAr,
                TargetCalories = block.TargetCalories,
                Status = status,
                MealOptionId = entry?.NutritionMealOptionId,
                OptionLabel = option?.Label,
                OptionLabelAr = option?.LabelAr,
                LoggedMealType = entry?.MealType,
                Servings = entry?.Servings,
                LoggedAt = entry?.LoggedAt
            };
        }).ToList();

        var completedBlocks = blockDtos.Count(block => block.Status == "Completed");
        var trackedBlocks = blockDtos.Count(block => block.Status != "NotTracked");
        return new NutritionPlanAdherenceDto
        {
            AssignmentId = assignment.Id,
            PlanName = plan.Name,
            PlanNameAr = plan.NameAr,
            DayType = dayType,
            IsPartialDay = isPartialDay,
            CompletedBlocks = completedBlocks,
            TotalBlocks = trackedBlocks,
            CompletionPercent = trackedBlocks == 0
                ? 0
                : Math.Round((decimal)completedBlocks / trackedBlocks * 100, 1),
            Blocks = blockDtos
        };
    }

    private static bool OptionIsLoggable(NutritionMealOptionDto option)
    {
        if (option.Items.Count == 0)
            return false;

        static string? GroupKey(NutritionOptionItemDto item) =>
            string.IsNullOrWhiteSpace(item.AlternativeGroupKey)
                ? null
                : item.AlternativeGroupKey.Trim().ToLowerInvariant();
        static bool ItemIsLoggable(NutritionOptionItemDto item) =>
            (item.FoodId.HasValue && item.Unit == IngredientUnit.Gram) ||
            (item.RecipeId.HasValue && item.Unit is IngredientUnit.Gram or IngredientUnit.Piece);

        var fixedItems = option.Items.Where(item => GroupKey(item) is null).ToList();
        if (fixedItems.Any(item => !ItemIsLoggable(item)))
            return false;

        return option.Items
            .Where(item => GroupKey(item) is not null)
            .GroupBy(item => GroupKey(item)!)
            .All(group => group.Any(ItemIsLoggable));
    }
}
