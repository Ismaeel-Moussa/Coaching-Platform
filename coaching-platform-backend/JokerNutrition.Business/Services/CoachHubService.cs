using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Athletes;
using JokerNutrition.Business.DTOs.Coach;
using JokerNutrition.Business.Forms.Coach;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface ICoachHubService
{
    Task<CoachDashboardDto> GetDashboardAsync();
    Task<PagedResult<LiveFeedItemDto>> GetLiveFeedAsync(int page, int pageSize);
    Task<List<ComplianceItemDto>> GetComplianceRosterAsync();
    Task<PagedResult<RosterItemDto>> GetRosterAsync(int page, int pageSize, string? filter);
    Task<AthleteDeepProfileDto> GetAthleteDeepProfileAsync(int athleteId);
    Task<CoachFeedbackNoteDto> SaveFeedbackNoteAsync(int athleteId, SaveFeedbackNoteForm form);
    Task<List<WeightHistoryPointDto>> GetWeightHistoryAsync(int athleteId);
    Task SetMacroTargetsAsync(int athleteId, SetMacroTargetsForm form);
}

public class CoachHubService : _BaseService, ICoachHubService
{
    private readonly ICoachRepository _coachRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly IWorkoutLogRepository _workoutLogRepo;
    private readonly IDailyDiaryRepository _diaryRepo;
    private readonly IMacroTargetRepository _macroTargetRepo;
    private readonly IClientCheckInRepository _checkInRepo;
    private readonly IClientProgramRepository _clientProgramRepo;
    private readonly ICoachFeedbackNoteRepository _feedbackNoteRepo;
    private readonly INotificationService _notificationService;

    public CoachHubService(
        IPrincipal principal,
        ILogger<CoachHubService> logger,
        ICoachRepository coachRepo,
        IAthleteRepository athleteRepo,
        IWorkoutLogRepository workoutLogRepo,
        IDailyDiaryRepository diaryRepo,
        IMacroTargetRepository macroTargetRepo,
        IClientCheckInRepository checkInRepo,
        IClientProgramRepository clientProgramRepo,
        ICoachFeedbackNoteRepository feedbackNoteRepo,
        INotificationService notificationService)
        : base(principal, logger)
    {
        _coachRepo = coachRepo;
        _athleteRepo = athleteRepo;
        _workoutLogRepo = workoutLogRepo;
        _diaryRepo = diaryRepo;
        _macroTargetRepo = macroTargetRepo;
        _checkInRepo = checkInRepo;
        _clientProgramRepo = clientProgramRepo;
        _feedbackNoteRepo = feedbackNoteRepo;
        _notificationService = notificationService;
    }

    // ─── Dashboard ────────────────────────────────────────────────────

    public async Task<CoachDashboardDto> GetDashboardAsync()
    {
        var coach = await GetCoachAsync();
        var athletes = LoggedInUser.Role == "Admin"
            ? await _athleteRepo.Query().Include(a => a.User).ToListAsync()
            : await GetCoachAthletesAsync(coach.Id);
        var athleteIds = athletes.Select(a => a.Id).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Active athlete count
        var activeCount = athletes.Count;

        // Average workout completion % for the past 7 days
        var weekAgo = today.AddDays(-7);
        var recentLogs = await _workoutLogRepo.Query()
            .Where(w => athleteIds.Contains(w.AthleteId) && w.Date >= weekAgo)
            .ToListAsync();

        double avgCompletion = 0;
        if (recentLogs.Any())
        {
            avgCompletion = recentLogs.Count(w => w.Status == WorkoutStatus.Completed)
                            / (double)recentLogs.Count * 100.0;
        }

        // Pending check-ins: athletes with no check-in in the past 7 days
        var weekStart = DateTime.UtcNow.AddDays(-7);
        var recentCheckInAthleteIds = await _checkInRepo.Query()
            .Where(c => athleteIds.Contains(c.AthleteId) && c.SubmittedAt >= weekStart)
            .Select(c => c.AthleteId)
            .Distinct()
            .ToListAsync();

        var pendingCount = athleteIds.Count - recentCheckInAthleteIds.Count;

        // Last 10 live feed items
        var recentFeed = await _workoutLogRepo.Query()
            .Include(w => w.Athlete).ThenInclude(a => a.User)
            .Include(w => w.Day)
            .Where(w => athleteIds.Contains(w.AthleteId))
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.CompletedAt)
            .Take(10)
            .ToListAsync();

        return new CoachDashboardDto
        {
            ActiveAthleteCount = activeCount,
            AvgWorkoutCompletionPercent = Math.Round(avgCompletion, 1),
            PendingCheckInsCount = Math.Max(0, pendingCount),
            RecentFeed = recentFeed.Select(CoachHubMapper.MapLiveFeedItem).ToList()
        };
    }

    // ─── Live Feed ────────────────────────────────────────────────────

    public async Task<PagedResult<LiveFeedItemDto>> GetLiveFeedAsync(int page, int pageSize)
    {
        var coach = await GetCoachAsync();
        var athleteIds = LoggedInUser.Role == "Admin"
            ? await _athleteRepo.Query().Select(a => a.Id).ToListAsync()
            : await GetCoachAthleteIdsAsync(coach.Id);

        var query = _workoutLogRepo.Query()
            .Include(w => w.Athlete).ThenInclude(a => a.User)
            .Include(w => w.Day)
            .Where(w => athleteIds.Contains(w.AthleteId))
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.CompletedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LiveFeedItemDto>
        {
            Items = items.Select(CoachHubMapper.MapLiveFeedItem),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ─── Compliance Roster ────────────────────────────────────────────

    public async Task<List<ComplianceItemDto>> GetComplianceRosterAsync()
    {
        var coach = await GetCoachAsync();
        var athletes = LoggedInUser.Role == "Admin"
            ? await _athleteRepo.Query().Include(a => a.User).ToListAsync()
            : await GetCoachAthletesAsync(coach.Id);
        var athleteIds = athletes.Select(a => a.Id).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Load today's diaries with meal logs
        var diaries = await _diaryRepo.Query()
            .Include(d => d.MealLogs)
            .Where(d => athleteIds.Contains(d.AthleteId) && d.Date == today)
            .ToListAsync();

        // Load active macro targets
        var targets = await _macroTargetRepo.Query()
            .Where(t => athleteIds.Contains(t.AthleteId) && t.IsActive)
            .ToListAsync();

        var result = athletes.Select(athlete =>
        {
            var diary = diaries.FirstOrDefault(d => d.AthleteId == athlete.Id);
            var target = targets.FirstOrDefault(t => t.AthleteId == athlete.Id);
            return CoachHubMapper.MapComplianceItem(athlete, diary, target);
        }).ToList();

        return result;
    }

    // ─── Roster ───────────────────────────────────────────────────────

    public async Task<PagedResult<RosterItemDto>> GetRosterAsync(int page, int pageSize, string? filter)
    {
        var coach = await GetCoachAsync();
        var athletes = LoggedInUser.Role == "Admin"
            ? await _athleteRepo.Query().Include(a => a.User).ToListAsync()
            : await GetCoachAthletesAsync(coach.Id);
        var athleteIds = athletes.Select(a => a.Id).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Load active programs
        var programs = await _clientProgramRepo.Query()
            .Include(p => p.WorkoutTemplate)
            .Where(p => athleteIds.Contains(p.AthleteId) && p.IsActive)
            .ToListAsync();

        // Load last check-ins per athlete
        var allCheckIns = await _checkInRepo.Query()
            .Where(c => athleteIds.Contains(c.AthleteId))
            .ToListAsync();

        // Load today's compliance data
        var complianceItems = await GetComplianceRosterAsync();
        var complianceMap = complianceItems.ToDictionary(c => c.AthleteId, c => c.CompliancePercent);

        // Build roster items
        var rosterItems = athletes.Select(athlete =>
        {
            var program = programs.FirstOrDefault(p => p.AthleteId == athlete.Id);
            var lastCheckIn = allCheckIns
                .Where(c => c.AthleteId == athlete.Id)
                .OrderByDescending(c => c.SubmittedAt)
                .FirstOrDefault();
            var compliance = complianceMap.GetValueOrDefault(athlete.Id, 0);
            return CoachHubMapper.MapRosterItem(athlete, program, lastCheckIn, compliance);
        }).ToList();

        // Apply filter
        var filtered = filter switch
        {
            "ComplianceAlert" => rosterItems.Where(r => r.Status == "ComplianceAlert").ToList(),
            "NoRecentCheckIn" => rosterItems.Where(r => r.Status == "NoRecentCheckIn").ToList(),
            _ => rosterItems
        };

        var totalCount = filtered.Count;
        var paged = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<RosterItemDto>
        {
            Items = paged,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ─── Athlete Deep Profile ─────────────────────────────────────────

    public async Task<AthleteDeepProfileDto> GetAthleteDeepProfileAsync(int athleteId)
    {
        var coach = await GetCoachAsync();
        await EnsureAthleteInRosterAsync(coach.Id, athleteId);

        var athlete = await _athleteRepo.Query()
            .Include(a => a.User)
            .FirstAsync(a => a.Id == athleteId);

        // Current macro targets
        var target = await _macroTargetRepo.Query()
            .Include(t => t.SetByCoach).ThenInclude(c => c.User)
            .Where(t => t.AthleteId == athleteId && t.IsActive)
            .OrderByDescending(t => t.SetAt)
            .FirstOrDefaultAsync();

        MacroTargetDto? targetDto = null;
        if (target != null)
        {
            var coachName = $"{target.SetByCoach.User.FirstName} {target.SetByCoach.User.LastName}";
            targetDto = MacroTargetMapper.Map(target, coachName);
        }

        // Weight history from check-ins
        var checkIns = await _checkInRepo.Query()
            .Where(c => c.AthleteId == athleteId)
            .OrderBy(c => c.WeekOf)
            .ToListAsync();

        // Feedback notes
        var notes = await _feedbackNoteRepo.Query()
            .Include(n => n.Coach).ThenInclude(c => c.User)
            .Where(n => n.AthleteId == athleteId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return new AthleteDeepProfileDto
        {
            Id = athlete.Id,
            FullName = $"{athlete.User.FirstName} {athlete.User.LastName}",
            AvatarUrl = athlete.User.ProfilePictureUrl,
            TargetGoal = athlete.TargetGoal,
            WeightKg = athlete.WeightKg,
            HeightCm = athlete.HeightCm,
            CurrentStreak = athlete.CurrentStreak,
            LongestStreak = athlete.LongestStreak,
            CurrentTargets = targetDto,
            WeightHistory = checkIns.Select(CoachHubMapper.MapWeightPoint).ToList(),
            FeedbackNotes = notes.Select(CoachHubMapper.MapFeedbackNote).ToList()
        };
    }

    // ─── Save Feedback Note ───────────────────────────────────────────

    public async Task<CoachFeedbackNoteDto> SaveFeedbackNoteAsync(int athleteId, SaveFeedbackNoteForm form)
    {
        var coach = await GetCoachAsync();
        await EnsureAthleteInRosterAsync(coach.Id, athleteId);

        var note = new CoachFeedbackNote
        {
            AthleteId = athleteId,
            CoachId = coach.Id,
            NoteText = form.NoteText,
            CreatedAt = DateTime.UtcNow
        };

        await _feedbackNoteRepo.CreateAsync(note);
        await _feedbackNoteRepo.SaveChangesAsync();

        var athleteUserId = await _athleteRepo.Query()
            .Where(a => a.Id == athleteId)
            .Select(a => a.UserId)
            .FirstAsync();

        await _notificationService.CreateAndSendNotificationAsync(
            athleteUserId,
            NotificationType.CoachNote,
            $"Your coach left you a new feedback note: \"{note.NoteText}\""
        );

        // Reload with coach navigation prop for mapping
        var saved = await _feedbackNoteRepo.Query()
            .Include(n => n.Coach).ThenInclude(c => c.User)
            .FirstAsync(n => n.Id == note.Id);

        return CoachHubMapper.MapFeedbackNote(saved);
    }

    // ─── Weight History ───────────────────────────────────────────────

    public async Task<List<WeightHistoryPointDto>> GetWeightHistoryAsync(int athleteId)
    {
        var coach = await GetCoachAsync();
        await EnsureAthleteInRosterAsync(coach.Id, athleteId);

        var checkIns = await _checkInRepo.Query()
            .Where(c => c.AthleteId == athleteId)
            .OrderBy(c => c.WeekOf)
            .ToListAsync();

        return checkIns.Select(CoachHubMapper.MapWeightPoint).ToList();
    }

    // ─── Set Macro Targets ───────────────────────────────────────────

    public async Task SetMacroTargetsAsync(int athleteId, SetMacroTargetsForm form)
    {
        var coach = await GetCoachAsync();
        await EnsureAthleteInRosterAsync(coach.Id, athleteId);

        // Deactivate existing targets
        var currentTargets = await _macroTargetRepo.Query()
            .Where(t => t.AthleteId == athleteId && t.IsActive)
            .ToListAsync();

        var activeTarget = currentTargets.FirstOrDefault();

        bool nutritionChanged = activeTarget == null ||
            activeTarget.TargetCalories != form.TargetCalories ||
            activeTarget.TargetProtein != form.TargetProtein ||
            activeTarget.TargetCarbs != form.TargetCarbs ||
            activeTarget.TargetFat != form.TargetFat;

        bool activityChanged = activeTarget == null ||
            activeTarget.WaterLitersTarget != form.WaterLitersTarget ||
            activeTarget.StepsTarget != form.StepsTarget;

        foreach (var target in currentTargets)
        {
            target.IsActive = false;
            _macroTargetRepo.Update(target);
        }

        // Create new targets
        var newTarget = new MacroTarget
        {
            AthleteId = athleteId,
            SetByCoachId = coach.Id,
            TargetCalories = form.TargetCalories,
            TargetProtein = form.TargetProtein,
            TargetCarbs = form.TargetCarbs,
            TargetFat = form.TargetFat,
            WaterLitersTarget = form.WaterLitersTarget,
            StepsTarget = form.StepsTarget,
            IsActive = true,
            SetAt = DateTime.UtcNow
        };

        await _macroTargetRepo.CreateAsync(newTarget);

        // Update today's DailyDiary snapshot so the athlete dashboard reflects
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

        // Notify Athlete
        var athleteUserId = await _athleteRepo.Query()
            .Where(a => a.Id == athleteId)
            .Select(a => a.UserId)
            .FirstAsync();

        string notificationMessage;
        if (nutritionChanged && activityChanged)
        {
            notificationMessage = $"Daily targets updated: {Math.Round(form.TargetCalories)} kcal, {Math.Round(form.TargetProtein)}g P, {Math.Round(form.TargetCarbs)}g C, {Math.Round(form.TargetFat)}g F, {form.WaterLitersTarget.ToString("0.#")}L Water, {form.StepsTarget} Steps.";
        }
        else if (nutritionChanged)
        {
            notificationMessage = $"Daily nutrition targets updated: {Math.Round(form.TargetCalories)} kcal, {Math.Round(form.TargetProtein)}g P, {Math.Round(form.TargetCarbs)}g C, {Math.Round(form.TargetFat)}g F.";
        }
        else if (activityChanged)
        {
            notificationMessage = $"Daily activity targets updated: {form.WaterLitersTarget.ToString("0.#")}L Water, {form.StepsTarget} Steps.";
        }
        else
        {
            notificationMessage = "Daily targets updated.";
        }

        await _notificationService.CreateAndSendNotificationAsync(
            athleteUserId,
            NotificationType.MacroAlert,
            notificationMessage
        );
    }

    // ─── Private helpers ──────────────────────────────────────────────

    private async Task<Coach> GetCoachAsync()
    {
        var userId = LoggedInUser.Id;
        return await _coachRepo.Query()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new UnauthorizedAccessException("Coach profile not found.");
    }

    private async Task<List<Athlete>> GetCoachAthletesAsync(int coachId) =>
        await _athleteRepo.Query()
            .Include(a => a.User)
            .Where(a => a.AssignedCoachId == coachId)
            .ToListAsync();

    private async Task<List<int>> GetCoachAthleteIdsAsync(int coachId) =>
        await _athleteRepo.Query()
            .Where(a => a.AssignedCoachId == coachId)
            .Select(a => a.Id)
            .ToListAsync();

    private async Task EnsureAthleteInRosterAsync(int coachId, int athleteId)
    {
        if (LoggedInUser.Role == "Admin")
            return;

        var belongs = await _athleteRepo.Query()
            .AnyAsync(a => a.Id == athleteId && a.AssignedCoachId == coachId);

        if (!belongs)
            throw new UnauthorizedAccessException("Athlete does not belong to this coach's roster.");
    }
}
