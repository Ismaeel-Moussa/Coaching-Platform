using System.Security.Principal;
using JokerNutrition.Business.Common;
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

public interface IWorkoutTemplateService
{
    Task<PagedResult<WorkoutTemplateSummaryDto>> GetTemplatesAsync(int page, int pageSize);
    Task<WorkoutTemplateDto> GetTemplateByIdAsync(int id);
    Task<WorkoutTemplateDto> CreateTemplateAsync(CreateWorkoutTemplateForm form);
    Task<WorkoutTemplateDto> UpdateTemplateAsync(int id, CreateWorkoutTemplateForm form);
    Task<(int assignedCount, string message)> AssignTemplateAsync(int templateId, AssignTemplateForm form);
    Task DeleteTemplateAsync(int id);
}

public class WorkoutTemplateService : _BaseService, IWorkoutTemplateService
{
    private readonly JokerNutritionContext _context;
    private readonly IWorkoutTemplateRepository _templateRepo;
    private readonly ICoachRepository _coachRepo;
    private readonly IClientProgramRepository _clientProgramRepo;
    private readonly INotificationService _notificationService;

    public WorkoutTemplateService(
        IPrincipal principal,
        ILogger<WorkoutTemplateService> logger,
        JokerNutritionContext context,
        IWorkoutTemplateRepository templateRepo,
        ICoachRepository coachRepo,
        IClientProgramRepository clientProgramRepo,
        INotificationService notificationService)
        : base(principal, logger)
    {
        _context = context;
        _templateRepo = templateRepo;
        _coachRepo = coachRepo;
        _clientProgramRepo = clientProgramRepo;
        _notificationService = notificationService;
    }

    // ─── List (summary, paged) ────────────────────────────────────────
    public async Task<PagedResult<WorkoutTemplateSummaryDto>> GetTemplatesAsync(int page, int pageSize)
    {
        var query = _templateRepo.Query()
            .Include(t => t.CreatedByCoach).ThenInclude(c => c.User)
            .Include(t => t.Days)
            .Where(t => t.IsActive);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WorkoutTemplateSummaryDto>
        {
            Items = items.Select(WorkoutTemplateMapper.MapSummary),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ─── Get by ID (full nested) ──────────────────────────────────────
    public async Task<WorkoutTemplateDto> GetTemplateByIdAsync(int id)
    {
        var template = await _templateRepo.Query()
            .Include(t => t.CreatedByCoach).ThenInclude(c => c.User)
            .Include(t => t.Days)
                .ThenInclude(d => d.Exercises)
                    .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive)
            ?? throw new KeyNotFoundException($"Workout template {id} not found.");

        return WorkoutTemplateMapper.MapFull(template);
    }

    // ─── Create ───────────────────────────────────────────────────────
    public async Task<WorkoutTemplateDto> CreateTemplateAsync(CreateWorkoutTemplateForm form)
    {
        var coachId = await GetCoachIdAsync();

        var template = new WorkoutTemplate
        {
            Name = form.Name,
            Description = form.Description,
            CreatedByCoachId = coachId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Days = form.Days.Select(dayForm => new WorkoutTemplateDay
            {
                DayNumber = dayForm.DayNumber,
                DayLabel = dayForm.DayLabel,
                IsRestDay = dayForm.IsRestDay,
                Exercises = dayForm.Exercises.Select(exForm => new TemplateExercise
                {
                    ExerciseId = exForm.ExerciseId,
                    Section = exForm.Section,
                    OrderIndex = exForm.OrderIndex,
                    TargetSets = exForm.TargetSets,
                    TargetReps = exForm.TargetReps,
                    RestSeconds = exForm.RestSeconds,
                    ProgressiveOverloadTargetKg = exForm.ProgressiveOverloadTargetKg
                }).ToList()
            }).ToList()
        };

        await _templateRepo.CreateAsync(template);
        await _templateRepo.SaveChangesAsync();

        return await GetTemplateByIdAsync(template.Id);
    }

    // ─── Update (full replacement) ────────────────────────────────────
    public async Task<WorkoutTemplateDto> UpdateTemplateAsync(int id, CreateWorkoutTemplateForm form)
    {
        var template = await _templateRepo.Query()
            .Include(t => t.Days).ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive)
            ?? throw new KeyNotFoundException($"Workout template {id} not found.");

        template.Name = form.Name;
        template.Description = form.Description;

        // Clear old days (cascade deletes old days and exercises)
        template.Days.Clear();

        // Add new days + exercises
        foreach (var dayForm in form.Days)
        {
            template.Days.Add(new WorkoutTemplateDay
            {
                DayNumber = dayForm.DayNumber,
                DayLabel = dayForm.DayLabel,
                IsRestDay = dayForm.IsRestDay,
                Exercises = dayForm.Exercises.Select(exForm => new TemplateExercise
                {
                    ExerciseId = exForm.ExerciseId,
                    Section = exForm.Section,
                    OrderIndex = exForm.OrderIndex,
                    TargetSets = exForm.TargetSets,
                    TargetReps = exForm.TargetReps,
                    RestSeconds = exForm.RestSeconds,
                    ProgressiveOverloadTargetKg = exForm.ProgressiveOverloadTargetKg
                }).ToList()
            });
        }

        await _templateRepo.SaveChangesAsync();

        return await GetTemplateByIdAsync(template.Id);
    }

    // ─── Assign to Athletes ───────────────────────────────────────────
    public async Task<(int assignedCount, string message)> AssignTemplateAsync(int templateId, AssignTemplateForm form)
    {
        var coachId = await GetCoachIdAsync();

        var template = await _templateRepo.Query()
            .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive)
            ?? throw new KeyNotFoundException($"Workout template {templateId} not found.");

        int count = 0;
        foreach (var athleteId in form.AthleteIds)
        {
            // Deactivate existing active program(s) for this athlete
            var existing = await _clientProgramRepo.Query()
                .Where(p => p.AthleteId == athleteId && p.IsActive)
                .ToListAsync();

            foreach (var prog in existing)
            {
                prog.IsActive = false;
                _clientProgramRepo.Update(prog);
            }

            // Create new program
            await _clientProgramRepo.CreateAsync(new ClientProgram
            {
                AthleteId = athleteId,
                WorkoutTemplateId = templateId,
                AssignedByCoachId = coachId,
                StartDate = DateTime.UtcNow,
                IsActive = true
            });

            count++;
        }

        await _clientProgramRepo.SaveChangesAsync();

        foreach (var athleteId in form.AthleteIds)
        {
            try
            {
                var athleteUserId = await _context.Athletes
                    .Where(a => a.Id == athleteId)
                    .Select(a => a.UserId)
                    .FirstOrDefaultAsync();

                if (athleteUserId != 0)
                {
                    await _notificationService.CreateAndSendNotificationAsync(
                        athleteUserId,
                        NotificationType.CoachNote,
                        $"New workout program template assigned: {template.Name}"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification to athlete {AthleteId} for workout template assignment.", athleteId);
            }
        }

        return (count, $"Template assigned to {count} athlete(s) successfully.");
    }

    // ─── Delete (soft delete) ─────────────────────────────────────────
    public async Task DeleteTemplateAsync(int id)
    {
        var template = await _templateRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Workout template {id} not found.");

        template.IsActive = false;
        _templateRepo.Update(template);

        // Deactivate all athlete programs that reference this template,
        // so athletes are no longer on a program pointing to a deleted template.
        var affectedPrograms = await _clientProgramRepo.Query()
            .Where(p => p.WorkoutTemplateId == id && p.IsActive)
            .ToListAsync();

        foreach (var program in affectedPrograms)
        {
            program.IsActive = false;
            _clientProgramRepo.Update(program);
        }

        await _templateRepo.SaveChangesAsync();
    }

    // ─── Helpers ──────────────────────────────────────────────────────
    private async Task<int> GetCoachIdAsync()
    {
        var userId = LoggedInUser.Id;
        var coach = await _coachRepo.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException("Coach profile not found for the current user.");
        return coach.Id;
    }
}
