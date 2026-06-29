using System.Security.Principal;
using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Business.Forms.Workouts;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IWorkoutLogService
{
    Task<TodaysWorkoutDto> GetTodaysWorkoutAsync();
    Task<WorkoutProgramDto> GetProgramAsync();
    Task<SetLogDto> LogSetAsync(LogSetForm form);
    Task CompleteWorkoutAsync(CompleteWorkoutForm form);
    Task<List<WorkoutHistoryDto>> GetHistoryAsync();
}

public class WorkoutLogService : _BaseService, IWorkoutLogService
{
    private readonly JokerNutritionContext _context;
    private readonly IAthleteRepository _athleteRepo;
    private readonly IClientProgramRepository _clientProgramRepo;
    private readonly IWorkoutLogRepository _workoutLogRepo;
    private readonly IExerciseSetLogRepository _setLogRepo;
    private readonly INotificationService _notificationService;

    public WorkoutLogService(
        IPrincipal principal,
        ILogger<WorkoutLogService> logger,
        JokerNutritionContext context,
        IAthleteRepository athleteRepo,
        IClientProgramRepository clientProgramRepo,
        IWorkoutLogRepository workoutLogRepo,
        IExerciseSetLogRepository setLogRepo,
        INotificationService notificationService)
        : base(principal, logger)
    {
        _context = context;
        _athleteRepo = athleteRepo;
        _clientProgramRepo = clientProgramRepo;
        _workoutLogRepo = workoutLogRepo;
        _setLogRepo = setLogRepo;
        _notificationService = notificationService;
    }

    // ─── Get Today's Workout ──────────────────────────────────────────
    public async Task<TodaysWorkoutDto> GetTodaysWorkoutAsync()
    {
        var athlete = await GetAthleteAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var program = await GetActiveProgramWithTemplateAsync(athlete.Id);
        if (program == null)
            return new TodaysWorkoutDto { Status = "NoProgram" };

        var todayDay = GetTodayPplDay(program, today);
        if (todayDay == null)
            return new TodaysWorkoutDto { Status = "NoProgram" };

        // Get or create the workout log for today
        var workoutLog = await _workoutLogRepo.Query()
            .FirstOrDefaultAsync(w => w.AthleteId == athlete.Id && w.Date == today);

        if (workoutLog == null)
        {
            workoutLog = new WorkoutLog
            {
                AthleteId = athlete.Id,
                WorkoutTemplateDayId = todayDay.Id,
                Date = today,
                Status = WorkoutStatus.InProgress
            };
            await _workoutLogRepo.CreateAsync(workoutLog);
            await _workoutLogRepo.SaveChangesAsync();
        }

        // Load logged sets for today
        var loggedSets = await _setLogRepo.Query()
            .Include(s => s.Exercise)
            .Where(s => s.WorkoutLogId == workoutLog.Id)
            .OrderBy(s => s.ExerciseId)
            .ThenBy(s => s.SetNumber)
            .ToListAsync();

        return new TodaysWorkoutDto
        {
            WorkoutLogId = workoutLog.Id,
            Status = workoutLog.Status.ToString(),
            Day = WorkoutMapper.MapDay(todayDay),
            LoggedSets = loggedSets.Select(WorkoutMapper.MapSet).ToList()
        };
    }

    // ─── Get Full 6-Day Program ───────────────────────────────────────
    public async Task<WorkoutProgramDto> GetProgramAsync()
    {
        var athlete = await GetAthleteAsync();
        var program = await GetActiveProgramWithTemplateAsync(athlete.Id)
            ?? throw new KeyNotFoundException("No active workout program found.");

        return new WorkoutProgramDto
        {
            TemplateId = program.WorkoutTemplate.Id,
            TemplateName = program.WorkoutTemplate.Name,
            Description = program.WorkoutTemplate.Description,
            StartDate = program.StartDate,
            Days = program.WorkoutTemplate.Days
                .OrderBy(d => d.DayNumber)
                .Select(WorkoutMapper.MapDay)
                .ToList()
        };
    }

    // ─── Log a Set ────────────────────────────────────────────────────
    public async Task<SetLogDto> LogSetAsync(LogSetForm form)
    {
        var athlete = await GetAthleteAsync();

        var workoutLog = await _workoutLogRepo.Query()
            .FirstOrDefaultAsync(w => w.Id == form.WorkoutLogId && w.AthleteId == athlete.Id)
            ?? throw new KeyNotFoundException("Workout log not found.");

        var set = new ExerciseSetLog
        {
            WorkoutLogId = form.WorkoutLogId,
            ExerciseId = form.ExerciseId,
            SetNumber = form.SetNumber,
            WeightKg = form.WeightKg,
            Reps = form.Reps,
            IsCompleted = true
        };

        await _setLogRepo.CreateAsync(set);
        await _setLogRepo.SaveChangesAsync();

        // Reload with Exercise name for mapping
        var saved = await _setLogRepo.Query()
            .Include(s => s.Exercise)
            .FirstAsync(s => s.Id == set.Id);

        return WorkoutMapper.MapSet(saved);
    }

    // ─── Complete Workout + Streak ────────────────────────────────────
    public async Task CompleteWorkoutAsync(CompleteWorkoutForm form)
    {
        var athlete = await GetAthleteAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var workoutLog = await _workoutLogRepo.Query()
            .FirstOrDefaultAsync(w => w.Id == form.WorkoutLogId && w.AthleteId == athlete.Id)
            ?? throw new KeyNotFoundException("Workout log not found.");

        workoutLog.Status = WorkoutStatus.Completed;
        workoutLog.CompletedAt = DateTime.UtcNow;
        _workoutLogRepo.Update(workoutLog);

        // Streak logic
        var yesterday = today.AddDays(-1);
        if (athlete.LastWorkoutDate.HasValue &&
            DateOnly.FromDateTime(athlete.LastWorkoutDate.Value) == yesterday)
        {
            athlete.CurrentStreak++;
        }
        else
        {
            athlete.CurrentStreak = 1;
        }

        if (athlete.CurrentStreak > athlete.LongestStreak)
            athlete.LongestStreak = athlete.CurrentStreak;

        athlete.LastWorkoutDate = DateTime.UtcNow;
        _athleteRepo.Update(athlete);

        await _workoutLogRepo.SaveChangesAsync();

        // Push real-time silent update to coach
        var coachUserId = await _athleteRepo.Query()
            .Where(a => a.Id == athlete.Id && a.AssignedCoachId.HasValue)
            .Select(a => a.AssignedCoach!.UserId)
            .FirstOrDefaultAsync();

        if (coachUserId > 0)
        {
            await _notificationService.SendDirectUpdateAsync(coachUserId, "AthleteActivity", new { type = "WorkoutCompleted", athleteId = athlete.Id });
        }
    }

    // ─── Exercise History (progressive overload) ──────────────────────
    public async Task<List<WorkoutHistoryDto>> GetHistoryAsync()
    {
        var athlete = await GetAthleteAsync();

        var allSets = await _setLogRepo.Query()
            .Include(s => s.Exercise)
            .Include(s => s.WorkoutLog)
            .Where(s => s.WorkoutLog.AthleteId == athlete.Id && s.IsCompleted)
            .OrderBy(s => s.ExerciseId)
            .ThenByDescending(s => s.WorkoutLog.Date)
            .ThenBy(s => s.SetNumber)
            .ToListAsync();

        var result = allSets
            .GroupBy(s => new { s.ExerciseId, ExerciseName = s.Exercise.Name })
            .Select(g => new WorkoutHistoryDto
            {
                ExerciseId = g.Key.ExerciseId,
                ExerciseName = g.Key.ExerciseName,
                Sessions = g
                    .GroupBy(s => s.WorkoutLog.Date)
                    .OrderByDescending(sg => sg.Key)
                    .Select(sg => new ExerciseSessionDto
                    {
                        Date = sg.Key,
                        Sets = sg.Select(WorkoutMapper.MapSet).ToList()
                    })
                    .ToList()
            })
            .ToList();

        return result;
    }

    // ─── Private helpers ──────────────────────────────────────────────
    private async Task<Athlete> GetAthleteAsync()
    {
        var userId = LoggedInUser.Id;
        return await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");
    }

    private async Task<ClientProgram?> GetActiveProgramWithTemplateAsync(int athleteId) =>
        await _clientProgramRepo.Query()
            .Include(p => p.WorkoutTemplate)
                .ThenInclude(t => t.Days)
                    .ThenInclude(d => d.Exercises)
                        .ThenInclude(e => e.Exercise)
            .Where(p => p.AthleteId == athleteId && p.IsActive)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync();

    private static WorkoutTemplateDay? GetTodayPplDay(ClientProgram program, DateOnly today)
    {
        var days = program.WorkoutTemplate?.Days;
        if (days == null || !days.Any()) return null;

        var daysSinceStart = (today.ToDateTime(TimeOnly.MinValue) - program.StartDate.Date).Days;
        var cycleIndex = (daysSinceStart % 6) + 1; // 1-6 rolling cycle

        return days.FirstOrDefault(d => d.DayNumber == cycleIndex);
    }
}
