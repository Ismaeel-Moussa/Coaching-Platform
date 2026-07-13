using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.CheckIns;
using JokerNutrition.Business.Forms.CheckIns;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface ICheckInService
{
    /// <summary>Upsert — creates or updates this week's check-in record.</summary>
    Task<CheckInDto> SubmitCheckInAsync(SubmitCheckInForm form);

    /// <summary>Upload 1-3 photos as multipart files. Replaces existing blob for the same angle.</summary>
    Task<CheckInDto> UploadPhotosAsync(int checkInId, List<(PhotoAngle Angle, IFormFile File)> photos);

    /// <summary>Delete a single photo angle from a check-in (blob + DB row).</summary>
    Task DeletePhotoAsync(int checkInId, PhotoAngle angle);

    /// <summary>Paginated check-in history for an athlete (by explicit ID — for coach use).</summary>
    Task<PagedResult<CheckInDto>> GetCheckInHistoryAsync(int athleteId, BasePaginationForm pagination);

    /// <summary>Paginated check-in history for the currently logged-in athlete.</summary>
    Task<PagedResult<CheckInDto>> GetMyCheckInHistoryAsync(BasePaginationForm pagination);

    /// <summary>Athletes under the coach's roster with no check-in this week.</summary>
    Task<PagedResult<PendingCheckInDto>> GetPendingCheckInsAsync(BasePaginationForm pagination);

    /// <summary>Coach saves feedback notes on an athlete's check-in.</summary>
    Task<CheckInDto> AddCoachNotesAsync(int checkInId, AddCoachNotesForm form);

    /// <summary>Returns signed 24h download URLs for all photos on a check-in.</summary>
    Task<List<CheckInPhotoDto>> GetCheckInPhotosAsync(int checkInId);

    /// <summary>Get a single check-in by ID (with permissions check).</summary>
    Task<CheckInDto> GetCheckInByIdAsync(int checkInId);
}

public class CheckInService : _BaseService, ICheckInService
{
    private readonly IClientCheckInRepository _checkInRepo;
    private readonly ICheckInPhotoRepository _photoRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly ICoachRepository _coachRepo;
    private readonly IBlobStorageService _blobService;
    private readonly INotificationService _notificationService;

    private const long MaxPhotoSizeBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/jpg"];

    public CheckInService(
        IPrincipal principal,
        ILogger<CheckInService> logger,
        IClientCheckInRepository checkInRepo,
        ICheckInPhotoRepository photoRepo,
        IAthleteRepository athleteRepo,
        ICoachRepository coachRepo,
        IBlobStorageService blobService,
        INotificationService notificationService)
        : base(principal, logger)
    {
        _checkInRepo = checkInRepo;
        _photoRepo = photoRepo;
        _athleteRepo = athleteRepo;
        _coachRepo = coachRepo;
        _blobService = blobService;
        _notificationService = notificationService;
    }

    // ─── Submit / Upsert ──────────────────────────────────────────────

    public async Task<CheckInDto> SubmitCheckInAsync(SubmitCheckInForm form)
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new KeyNotFoundException("Athlete profile not found.");

        // Monday of the current ISO week
        var weekOf = GetCurrentWeekMonday();

        // Upsert: find existing or create new
        var existing = await _checkInRepo.Query()
            .Include(ci => ci.Photos)
            .FirstOrDefaultAsync(ci => ci.AthleteId == athlete.Id && ci.WeekOf == weekOf);

        bool isNew = existing is null;

        if (isNew)
        {
            existing = new ClientCheckIn
            {
                AthleteId = athlete.Id,
                Athlete = athlete,
                WeekOf = weekOf,
                SubmittedAt = DateTime.UtcNow
            };
        }
        else
        {
            existing!.SubmittedAt = DateTime.UtcNow;
            existing.CoachNotes = null;
            existing.CoachReviewedAt = null;
        }

        // Apply form values
        existing.WeightKg = form.WeightKg;
        existing.WaistCm = form.WaistCm;
        existing.ChestCm = form.ChestCm;
        existing.ThighCm = form.ThighCm;
        existing.SleepQuality = form.SleepQuality;
        existing.EnergyLevel = form.EnergyLevel;
        existing.GutHealth = form.GutHealth;
        existing.TrainingStress = form.TrainingStress;

        if (isNew)
        {
            await _checkInRepo.CreateAsync(existing);
        }
        else
        {
            _checkInRepo.Update(existing);
        }

        await _checkInRepo.SaveChangesAsync();

        // Notify the coach on submission or update
        if (athlete.AssignedCoachId.HasValue)
        {
            var coach = await _coachRepo.Query()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == athlete.AssignedCoachId.Value);

            if (coach is not null)
            {
                var athleteName = $"{athlete.User.FirstName} {athlete.User.LastName}";
                var notificationMsg = isNew
                    ? $"{athleteName} submitted their weekly check-in."
                    : $"{athleteName} updated their weekly check-in.";

                await _notificationService.CreateAndSendNotificationAsync(
                    coach.UserId,
                    NotificationType.CheckInSubmitted,
                    notificationMsg);
            }
        }

        var photoDtos = await BuildPhotoDtosAsync(existing.Id);
        return CheckInMapper.Map(existing, photoDtos);
    }

    // ─── Upload Photos ────────────────────────────────────────────────

    public async Task<CheckInDto> UploadPhotosAsync(int checkInId, List<(PhotoAngle Angle, IFormFile File)> photos)
    {
        var userId = LoggedInUser.Id;
        var checkIn = await GetCheckInForAthleteAsync(checkInId, userId);

        foreach (var (angle, file) in photos)
        {
            // Validate
            if (file.Length > MaxPhotoSizeBytes)
                throw new InvalidOperationException($"File for {angle} exceeds the 10 MB limit.");

            if (!AllowedContentTypes.Contains(file.ContentType.ToLower()))
                throw new InvalidOperationException($"File type '{file.ContentType}' is not allowed. Use JPEG or PNG.");

            // Delete old blob + DB row if same angle already exists
            var existing = await _photoRepo.Query()
                .FirstOrDefaultAsync(p => p.ClientCheckInId == checkInId && p.Angle == angle);

            if (existing is not null)
            {
                await _blobService.DeleteFileAsync(existing.BlobUrl);
                _photoRepo.Delete(existing);
                await _photoRepo.SaveChangesAsync();
            }

            // Upload new file
            var blobName = $"checkins/{checkInId}/{angle.ToString().ToLower()}.jpg";
            string blobUrl;
            using (var stream = file.OpenReadStream())
            {
                blobUrl = await _blobService.UploadFileAsync(stream, blobName, file.ContentType);
            }

            // Insert new DB row
            var newPhoto = new CheckInPhoto
            {
                ClientCheckInId = checkInId,
                Angle = angle,
                BlobUrl = blobUrl,
                UploadedAt = DateTime.UtcNow
            };

            await _photoRepo.CreateAsync(newPhoto);
        }

        await _photoRepo.SaveChangesAsync();

        // Reload check-in with athlete for mapper
        var updated = await _checkInRepo.Query()
            .Include(ci => ci.Athlete).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(ci => ci.Id == checkInId)!;

        var photoDtos = await BuildPhotoDtosAsync(checkInId);
        return CheckInMapper.Map(updated!, photoDtos);
    }

    // ─── Delete Photo ─────────────────────────────────────────────────

    public async Task DeletePhotoAsync(int checkInId, PhotoAngle angle)
    {
        var userId = LoggedInUser.Id;
        await GetCheckInForAthleteAsync(checkInId, userId); // ownership check

        var photo = await _photoRepo.Query()
            .FirstOrDefaultAsync(p => p.ClientCheckInId == checkInId && p.Angle == angle)
            ?? throw new KeyNotFoundException($"No photo found for angle '{angle}' on check-in {checkInId}.");

        await _blobService.DeleteFileAsync(photo.BlobUrl);
        _photoRepo.Delete(photo);
        await _photoRepo.SaveChangesAsync();
    }

    // ─── My History (logged-in athlete) ─────────────────────────────

    public async Task<PagedResult<CheckInDto>> GetMyCheckInHistoryAsync(BasePaginationForm pagination)
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.QueryAll()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new KeyNotFoundException("Athlete profile not found.");

        return await GetCheckInHistoryAsync(athlete.Id, pagination);
    }

    // ─── History (by explicit athlete ID) ─────────────────────────────────

    public async Task<PagedResult<CheckInDto>> GetCheckInHistoryAsync(int athleteId, BasePaginationForm pagination)
    {
        var query = _checkInRepo.QueryAll()
            .Include(ci => ci.Athlete).ThenInclude(a => a.User)
            .Where(ci => ci.AthleteId == athleteId)
            .OrderByDescending(ci => ci.WeekOf);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var dtos = new List<CheckInDto>();
        foreach (var ci in items)
        {
            var photos = await BuildPhotoDtosAsync(ci.Id);
            dtos.Add(CheckInMapper.Map(ci, photos));
        }

        return new PagedResult<CheckInDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    // ─── Pending (Coach) ──────────────────────────────────────────────

    public async Task<PagedResult<PendingCheckInDto>> GetPendingCheckInsAsync(BasePaginationForm pagination)
    {
        var userId = LoggedInUser.Id;
        var coach = await _coachRepo.QueryAll()
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException("Coach profile not found.");

        var weekOf = GetCurrentWeekMonday();

        // All athletes under this coach
        var allAthletes = await _athleteRepo.QueryAll()
            .Include(a => a.User)
            .Where(a => a.AssignedCoachId == coach.Id)
            .ToListAsync();

        // Athletes who have submitted this week
        var submittedAthleteIds = await _checkInRepo.QueryAll()
            .Where(ci => ci.WeekOf == weekOf && allAthletes.Select(a => a.Id).Contains(ci.AthleteId))
            .Select(ci => ci.AthleteId)
            .ToListAsync();

        var pendingAthletes = allAthletes
            .Where(a => !submittedAthleteIds.Contains(a.Id))
            .ToList();

        var total = pendingAthletes.Count;

        var paged = pendingAthletes
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var pagedAthleteIds = paged.Select(a => a.Id).ToList();
        var lastCheckIns = await _checkInRepo.QueryAll()
            .Where(ci => pagedAthleteIds.Contains(ci.AthleteId))
            .GroupBy(ci => ci.AthleteId)
            .Select(g => g.OrderByDescending(ci => ci.WeekOf).FirstOrDefault())
            .ToListAsync();

        var lastCheckInMap = lastCheckIns
            .Where(ci => ci != null)
            .ToDictionary(ci => ci!.AthleteId);

        var dtos = new List<PendingCheckInDto>();
        foreach (var athlete in paged)
        {
            lastCheckInMap.TryGetValue(athlete.Id, out var lastCheckIn);
            dtos.Add(CheckInMapper.MapPending(athlete, lastCheckIn));
        }

        return new PagedResult<PendingCheckInDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    // ─── Coach Notes ──────────────────────────────────────────────────

    public async Task<CheckInDto> AddCoachNotesAsync(int checkInId, AddCoachNotesForm form)
    {
        var checkIn = await _checkInRepo.Query()
            .Include(ci => ci.Athlete).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(ci => ci.Id == checkInId)
            ?? throw new KeyNotFoundException("Check-in not found.");

        checkIn.CoachNotes = form.Notes;
        checkIn.CoachReviewedAt = DateTime.UtcNow;
        _checkInRepo.Update(checkIn);
        await _checkInRepo.SaveChangesAsync();

        // Send notification to athlete
        try
        {
            await _notificationService.CreateAndSendNotificationAsync(
                checkIn.Athlete.UserId,
                NotificationType.CoachNote,
                $"Your coach reviewed your weekly check-in: \"{checkIn.CoachNotes}\""
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send check-in review notification to user {UserId}.", checkIn.Athlete.UserId);
        }

        var photoDtos = await BuildPhotoDtosAsync(checkInId);
        return CheckInMapper.Map(checkIn, photoDtos);
    }

    // ─── Get Photos (signed URLs) ─────────────────────────────────────

    public async Task<List<CheckInPhotoDto>> GetCheckInPhotosAsync(int checkInId)
    {
        var photos = await _photoRepo.QueryAll()
            .Where(p => p.ClientCheckInId == checkInId)
            .ToListAsync();

        return await MapPhotosWithSignedUrlsAsync(photos);
    }

    public async Task<CheckInDto> GetCheckInByIdAsync(int checkInId)
    {
        var checkIn = await _checkInRepo.Query()
            .Include(ci => ci.Athlete).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(ci => ci.Id == checkInId)
            ?? throw new KeyNotFoundException("Check-in not found.");

        // Permission check
        if (LoggedInUser.Role == "Athlete" && checkIn.Athlete.UserId != LoggedInUser.Id)
        {
            throw new UnauthorizedAccessException("You can only access your own check-ins.");
        }

        var photoDtos = await BuildPhotoDtosAsync(checkInId);
        return CheckInMapper.Map(checkIn, photoDtos);
    }

    // ─── Private Helpers ──────────────────────────────────────────────

    private static DateOnly GetCurrentWeekMonday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysFromMonday);
    }

    private async Task<ClientCheckIn> GetCheckInForAthleteAsync(int checkInId, int userId)
    {
        var athlete = await _athleteRepo.QueryAll()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new KeyNotFoundException("Athlete profile not found.");

        return await _checkInRepo.QueryAll()
            .FirstOrDefaultAsync(ci => ci.Id == checkInId && ci.AthleteId == athlete.Id)
            ?? throw new UnauthorizedAccessException("Check-in not found or does not belong to the current athlete.");
    }

    private async Task<List<CheckInPhotoDto>> BuildPhotoDtosAsync(int checkInId)
    {
        var photos = await _photoRepo.QueryAll()
            .Where(p => p.ClientCheckInId == checkInId)
            .ToListAsync();

        return await MapPhotosWithSignedUrlsAsync(photos);
    }

    private async Task<List<CheckInPhotoDto>> MapPhotosWithSignedUrlsAsync(List<CheckInPhoto> photos)
    {
        var dtos = new List<CheckInPhotoDto>();
        foreach (var photo in photos)
        {
            // BlobStorageService.DeleteFileAsync works with full URL, 
            // UploadFileAsync returns full URL — we serve the URL directly 
            // since local dev uses wwwroot/uploads and Azure uses blob URLs.
            // For production, swap this with a SAS token generator.
            dtos.Add(CheckInMapper.MapPhoto(photo, photo.BlobUrl));
        }
        return dtos;
    }
}
