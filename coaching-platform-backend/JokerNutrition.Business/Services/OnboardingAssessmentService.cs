using System.Security.Principal;
using System.Text.Json;
using JokerNutrition.Business.DTOs.Onboarding;
using JokerNutrition.Business.Forms.Onboarding;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public class OnboardingAssessmentService : _BaseService, IOnboardingAssessmentService
{
    private readonly JokerNutritionContext _context;
    private readonly INotificationService _notificationService;
    private readonly IBlobStorageService _blobService;
    private readonly IAuditLogService _auditLogService;
    private readonly ICacheService _cacheService;

    private const long MaxPhotoSizeBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/jpg"];

    public OnboardingAssessmentService(
        IPrincipal principal,
        ILogger<OnboardingAssessmentService> logger,
        JokerNutritionContext context,
        INotificationService notificationService,
        IBlobStorageService blobService,
        IAuditLogService auditLogService,
        ICacheService cacheService)
        : base(principal, logger)
    {
        _context = context;
        _notificationService = notificationService;
        _blobService = blobService;
        _auditLogService = auditLogService;
        _cacheService = cacheService;
    }

    public async Task<OnboardingAssessmentDto> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var athlete = await GetCurrentAthleteAsync(cancellationToken);
        return Map(athlete, athlete.OnboardingAssessment);
    }

    public async Task<OnboardingAssessmentDto> SaveDraftAsync(SaveOnboardingAssessmentForm form, CancellationToken cancellationToken = default)
    {
        var athlete = await GetCurrentAthleteAsync(cancellationToken);
        if (athlete.OnboardingAssessment?.Status is OnboardingAssessmentStatus.Submitted or OnboardingAssessmentStatus.Reviewed)
            throw new InvalidOperationException("A submitted assessment is read-only. Contact your coach if it needs to be reopened.");

        var assessment = UpsertAssessment(athlete, form);
        SyncAthleteProfile(athlete, assessment, onlyIfAssessmentHasValue: true);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Reload to ensure navigation properties/collections are updated
        var updatedAthlete = await GetCurrentAthleteAsync(cancellationToken);
        return Map(updatedAthlete, updatedAthlete.OnboardingAssessment);
    }

    public async Task<OnboardingAssessmentDto> SubmitAsync(SaveOnboardingAssessmentForm form, CancellationToken cancellationToken = default)
    {
        var athlete = await GetCurrentAthleteAsync(cancellationToken);
        if (athlete.OnboardingAssessment?.Status is OnboardingAssessmentStatus.Submitted or OnboardingAssessmentStatus.Reviewed)
            throw new InvalidOperationException("This assessment has already been submitted.");

        var assessment = UpsertAssessment(athlete, form);
        ValidateForSubmission(assessment);

        assessment.Status = OnboardingAssessmentStatus.Submitted;
        assessment.SubmittedAt = DateTime.UtcNow;
        assessment.ReviewedAt = null;
        assessment.ReviewedByCoachId = null;
        assessment.CoachReviewNotes = null;
        SyncAthleteProfile(athlete, assessment);
        await _context.SaveChangesAsync(cancellationToken);
        _cacheService.EvictByPrefix("coach-dashboard:");

        var recipientUserIds = new HashSet<int>();

        if (athlete.AssignedCoachId.HasValue)
        {
            var coachUserId = await _context.Coaches
                .Where(x => x.Id == athlete.AssignedCoachId.Value)
                .Select(x => (int?)x.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (coachUserId.HasValue)
            {
                recipientUserIds.Add(coachUserId.Value);
            }
        }

        var adminRoleId = await _context.Roles
            .Where(r => r.Name == "Admin")
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminRoleId > 0)
        {
            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);

            foreach (var adminId in adminUserIds)
            {
                recipientUserIds.Add(adminId);
            }
        }

        foreach (var recipientUserId in recipientUserIds)
        {
            await TrySendNotificationAsync(
                recipientUserId,
                NotificationType.OnboardingSubmitted,
                $"{athlete.User.FirstName} {athlete.User.LastName} submitted their onboarding assessment.");
        }

        // Reload to ensure navigation properties/collections are updated
        var updatedAthlete = await GetCurrentAthleteAsync(cancellationToken);
        return Map(updatedAthlete, updatedAthlete.OnboardingAssessment);
    }

    public async Task<OnboardingAssessmentDto> GetForAthleteAsync(int athleteId, CancellationToken cancellationToken = default)
    {
        var athlete = await GetAuthorizedAthleteAsync(athleteId, cancellationToken);
        if (athlete.OnboardingAssessment?.Status == OnboardingAssessmentStatus.Draft)
        {
            return new OnboardingAssessmentDto
            {
                Id = athlete.OnboardingAssessment.Id,
                AthleteId = athlete.Id,
                AthleteName = $"{athlete.User.FirstName} {athlete.User.LastName}".Trim(),
                Status = OnboardingAssessmentStatus.Draft,
                ReopenReason = athlete.OnboardingAssessment.ReopenReason,
                ReopenedAt = athlete.OnboardingAssessment.ReopenedAt,
                UpdatedAt = athlete.OnboardingAssessment.UpdatedAt
            };
        }

        return Map(athlete, athlete.OnboardingAssessment);
    }

    public async Task<OnboardingAssessmentDto> ReviewAsync(int athleteId, ReviewOnboardingAssessmentForm form, CancellationToken cancellationToken = default)
    {
        var athlete = await GetAuthorizedAthleteAsync(athleteId, cancellationToken);
        var assessment = athlete.OnboardingAssessment
            ?? throw new InvalidOperationException("The athlete has not started their onboarding assessment.");

        if (assessment.Status == OnboardingAssessmentStatus.Draft)
            throw new InvalidOperationException("The athlete must submit the assessment before it can be reviewed.");

        int? coachId = null;
        if (LoggedInUser.Role.Equals("Coach", StringComparison.OrdinalIgnoreCase))
        {
            coachId = await _context.Coaches
                .Where(x => x.UserId == LoggedInUser.Id)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new UnauthorizedAccessException("Coach profile not found.");
        }

        assessment.Status = OnboardingAssessmentStatus.Reviewed;
        assessment.CoachReviewNotes = Normalize(form.CoachReviewNotes);
        assessment.ReviewedAt = DateTime.UtcNow;
        assessment.ReviewedByCoachId = coachId;
        assessment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _cacheService.EvictByPrefix("coach-dashboard:");

        await TrySendNotificationAsync(
            athlete.UserId,
            NotificationType.OnboardingReviewed,
            "Your coach reviewed your onboarding assessment.");

        // Reload to ensure navigation properties/collections are updated
        var updatedAthlete = await GetAuthorizedAthleteAsync(athleteId, cancellationToken);
        return Map(updatedAthlete, updatedAthlete.OnboardingAssessment);
    }

    public async Task<OnboardingAssessmentDto> ReopenAsync(
        int athleteId,
        ReopenOnboardingAssessmentForm form,
        CancellationToken cancellationToken = default)
    {
        var athlete = await GetAuthorizedAthleteAsync(athleteId, cancellationToken);
        var assessment = athlete.OnboardingAssessment
            ?? throw new InvalidOperationException("The athlete has not started their onboarding assessment.");

        if (assessment.Status is not (OnboardingAssessmentStatus.Submitted or OnboardingAssessmentStatus.Reviewed))
            throw new InvalidOperationException("Only a submitted or reviewed assessment can be reopened.");

        var reason = Normalize(form.Reason)
            ?? throw new ArgumentException("A reason is required to reopen the assessment.");

        if (reason.Length < 10)
            throw new ArgumentException("The reopen reason must be at least 10 characters.");

        int? coachId = null;
        if (LoggedInUser.Role.Equals("Coach", StringComparison.OrdinalIgnoreCase))
        {
            coachId = await _context.Coaches
                .Where(x => x.UserId == LoggedInUser.Id)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new UnauthorizedAccessException("Coach profile not found.");
        }

        var previousState = JsonSerializer.Serialize(new
        {
            PreviousStatus = assessment.Status.ToString(),
            assessment.SubmittedAt,
            assessment.ReviewedAt,
            assessment.ReviewedByCoachId,
            assessment.CoachReviewNotes,
            ReopenReason = reason
        });

        assessment.Status = OnboardingAssessmentStatus.Draft;
        assessment.ReopenReason = reason;
        assessment.ReopenedAt = DateTime.UtcNow;
        assessment.ReopenedByCoachId = coachId;
        assessment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _cacheService.EvictByPrefix("coach-dashboard:");

        await _auditLogService.LogAsync(
            LoggedInUser.Id,
            LoggedInUser.FirstName,
            "OnboardingAssessmentReopened",
            nameof(AthleteOnboardingAssessment),
            assessment.Id.ToString(),
            details: previousState);

        await TrySendNotificationAsync(
            athlete.UserId,
            NotificationType.OnboardingReopened,
            $"Your onboarding assessment was reopened: \"{reason}\"");

        return new OnboardingAssessmentDto
        {
            Id = assessment.Id,
            AthleteId = athlete.Id,
            AthleteName = $"{athlete.User.FirstName} {athlete.User.LastName}".Trim(),
            Status = OnboardingAssessmentStatus.Draft,
            ReopenReason = reason,
            ReopenedAt = assessment.ReopenedAt,
            UpdatedAt = assessment.UpdatedAt
        };
    }

    public async Task<OnboardingAssessmentDto> UploadPhotosAsync(List<(PhotoAngle Angle, IFormFile File)> photos, CancellationToken cancellationToken = default)
    {
        var athlete = await GetCurrentAthleteAsync(cancellationToken);
        
        var assessment = athlete.OnboardingAssessment;
        if (assessment is null)
        {
            assessment = new AthleteOnboardingAssessment
            {
                AthleteId = athlete.Id,
                Athlete = athlete,
                Status = OnboardingAssessmentStatus.Draft
            };
            athlete.OnboardingAssessment = assessment;
            _context.AthleteOnboardingAssessments.Add(assessment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (assessment.Status is OnboardingAssessmentStatus.Submitted or OnboardingAssessmentStatus.Reviewed)
            throw new InvalidOperationException("A submitted assessment is read-only.");

        foreach (var (angle, file) in photos)
        {
            if (file.Length > MaxPhotoSizeBytes)
                throw new InvalidOperationException($"File for {angle} exceeds the 10 MB limit.");

            if (!AllowedContentTypes.Contains(file.ContentType.ToLower()))
                throw new InvalidOperationException($"File type '{file.ContentType}' is not allowed. Use JPEG or PNG.");

            // Delete old photo if same angle already exists
            var existing = assessment.Photos.FirstOrDefault(p => p.Angle == angle);
            if (existing is not null)
            {
                await _blobService.DeleteFileAsync(existing.BlobUrl);
                _context.OnboardingPhotos.Remove(existing);
            }

            // Upload new file
            var blobName = $"onboarding/{athlete.Id}/{angle.ToString().ToLower()}.jpg";
            string blobUrl;
            using (var stream = file.OpenReadStream())
            {
                blobUrl = await _blobService.UploadFileAsync(stream, blobName, file.ContentType);
            }

            // Insert new photo
            var newPhoto = new OnboardingPhoto
            {
                OnboardingAssessmentId = assessment.Id,
                Angle = angle,
                BlobUrl = blobUrl,
                UploadedAt = DateTime.UtcNow
            };
            _context.OnboardingPhotos.Add(newPhoto);
        }

        assessment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get Photos populated
        var updatedAthlete = await GetCurrentAthleteAsync(cancellationToken);
        return Map(updatedAthlete, updatedAthlete.OnboardingAssessment);
    }

    public async Task DeletePhotoAsync(PhotoAngle angle, CancellationToken cancellationToken = default)
    {
        var athlete = await GetCurrentAthleteAsync(cancellationToken);
        var assessment = athlete.OnboardingAssessment 
            ?? throw new KeyNotFoundException("Onboarding assessment not found.");

        if (assessment.Status is OnboardingAssessmentStatus.Submitted or OnboardingAssessmentStatus.Reviewed)
            throw new InvalidOperationException("A submitted assessment is read-only.");

        var photo = assessment.Photos.FirstOrDefault(p => p.Angle == angle)
            ?? throw new KeyNotFoundException($"No photo found for angle '{angle}' on onboarding assessment.");

        await _blobService.DeleteFileAsync(photo.BlobUrl);
        _context.OnboardingPhotos.Remove(photo);
        assessment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Athlete> GetCurrentAthleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Athletes
            .Include(x => x.User)
            .Include(x => x.OnboardingAssessment)
                .ThenInclude(oa => oa!.Photos)
            .FirstOrDefaultAsync(x => x.UserId == LoggedInUser.Id, cancellationToken)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");
    }

    private async Task<Athlete> GetAuthorizedAthleteAsync(int athleteId, CancellationToken cancellationToken = default)
    {
        var athlete = await _context.Athletes
            .Include(x => x.User)
            .Include(x => x.OnboardingAssessment)
                .ThenInclude(oa => oa!.Photos)
            .FirstOrDefaultAsync(x => x.Id == athleteId, cancellationToken)
            ?? throw new KeyNotFoundException("Athlete profile not found.");

        if (LoggedInUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            return athlete;

        if (!LoggedInUser.Role.Equals("Coach", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Only coaches and admins can review athlete assessments.");

        var coachId = await _context.Coaches
            .Where(x => x.UserId == LoggedInUser.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Coach profile not found.");

        if (athlete.AssignedCoachId != coachId)
            throw new UnauthorizedAccessException("This athlete is outside your roster.");

        return athlete;
    }

    private AthleteOnboardingAssessment UpsertAssessment(Athlete athlete, SaveOnboardingAssessmentForm form)
    {
        var assessment = athlete.OnboardingAssessment;
        if (assessment is null)
        {
            assessment = new AthleteOnboardingAssessment
            {
                AthleteId = athlete.Id,
                Athlete = athlete,
                Status = OnboardingAssessmentStatus.Draft
            };
            athlete.OnboardingAssessment = assessment;
            _context.AthleteOnboardingAssessments.Add(assessment);
        }

        assessment.PrimaryGoal = Normalize(form.PrimaryGoal);
        assessment.WeightKg = form.WeightKg;
        assessment.HeightCm = form.HeightCm;
        assessment.ActivityLevel = Normalize(form.ActivityLevel);
        assessment.TrainingExperience = Normalize(form.TrainingExperience);
        assessment.TrainingDaysPerWeek = form.TrainingDaysPerWeek;
        assessment.AvailableEquipmentJson = JsonSerializer.Serialize(CleanList(form.AvailableEquipment));
        assessment.PreferredTrainingDaysJson = JsonSerializer.Serialize(CleanList(form.PreferredTrainingDays));
        assessment.InjuriesOrLimitations = Normalize(form.InjuriesOrLimitations);
        assessment.CurrentPain = Normalize(form.CurrentPain);
        assessment.AverageSleepHours = form.AverageSleepHours;
        assessment.SleepQuality = Normalize(form.SleepQuality);
        assessment.FoodAllergies = Normalize(form.FoodAllergies);
        assessment.FoodIntolerances = Normalize(form.FoodIntolerances);
        assessment.PreferredFoods = Normalize(form.PreferredFoods);
        assessment.FoodsToAvoid = Normalize(form.FoodsToAvoid);
        assessment.TypicalMealsPerDay = form.TypicalMealsPerDay;
        assessment.TypicalMealSchedule = Normalize(form.TypicalMealSchedule);
        assessment.CurrentSupplements = Normalize(form.CurrentSupplements);
        assessment.AdditionalNotes = Normalize(form.AdditionalNotes);
        assessment.UpdatedAt = DateTime.UtcNow;
        return assessment;
    }

    private static void ValidateForSubmission(AthleteOnboardingAssessment assessment)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(assessment.PrimaryGoal)) missing.Add("primary goal");
        if (!assessment.WeightKg.HasValue) missing.Add("weight");
        if (!assessment.HeightCm.HasValue) missing.Add("height");
        if (string.IsNullOrWhiteSpace(assessment.ActivityLevel)) missing.Add("activity level");
        if (string.IsNullOrWhiteSpace(assessment.TrainingExperience)) missing.Add("training experience");
        if (!assessment.TrainingDaysPerWeek.HasValue) missing.Add("training days per week");
        if (!assessment.AverageSleepHours.HasValue) missing.Add("average sleep hours");
        if (string.IsNullOrWhiteSpace(assessment.SleepQuality)) missing.Add("sleep quality");
        if (!assessment.TypicalMealsPerDay.HasValue) missing.Add("typical meals per day");

        if (missing.Count > 0)
            throw new ArgumentException($"Complete the required fields before submitting: {string.Join(", ", missing)}.");
    }

    private static void SyncAthleteProfile(
        Athlete athlete,
        AthleteOnboardingAssessment assessment,
        bool onlyIfAssessmentHasValue = false)
    {
        if (!onlyIfAssessmentHasValue || assessment.WeightKg.HasValue) athlete.WeightKg = assessment.WeightKg;
        if (!onlyIfAssessmentHasValue || assessment.HeightCm.HasValue) athlete.HeightCm = assessment.HeightCm;
        if (!onlyIfAssessmentHasValue || !string.IsNullOrWhiteSpace(assessment.PrimaryGoal)) athlete.TargetGoal = assessment.PrimaryGoal;
    }

    private async Task TrySendNotificationAsync(int userId, NotificationType type, string message)
    {
        try
        {
            await _notificationService.CreateAndSendNotificationAsync(userId, type, message);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Onboarding state was saved, but notification delivery failed for user {UserId}.", userId);
        }
    }

    private static OnboardingAssessmentDto Map(Athlete athlete, AthleteOnboardingAssessment? assessment)
    {
        if (assessment is null)
        {
            return new OnboardingAssessmentDto
            {
                AthleteId = athlete.Id,
                AthleteName = $"{athlete.User.FirstName} {athlete.User.LastName}".Trim(),
                Status = OnboardingAssessmentStatus.NotStarted,
                WeightKg = athlete.WeightKg,
                HeightCm = athlete.HeightCm,
                PrimaryGoal = athlete.TargetGoal
            };
        }

        return new OnboardingAssessmentDto
        {
            Id = assessment.Id,
            AthleteId = athlete.Id,
            AthleteName = $"{athlete.User.FirstName} {athlete.User.LastName}".Trim(),
            Status = assessment.Status,
            PrimaryGoal = assessment.PrimaryGoal,
            WeightKg = assessment.WeightKg,
            HeightCm = assessment.HeightCm,
            ActivityLevel = assessment.ActivityLevel,
            TrainingExperience = assessment.TrainingExperience,
            TrainingDaysPerWeek = assessment.TrainingDaysPerWeek,
            AvailableEquipment = ParseList(assessment.AvailableEquipmentJson),
            PreferredTrainingDays = ParseList(assessment.PreferredTrainingDaysJson),
            InjuriesOrLimitations = assessment.InjuriesOrLimitations,
            CurrentPain = assessment.CurrentPain,
            AverageSleepHours = assessment.AverageSleepHours,
            SleepQuality = assessment.SleepQuality,
            FoodAllergies = assessment.FoodAllergies,
            FoodIntolerances = assessment.FoodIntolerances,
            PreferredFoods = assessment.PreferredFoods,
            FoodsToAvoid = assessment.FoodsToAvoid,
            TypicalMealsPerDay = assessment.TypicalMealsPerDay,
            TypicalMealSchedule = assessment.TypicalMealSchedule,
            CurrentSupplements = assessment.CurrentSupplements,
            AdditionalNotes = assessment.AdditionalNotes,
            CoachReviewNotes = assessment.CoachReviewNotes,
            SubmittedAt = assessment.SubmittedAt,
            ReviewedAt = assessment.ReviewedAt,
            ReopenReason = assessment.ReopenReason,
            ReopenedAt = assessment.ReopenedAt,
            UpdatedAt = assessment.UpdatedAt,
            HasInjuryFlag = HasSafetyValue(assessment.InjuriesOrLimitations),
            HasPainFlag = HasSafetyValue(assessment.CurrentPain),
            HasAllergyFlag = HasSafetyValue(assessment.FoodAllergies),
            HasFoodRestrictionFlag = HasSafetyValue(assessment.FoodIntolerances) || HasSafetyValue(assessment.FoodsToAvoid),
            Photos = assessment.Photos?.Select(p => new OnboardingPhotoDto
            {
                Id = p.Id,
                Angle = p.Angle.ToString(),
                SignedDownloadUrl = p.BlobUrl,
                UploadedAt = p.UploadedAt
            }).ToList() ?? []
        };
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> CleanList(IEnumerable<string>? values) =>
        values?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().Take(30).ToList() ?? [];

    private static List<string> ParseList(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch (JsonException) { return []; }
    }

    private static bool HasSafetyValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = value.Trim().ToLowerInvariant();
        return normalized is not ("no" or "none" or "n/a" or "na" or "لا" or "لا يوجد");
    }
}
