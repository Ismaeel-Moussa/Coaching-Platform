using System.Security.Principal;
using JokerNutrition.Business.DTOs.Supplements;
using JokerNutrition.Business.Forms.Supplements;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface ISupplementService
{
    Task<List<SupplementDto>> GetScheduleAsync();
    Task<List<SupplementDto>> GetAthleteScheduleAsync(int athleteId);
    Task<SupplementDto> ToggleTakenAsync(ToggleSupplementForm form);
    Task<SupplementDto> AssignSupplementAsync(AssignSupplementForm form);
    Task<SupplementDto> UpdateSupplementAsync(int id, UpdateSupplementForm form);
    Task DeleteSupplementAsync(int id);
}

public class SupplementService : _BaseService, ISupplementService
{
    private readonly IAthleteRepository _athleteRepo;
    private readonly ISupplementScheduleRepository _scheduleRepo;
    private readonly ISupplementLogRepository _logRepo;

    public SupplementService(
        IPrincipal principal,
        ILogger<SupplementService> logger,
        IAthleteRepository athleteRepo,
        ISupplementScheduleRepository scheduleRepo,
        ISupplementLogRepository logRepo)
        : base(principal, logger)
    {
        _athleteRepo = athleteRepo;
        _scheduleRepo = scheduleRepo;
        _logRepo = logRepo;
    }

    // ─── Get athlete's supplement schedule with today's status ────────
    public async Task<List<SupplementDto>> GetScheduleAsync()
    {
        var athlete = await GetAthleteAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var schedules = await _scheduleRepo.Query()
            .Where(s => s.AthleteId == athlete.Id && s.IsActive)
            .OrderBy(s => s.Type)
            .ThenBy(s => s.Name)
            .ToListAsync();

        var todayLogs = await _logRepo.Query()
            .Where(l => schedules.Select(s => s.Id).Contains(l.SupplementScheduleId)
                        && l.Date == today)
            .ToListAsync();

        return schedules
            .Select(s =>
            {
                var log = todayLogs.FirstOrDefault(l => l.SupplementScheduleId == s.Id);
                return SupplementMapper.Map(s, log);
            })
            .ToList();
    }

    // ─── Get a specific athlete's supplement schedule (for coach) ────
    public async Task<List<SupplementDto>> GetAthleteScheduleAsync(int athleteId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var schedules = await _scheduleRepo.Query()
            .Where(s => s.AthleteId == athleteId && s.IsActive)
            .OrderBy(s => s.Type)
            .ThenBy(s => s.Name)
            .ToListAsync();

        var todayLogs = await _logRepo.Query()
            .Where(l => schedules.Select(s => s.Id).Contains(l.SupplementScheduleId)
                        && l.Date == today)
            .ToListAsync();

        return schedules
            .Select(s =>
            {
                var log = todayLogs.FirstOrDefault(l => l.SupplementScheduleId == s.Id);
                return SupplementMapper.Map(s, log);
            })
            .ToList();
    }

    // ─── Toggle supplement taken/untaken for a given date ─────────────
    public async Task<SupplementDto> ToggleTakenAsync(ToggleSupplementForm form)
    {
        var athlete = await GetAthleteAsync();

        var schedule = await _scheduleRepo.Query()
            .FirstOrDefaultAsync(s => s.Id == form.SupplementScheduleId && s.AthleteId == athlete.Id && s.IsActive)
            ?? throw new KeyNotFoundException("Supplement schedule not found.");

        var log = await _logRepo.Query()
            .FirstOrDefaultAsync(l => l.SupplementScheduleId == form.SupplementScheduleId && l.Date == form.Date);

        if (log == null)
        {
            log = new SupplementLog
            {
                SupplementScheduleId = form.SupplementScheduleId,
                Date = form.Date,
                IsTaken = true,
                TakenAt = DateTime.UtcNow
            };
            await _logRepo.CreateAsync(log);
        }
        else
        {
            log.IsTaken = !log.IsTaken;
            log.TakenAt = log.IsTaken ? DateTime.UtcNow : null;
            _logRepo.Update(log);
        }

        await _logRepo.SaveChangesAsync();
        return SupplementMapper.Map(schedule, log);
    }

    // ─── Coach assigns a new supplement to an athlete ──────────────────
    public async Task<SupplementDto> AssignSupplementAsync(AssignSupplementForm form)
    {
        var schedule = new SupplementSchedule
        {
            AthleteId = form.AthleteId,
            Name = form.Name,
            Type = form.Type,
            Dosage = form.Dosage,
            Notes = form.Notes,
            IsActive = true
        };

        await _scheduleRepo.CreateAsync(schedule);
        await _scheduleRepo.SaveChangesAsync();

        return SupplementMapper.Map(schedule, null);
    }

    // ─── Update supplement (for coach) ───────────────────────────────
    public async Task<SupplementDto> UpdateSupplementAsync(int id, UpdateSupplementForm form)
    {
        var schedule = await _scheduleRepo.Query()
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive)
            ?? throw new KeyNotFoundException("Supplement schedule not found.");

        schedule.Name = form.Name;
        schedule.Type = form.Type;
        schedule.Dosage = form.Dosage;
        schedule.Notes = form.Notes;

        _scheduleRepo.Update(schedule);
        await _scheduleRepo.SaveChangesAsync();

        return SupplementMapper.Map(schedule, null);
    }

    // ─── Delete supplement (for coach) ───────────────────────────────
    public async Task DeleteSupplementAsync(int id)
    {
        var schedule = await _scheduleRepo.Query()
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive)
            ?? throw new KeyNotFoundException("Supplement schedule not found.");

        schedule.IsActive = false;
        _scheduleRepo.Update(schedule);
        await _scheduleRepo.SaveChangesAsync();
    }

    // ─── Private helpers ──────────────────────────────────────────────
    private async Task<Athlete> GetAthleteAsync()
    {
        var userId = LoggedInUser.Id;
        return await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");
    }
}
