using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Business.Forms.Workouts;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
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
}

public class WorkoutTemplateService : _BaseService, IWorkoutTemplateService
{
    private readonly JokerNutritionContext _context;
    private readonly IWorkoutTemplateRepository _templateRepo;
    private readonly ICoachRepository _coachRepo;
    private readonly IClientProgramRepository _clientProgramRepo;

    public WorkoutTemplateService(
        IPrincipal principal,
        ILogger<WorkoutTemplateService> logger,
        JokerNutritionContext context,
        IWorkoutTemplateRepository templateRepo,
        ICoachRepository coachRepo,
        IClientProgramRepository clientProgramRepo)
        : base(principal, logger)
    {
        _context = context;
        _templateRepo = templateRepo;
        _coachRepo = coachRepo;
        _clientProgramRepo = clientProgramRepo;
    }

    // ─── List (summary, paged) ────────────────────────────────────────
    public async Task<PagedResult<WorkoutTemplateSummaryDto>> GetTemplatesAsync(int page, int pageSize)
    {
        var coachId = await GetCoachIdAsync();

        var query = _templateRepo.Query()
            .Include(t => t.CreatedByCoach).ThenInclude(c => c.User)
            .Include(t => t.Days)
            .Where(t => t.IsActive && t.CreatedByCoachId == coachId);

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

        // Remove old days + exercises
        _context.Set<TemplateExercise>().RemoveRange(template.Days.SelectMany(d => d.Exercises));
        _context.Set<WorkoutTemplateDay>().RemoveRange(template.Days);

        // Add new days + exercises
        template.Days = form.Days.Select(dayForm => new WorkoutTemplateDay
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
        }).ToList();

        _templateRepo.Update(template);
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

        return (count, $"Template assigned to {count} athlete(s) successfully.");
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
