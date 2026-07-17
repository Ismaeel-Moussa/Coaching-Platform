using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Athletes;
using JokerNutrition.Business.DTOs.Coach;
using JokerNutrition.Business.Forms.Coach;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface ICoachHubService
{
    Task<CoachDashboardDto> GetDashboardAsync();
    Task<PagedResult<CoachActionItemDto>> GetActionItemsAsync(
        int page,
        int pageSize,
        string? type,
        string? priority,
        string? search);
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
    private readonly ICacheService _cacheService;
    private readonly JokerNutritionContext _context;

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
        INotificationService notificationService,
        ICacheService cacheService,
        JokerNutritionContext context)
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
        _cacheService = cacheService;
        _context = context;
    }

    // ─── Dashboard ────────────────────────────────────────────────────

    public async Task<CoachDashboardDto> GetDashboardAsync()
    {
        var isAdmin = LoggedInUser.Role == "Admin";
        var coach = isAdmin ? null : await GetCoachAsync();
        string cacheKey = isAdmin
            ? $"coach-dashboard:admin:{LoggedInUser.Id}"
            : $"coach-dashboard:{coach!.Id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var athletes = isAdmin
                ? await _athleteRepo.QueryAll()
                    .Include(a => a.User)
                    .Include(a => a.OnboardingAssessment)
                    .ToListAsync()
                : await GetCoachAthletesAsync(coach!.Id);
            var athleteIds = athletes.Select(a => a.Id).ToList();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Active athlete count
            var activeCount = athletes.Count;

            // Average workout completion % for the past 7 days
            var weekAgo = today.AddDays(-7);
            var recentLogs = await _workoutLogRepo.QueryAll()
                .Where(w => athleteIds.Contains(w.AthleteId) && w.Date >= weekAgo)
                .ToListAsync();

            double avgCompletion = 0;
            if (recentLogs.Any())
            {
                avgCompletion = recentLogs.Count(w => w.Status == WorkoutStatus.Completed)
                                / (double)recentLogs.Count * 100.0;
            }

            // Pending check-ins: athletes with no submission for the current ISO week.
            var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            var currentWeekMonday = today.AddDays(-daysSinceMonday);
            var submittedThisWeekAthleteIds = await _checkInRepo.QueryAll()
                .Where(c => athleteIds.Contains(c.AthleteId) && c.WeekOf == currentWeekMonday)
                .Select(c => c.AthleteId)
                .Distinct()
                .ToListAsync();

            var pendingCount = athleteIds.Count - submittedThisWeekAthleteIds.Count;
            var pendingOnboardingAssessmentsCount = athletes.Count(a =>
                a.OnboardingAssessment?.Status == OnboardingAssessmentStatus.Submitted);

            var activeProgramAthleteIds = await _context.ClientPrograms.AsNoTracking()
                .Where(p => athleteIds.Contains(p.AthleteId) && p.IsActive)
                .Select(p => p.AthleteId)
                .Distinct()
                .ToListAsync();
            var now = DateTime.UtcNow;
            var activeNutritionPlanAthleteIds = await _context.NutritionPlanAssignments.AsNoTracking()
                .Where(a => athleteIds.Contains(a.AthleteId) && a.IsActive &&
                            a.StartDate <= now && (a.EndDate == null || a.EndDate > now))
                .Select(a => a.AthleteId)
                .Distinct()
                .ToListAsync();
            var activeTargets = await _context.MacroTargets.AsNoTracking()
                .Where(t => athleteIds.Contains(t.AthleteId) && t.IsActive)
                .ToListAsync();

            var programSet = activeProgramAthleteIds.ToHashSet();
            var nutritionPlanSet = activeNutritionPlanAthleteIds.ToHashSet();
            var targetByAthlete = activeTargets
                .GroupBy(t => t.AthleteId)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(t => t.SetAt).First());
            var athletesNeedingSetupCount = athletes.Count(athlete =>
            {
                targetByAthlete.TryGetValue(athlete.Id, out var activeTarget);
                return !CoachHubMapper.MapSetupReadiness(
                    athlete,
                    programSet.Contains(athlete.Id),
                    nutritionPlanSet.Contains(athlete.Id),
                    activeTarget).IsComplete;
            });

            var todayCalorieTotals = await _diaryRepo.QueryAll()
                .AsNoTracking()
                .Where(d => athleteIds.Contains(d.AthleteId) && d.Date == today)
                .Select(d => new
                {
                    d.AthleteId,
                    ConsumedCalories = d.MealLogs.Sum(m => (decimal?)m.Calories) ?? 0m
                })
                .ToListAsync();
            var caloriesByAthlete = todayCalorieTotals.ToDictionary(d => d.AthleteId, d => d.ConsumedCalories);
            var allActions = BuildActionItems(
                athletes,
                programSet,
                nutritionPlanSet,
                targetByAthlete,
                submittedThisWeekAthleteIds.ToHashSet(),
                caloriesByAthlete);
            var prioritizedActions = allActions
                .GroupBy(item => item.Item.AthleteId)
                .Select(group => group.OrderBy(item => item.Rank).First())
                .OrderBy(item => item.Rank)
                .ThenBy(item => item.Item.AthleteName)
                .Take(10)
                .Select(item => item.Item)
                .ToList();

            // Last 10 live feed items
            var recentFeed = await _workoutLogRepo.QueryAll()
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
                PendingOnboardingAssessmentsCount = pendingOnboardingAssessmentsCount,
                AthletesNeedingSetupCount = athletesNeedingSetupCount,
                ActionItems = prioritizedActions,
                RecentFeed = recentFeed.Select(CoachHubMapper.MapLiveFeedItem).ToList()
            };
        }, TimeSpan.FromSeconds(60));
    }

    // ─── Coach Tasks ────────────────────────────────────────────────────────

    public async Task<PagedResult<CoachActionItemDto>> GetActionItemsAsync(
        int page,
        int pageSize,
        string? type,
        string? priority,
        string? search)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var isAdmin = LoggedInUser.Role == "Admin";
        var coach = isAdmin ? null : await GetCoachAsync();
        var athletes = isAdmin
            ? await _athleteRepo.QueryAll()
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.OnboardingAssessment)
                .ToListAsync()
            : await GetCoachAthletesAsync(coach!.Id);
        var athleteIds = athletes.Select(a => a.Id).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var currentWeekMonday = today.AddDays(-daysSinceMonday);
        var now = DateTime.UtcNow;

        var submittedThisWeek = (await _checkInRepo.QueryAll()
            .AsNoTracking()
            .Where(c => athleteIds.Contains(c.AthleteId) && c.WeekOf == currentWeekMonday)
            .Select(c => c.AthleteId)
            .Distinct()
            .ToListAsync()).ToHashSet();
        var programSet = (await _context.ClientPrograms.AsNoTracking()
            .Where(p => athleteIds.Contains(p.AthleteId) && p.IsActive)
            .Select(p => p.AthleteId)
            .Distinct()
            .ToListAsync()).ToHashSet();
        var nutritionPlanSet = (await _context.NutritionPlanAssignments.AsNoTracking()
            .Where(a => athleteIds.Contains(a.AthleteId) && a.IsActive &&
                        a.StartDate <= now && (a.EndDate == null || a.EndDate > now))
            .Select(a => a.AthleteId)
            .Distinct()
            .ToListAsync()).ToHashSet();
        var activeTargets = await _context.MacroTargets.AsNoTracking()
            .Where(t => athleteIds.Contains(t.AthleteId) && t.IsActive)
            .ToListAsync();
        var targetByAthlete = activeTargets
            .GroupBy(t => t.AthleteId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(t => t.SetAt).First());
        var calorieTotals = await _diaryRepo.QueryAll()
            .AsNoTracking()
            .Where(d => athleteIds.Contains(d.AthleteId) && d.Date == today)
            .Select(d => new
            {
                d.AthleteId,
                ConsumedCalories = d.MealLogs.Sum(m => (decimal?)m.Calories) ?? 0m
            })
            .ToListAsync();
        var caloriesByAthlete = calorieTotals.ToDictionary(d => d.AthleteId, d => d.ConsumedCalories);

        var actions = BuildActionItems(
                athletes,
                programSet,
                nutritionPlanSet,
                targetByAthlete,
                submittedThisWeek,
                caloriesByAthlete)
            .Where(item => string.IsNullOrWhiteSpace(type) ||
                           item.Item.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(priority) ||
                           item.Item.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(search) ||
                           item.Item.AthleteName.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Rank)
            .ThenBy(item => item.Item.AthleteName)
            .ToList();

        return new PagedResult<CoachActionItemDto>
        {
            Items = actions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(item => item.Item),
            TotalCount = actions.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    // ─── Live Feed ────────────────────────────────────────────────────

    public async Task<PagedResult<LiveFeedItemDto>> GetLiveFeedAsync(int page, int pageSize)
    {
        var coach = await GetCoachAsync();
        var athleteIds = LoggedInUser.Role == "Admin"
            ? await _athleteRepo.QueryAll().Select(a => a.Id).ToListAsync()
            : await GetCoachAthleteIdsAsync(coach.Id);

        var query = _workoutLogRepo.QueryAll()
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
        string cacheKey = $"coach-compliance:{coach.Id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var athletes = LoggedInUser.Role == "Admin"
                ? await _athleteRepo.QueryAll().Include(a => a.User).ToListAsync()
                : await GetCoachAthletesAsync(coach.Id);
            var athleteIds = athletes.Select(a => a.Id).ToList();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Load today's diaries with meal logs
            var diaries = await _diaryRepo.QueryAll()
                .Include(d => d.MealLogs)
                .Where(d => athleteIds.Contains(d.AthleteId) && d.Date == today)
                .ToListAsync();

            // Load active macro targets
            var targets = await _macroTargetRepo.QueryAll()
                .Where(t => athleteIds.Contains(t.AthleteId) && t.IsActive)
                .ToListAsync();

            var result = athletes.Select(athlete =>
            {
                var diary = diaries.FirstOrDefault(d => d.AthleteId == athlete.Id);
                var target = targets.FirstOrDefault(t => t.AthleteId == athlete.Id);
                return CoachHubMapper.MapComplianceItem(athlete, diary, target);
            }).ToList();

            return result;
        }, TimeSpan.FromSeconds(60));
    }

    // ─── Roster ───────────────────────────────────────────────────────

    public async Task<PagedResult<RosterItemDto>> GetRosterAsync(int page, int pageSize, string? filter)
    {
        var isAdmin = LoggedInUser.Role == "Admin";
        var coach = isAdmin ? null : await GetCoachAsync();
        IQueryable<Athlete> athleteQuery = _athleteRepo.QueryAll()
            .Include(a => a.User)
            .Include(a => a.OnboardingAssessment);

        if (!isAdmin)
        {
            athleteQuery = athleteQuery.Where(a => a.AssignedCoachId == coach!.Id);
        }

        var isAssessmentReviewFilter = filter == "AwaitingAssessmentReview";
        var isSetupRequiredFilter = filter == "SetupRequired";
        int? assessmentReviewTotalCount = null;
        List<Athlete> athletes;

        if (isAssessmentReviewFilter)
        {
            var pendingAssessmentQuery = athleteQuery
                .Where(a => a.OnboardingAssessment != null &&
                            a.OnboardingAssessment.Status == OnboardingAssessmentStatus.Submitted)
                .OrderBy(a => a.OnboardingAssessment!.SubmittedAt);

            assessmentReviewTotalCount = await pendingAssessmentQuery.CountAsync();
            athletes = await pendingAssessmentQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        else if (isSetupRequiredFilter)
        {
            var now = DateTime.UtcNow;
            var setupRequiredQuery = athleteQuery
                .Where(a => a.OnboardingAssessment == null ||
                            a.OnboardingAssessment.Status != OnboardingAssessmentStatus.Reviewed ||
                            !_context.ClientPrograms.Any(p => p.AthleteId == a.Id && p.IsActive) ||
                            !_context.NutritionPlanAssignments.Any(n => n.AthleteId == a.Id && n.IsActive &&
                                n.StartDate <= now && (n.EndDate == null || n.EndDate > now)) ||
                            !_context.MacroTargets.Any(t => t.AthleteId == a.Id && t.IsActive &&
                                t.TargetCalories > 0 && t.TargetProtein > 0 && t.TargetCarbs > 0 && t.TargetFat > 0) ||
                            !_context.MacroTargets.Any(t => t.AthleteId == a.Id && t.IsActive &&
                                t.WaterLitersTarget > 0 && t.StepsTarget > 0))
                .OrderBy(a => a.User.FirstName)
                .ThenBy(a => a.User.LastName);

            assessmentReviewTotalCount = await setupRequiredQuery.CountAsync();
            athletes = await setupRequiredQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        else
        {
            athletes = await athleteQuery.ToListAsync();
        }
        var athleteIds = athletes.Select(a => a.Id).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Load active programs (AsNoTracking)
        var programs = await _clientProgramRepo.QueryAll()
            .Include(p => p.WorkoutTemplate)
            .Where(p => athleteIds.Contains(p.AthleteId) && p.IsActive)
            .ToListAsync();

        // Load last check-ins per athlete (AsNoTracking)
        var allCheckIns = await _checkInRepo.QueryAll()
            .Where(c => athleteIds.Contains(c.AthleteId))
            .ToListAsync();

        // Load compliance data directly to avoid duplicate coach/athlete queries
        var diaries = await _diaryRepo.QueryAll()
            .Include(d => d.MealLogs)
            .Where(d => athleteIds.Contains(d.AthleteId) && d.Date == today)
            .ToListAsync();

        var targets = await _macroTargetRepo.QueryAll()
            .Where(t => athleteIds.Contains(t.AthleteId) && t.IsActive)
            .OrderByDescending(t => t.SetAt)
            .ToListAsync();

        var nowUtc = DateTime.UtcNow;
        var nutritionPlanAthleteIds = (await _context.NutritionPlanAssignments.AsNoTracking()
            .Where(a => athleteIds.Contains(a.AthleteId) && a.IsActive &&
                        a.StartDate <= nowUtc && (a.EndDate == null || a.EndDate > nowUtc))
            .Select(a => a.AthleteId)
            .Distinct()
            .ToListAsync()).ToHashSet();

        // Build roster items
        var rosterItems = athletes.Select(athlete =>
        {
            var program = programs.FirstOrDefault(p => p.AthleteId == athlete.Id);
            var lastCheckIn = allCheckIns
                .Where(c => c.AthleteId == athlete.Id)
                .OrderByDescending(c => c.SubmittedAt)
                .FirstOrDefault();

            var diary = diaries.FirstOrDefault(d => d.AthleteId == athlete.Id);
            var target = targets.FirstOrDefault(t => t.AthleteId == athlete.Id);
            var targetCalories = target?.TargetCalories ?? 0m;
            var consumed = diary != null ? diary.MealLogs.Sum(m => m.Calories) : 0m;
            double compliance = 0;
            if (targetCalories > 0)
            {
                compliance = (double)(consumed / targetCalories) * 100.0;
            }

            return CoachHubMapper.MapRosterItem(
                athlete,
                program,
                lastCheckIn,
                compliance,
                nutritionPlanAthleteIds.Contains(athlete.Id),
                target);
        }).ToList();

        // Apply filter
        var filtered = filter switch
        {
            "ComplianceAlert" => rosterItems.Where(r => r.Status == "ComplianceAlert").ToList(),
            "NoRecentCheckIn" => rosterItems.Where(r => r.Status == "NoRecentCheckIn").ToList(),
            "AwaitingAssessmentReview" => rosterItems,
            "SetupRequired" => rosterItems,
            _ => rosterItems
        };

        var totalCount = assessmentReviewTotalCount ?? filtered.Count;
        var paged = isAssessmentReviewFilter || isSetupRequiredFilter
            ? filtered
            : filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

        var athlete = await _athleteRepo.QueryAll()
            .Include(a => a.User)
            .Include(a => a.OnboardingAssessment)
            .FirstAsync(a => a.Id == athleteId);

        // Current macro targets
        var target = await _macroTargetRepo.QueryAll()
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

        var hasActiveProgram = await _context.ClientPrograms.AsNoTracking()
            .AnyAsync(p => p.AthleteId == athleteId && p.IsActive);
        var now = DateTime.UtcNow;
        var hasActiveNutritionPlan = await _context.NutritionPlanAssignments.AsNoTracking()
            .AnyAsync(a => a.AthleteId == athleteId && a.IsActive &&
                           a.StartDate <= now && (a.EndDate == null || a.EndDate > now));

        // Weight history from check-ins
        var checkIns = await _checkInRepo.QueryAll()
            .Where(c => c.AthleteId == athleteId)
            .OrderBy(c => c.WeekOf)
            .ToListAsync();

        // Feedback notes
        var notes = await _feedbackNoteRepo.QueryAll()
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
            SetupReadiness = CoachHubMapper.MapSetupReadiness(
                athlete,
                hasActiveProgram,
                hasActiveNutritionPlan,
                target),
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

        var athleteUserId = await _athleteRepo.QueryAll()
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

        var checkIns = await _checkInRepo.QueryAll()
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

        // Evict cached dashboard and compliance for this coach
        _cacheService.EvictByPrefix("coach-dashboard:");
        _cacheService.Evict($"coach-compliance:{coach.Id}");

        // Notify Athlete
        var athleteUserId = await _athleteRepo.QueryAll()
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

    private static List<(int Rank, CoachActionItemDto Item)> BuildActionItems(
        IEnumerable<Athlete> athletes,
        IReadOnlySet<int> programSet,
        IReadOnlySet<int> nutritionPlanSet,
        IReadOnlyDictionary<int, MacroTarget> targetByAthlete,
        IReadOnlySet<int> submittedThisWeek,
        IReadOnlyDictionary<int, decimal> caloriesByAthlete)
    {
        var actions = new List<(int Rank, CoachActionItemDto Item)>();

        foreach (var athlete in athletes)
        {
            var athleteName = $"{athlete.User.FirstName} {athlete.User.LastName}".Trim();
            targetByAthlete.TryGetValue(athlete.Id, out var activeTarget);
            var readiness = CoachHubMapper.MapSetupReadiness(
                athlete,
                programSet.Contains(athlete.Id),
                nutritionPlanSet.Contains(athlete.Id),
                activeTarget);

            void AddAction(int rank, CoachActionItemDto action)
            {
                action.AthleteId = athlete.Id;
                action.AthleteName = athleteName;
                action.AthleteAvatarUrl = athlete.User.ProfilePictureUrl;
                actions.Add((rank, action));
            }

            if (athlete.OnboardingAssessment?.Status == OnboardingAssessmentStatus.Submitted)
            {
                AddAction(1, new CoachActionItemDto
                {
                    Type = "AssessmentReview",
                    Priority = "High"
                });
            }

            if (!readiness.IsComplete)
            {
                AddAction(2, new CoachActionItemDto
                {
                    Type = "SetupRequired",
                    Priority = "High",
                    ProgressCurrent = readiness.CompletedRequiredSteps,
                    ProgressTotal = readiness.TotalRequiredSteps
                });
            }

            if (readiness.IsComplete && !submittedThisWeek.Contains(athlete.Id))
            {
                AddAction(3, new CoachActionItemDto
                {
                    Type = "CheckInPending",
                    Priority = "Medium"
                });
            }

            if (readiness.IsComplete && activeTarget is { TargetCalories: > 0 } &&
                caloriesByAthlete.TryGetValue(athlete.Id, out var consumedCalories))
            {
                var compliancePercent = (double)(consumedCalories / activeTarget.TargetCalories) * 100.0;
                if (compliancePercent > 105.0)
                {
                    AddAction(4, new CoachActionItemDto
                    {
                        Type = "ComplianceAlert",
                        Priority = "Medium",
                        MetricValue = Math.Round(compliancePercent, 1)
                    });
                }
            }
        }

        return actions;
    }

    private async Task<Coach> GetCoachAsync()
    {
        var userId = LoggedInUser.Id;
        return await _coachRepo.QueryAll()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new UnauthorizedAccessException("Coach profile not found.");
    }

    private async Task<List<Athlete>> GetCoachAthletesAsync(int coachId) =>
        await _athleteRepo.QueryAll()
            .Include(a => a.User)
            .Include(a => a.OnboardingAssessment)
            .Where(a => a.AssignedCoachId == coachId)
            .ToListAsync();

    private async Task<List<int>> GetCoachAthleteIdsAsync(int coachId) =>
        await _athleteRepo.QueryAll()
            .Where(a => a.AssignedCoachId == coachId)
            .Select(a => a.Id)
            .ToListAsync();

    private async Task EnsureAthleteInRosterAsync(int coachId, int athleteId)
    {
        if (LoggedInUser.Role == "Admin")
            return;

        var belongs = await _athleteRepo.QueryAll()
            .AnyAsync(a => a.Id == athleteId && a.AssignedCoachId == coachId);

        if (!belongs)
            throw new UnauthorizedAccessException("Athlete does not belong to this coach's roster.");
    }
}
