using System.Security.Principal;
using JokerNutrition.Business.DTOs.Athletes;
using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Business.Forms.Athletes;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IAthleteService
{
    Task<AthleteDashboardDto> GetDashboardAsync();
    Task<MacroTargetDto> GetActiveTargetsAsync();
    Task<MacroTargetDto> GetTargetsForAthleteAsync(int athleteId);
    Task<MacroTargetDto> SetTargetsAsync(int athleteId, SetMacroTargetForm form);
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
        ICoachFeedbackNoteRepository feedbackNoteRepo)
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
    }

    public async Task<AthleteDashboardDto> GetDashboardAsync()
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .Include(a => a.User)
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

        // Fetch recent coach feedback notes
        var feedbackNotes = await _feedbackNoteRepo.Query()
            .Include(n => n.Coach).ThenInclude(c => c.User)
            .Where(n => n.AthleteId == athlete.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .ToListAsync();

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
            RecentFeedbackNotes = feedbackNotes.Select(CoachHubMapper.MapFeedbackNote).ToList()
        };
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
}
