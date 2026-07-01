using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Business.Forms.Exercises;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IExerciseService
{
    Task<PagedResult<ExerciseDto>> GetExercisesAsync(string? search, MuscleGroup? muscle, int page, int pageSize);
    Task<ExerciseDto> GetExerciseByIdAsync(int id);
    Task<ExerciseDto> CreateExerciseAsync(CreateExerciseForm form);
    Task<ExerciseDto> UpdateExerciseAsync(int id, UpdateExerciseForm form);
    Task DeleteExerciseAsync(int id);
}

public class ExerciseService : _BaseService, IExerciseService
{
    private readonly IExerciseRepository _exerciseRepo;

    public ExerciseService(
        IPrincipal principal,
        ILogger<ExerciseService> logger,
        IExerciseRepository exerciseRepo)
        : base(principal, logger)
    {
        _exerciseRepo = exerciseRepo;
    }

    public async Task<PagedResult<ExerciseDto>> GetExercisesAsync(string? search, MuscleGroup? muscle, int page, int pageSize)
    {
        var query = _exerciseRepo.Query()
            .Where(e => e.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Name.Contains(search));

        if (muscle.HasValue)
            query = query.Where(e => e.PrimaryMuscle == muscle.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.PrimaryMuscle)
            .ThenBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => ExerciseMapper.Map(e))
            .ToListAsync();

        return new PagedResult<ExerciseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ExerciseDto> GetExerciseByIdAsync(int id)
    {
        var exercise = await _exerciseRepo.Query()
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive)
            ?? throw new KeyNotFoundException($"Exercise {id} not found.");

        return ExerciseMapper.Map(exercise);
    }

    // ─── Admin Mutations ──────────────────────────────────────────────

    public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseForm form)
    {
        if (!Enum.TryParse<MuscleGroup>(form.PrimaryMuscle, ignoreCase: true, out var muscleGroup))
            throw new ArgumentException($"Invalid muscle group: {form.PrimaryMuscle}");

        var exercise = new Exercise
        {
            Name = form.Name,
            PrimaryMuscle = muscleGroup,
            EquipmentRequired = form.EquipmentRequired,
            Instructions = form.Instructions,
            YouTubeVideoId = form.YouTubeVideoId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _exerciseRepo.CreateAsync(exercise);
        await _exerciseRepo.SaveChangesAsync();

        return ExerciseMapper.Map(exercise);
    }

    public async Task<ExerciseDto> UpdateExerciseAsync(int id, UpdateExerciseForm form)
    {
        var exercise = await _exerciseRepo.Query()
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive)
            ?? throw new KeyNotFoundException($"Exercise {id} not found.");

        if (form.Name is not null) exercise.Name = form.Name;
        if (form.EquipmentRequired is not null) exercise.EquipmentRequired = form.EquipmentRequired;
        if (form.Instructions is not null) exercise.Instructions = form.Instructions;
        if (form.YouTubeVideoId is not null) exercise.YouTubeVideoId = form.YouTubeVideoId;

        if (form.PrimaryMuscle is not null)
        {
            if (!Enum.TryParse<MuscleGroup>(form.PrimaryMuscle, ignoreCase: true, out var mg))
                throw new ArgumentException($"Invalid muscle group: {form.PrimaryMuscle}");
            exercise.PrimaryMuscle = mg;
        }

        _exerciseRepo.Update(exercise);
        await _exerciseRepo.SaveChangesAsync();

        return ExerciseMapper.Map(exercise);
    }

    public async Task DeleteExerciseAsync(int id)
    {
        var exercise = await _exerciseRepo.Query()
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive)
            ?? throw new KeyNotFoundException($"Exercise {id} not found.");

        exercise.IsActive = false;
        _exerciseRepo.Update(exercise);
        await _exerciseRepo.SaveChangesAsync();
    }
}
