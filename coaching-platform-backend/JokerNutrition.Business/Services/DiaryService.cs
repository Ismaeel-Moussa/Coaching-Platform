using System.Security.Principal;
using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Business.Forms.Diary;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Business.Security;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IDiaryService
{
    Task<DailyDiaryDto> GetDiaryAsync(DateOnly date);
    Task<MacroSummaryDto> GetMacroSummaryAsync(DateOnly date);
    Task UpdateWaterAsync(DateOnly date, UpdateWaterForm form);
    Task UpdateStepsAsync(DateOnly date, UpdateStepsForm form);
    Task<DailyDiary> GetOrCreateDiaryAsync(int athleteId, DateOnly date);
}

public class DiaryService : _BaseService, IDiaryService
{
    private readonly IDailyDiaryRepository _diaryRepo;
    private readonly IMealLogRepository _mealLogRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly IMacroTargetRepository _macroTargetRepo;

    public DiaryService(
        IPrincipal principal,
        ILogger<DiaryService> logger,
        IDailyDiaryRepository diaryRepo,
        IMealLogRepository mealLogRepo,
        IAthleteRepository athleteRepo,
        IMacroTargetRepository macroTargetRepo)
        : base(principal, logger)
    {
        _diaryRepo = diaryRepo;
        _mealLogRepo = mealLogRepo;
        _athleteRepo = athleteRepo;
        _macroTargetRepo = macroTargetRepo;
    }

    public async Task<DailyDiaryDto> GetDiaryAsync(DateOnly date)
    {
        var athleteId = await GetAthleteIdAsync();
        var diary = await GetOrCreateDiaryAsync(athleteId, date);

        var logs = await _mealLogRepo.Query()
            .Include(l => l.Food)
            .Include(l => l.Recipe)
            .Where(l => l.DailyDiaryId == diary.Id)
            .OrderBy(l => l.LoggedAt)
            .ToListAsync();

        return DiaryMapper.Map(diary, logs);
    }

    public async Task<MacroSummaryDto> GetMacroSummaryAsync(DateOnly date)
    {
        var athleteId = await GetAthleteIdAsync();
        var diary = await GetOrCreateDiaryAsync(athleteId, date);

        var logs = await _mealLogRepo.Query()
            .Where(l => l.DailyDiaryId == diary.Id)
            .ToListAsync();

        return DiaryMapper.MapSummary(diary, logs);
    }

    public async Task UpdateWaterAsync(DateOnly date, UpdateWaterForm form)
    {
        var athleteId = await GetAthleteIdAsync();
        var diary = await GetOrCreateDiaryAsync(athleteId, date);
        diary.WaterLitersConsumed = form.WaterLiters;
        _diaryRepo.Update(diary);
        await _diaryRepo.SaveChangesAsync();
    }

    public async Task UpdateStepsAsync(DateOnly date, UpdateStepsForm form)
    {
        var athleteId = await GetAthleteIdAsync();
        var diary = await GetOrCreateDiaryAsync(athleteId, date);
        diary.StepsWalked = form.Steps;
        _diaryRepo.Update(diary);
        await _diaryRepo.SaveChangesAsync();
    }

    public async Task<DailyDiary> GetOrCreateDiaryAsync(int athleteId, DateOnly date)
    {
        var diary = await _diaryRepo.Query()
            .FirstOrDefaultAsync(d => d.AthleteId == athleteId && d.Date == date);

        if (diary is not null) return diary;

        // Pull active macro targets to snapshot into the diary
        var target = await _macroTargetRepo.Query()
            .Where(t => t.AthleteId == athleteId && t.IsActive)
            .OrderByDescending(t => t.SetAt)
            .FirstOrDefaultAsync();

        diary = new DailyDiary
        {
            AthleteId = athleteId,
            Date = date,
            TargetCalories = target?.TargetCalories ?? 2000m,
            TargetProtein = target?.TargetProtein ?? 150m,
            TargetCarbs = target?.TargetCarbs ?? 200m,
            TargetFat = target?.TargetFat ?? 65m,
            WaterLitersTarget = target?.WaterLitersTarget ?? 4.0m,
            StepsTarget = target?.StepsTarget ?? 7000
        };

        await _diaryRepo.CreateAsync(diary);
        await _diaryRepo.SaveChangesAsync();
        return diary;
    }

    // ─── Private helpers ───────────────────────────────────────────────
    private async Task<int> GetAthleteIdAsync()
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");
        return athlete.Id;
    }
}
