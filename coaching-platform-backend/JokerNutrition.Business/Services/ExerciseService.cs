using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IExerciseService
{
    Task<PagedResult<ExerciseDto>> GetExercisesAsync(string? search, MuscleGroup? muscle, int page, int pageSize);
    Task<ExerciseDto> GetExerciseByIdAsync(int id);
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
}
