import { beforeEach, describe, expect, it, vi } from 'vitest';
import axiosInstance from './axiosInstance';
import { assignWorkoutTemplate } from './workoutTemplate';
import { updateNutritionPlan, validateNutritionPlan } from './nutritionPlan';
import { downloadAthleteProgressReportPdf, getAthleteProgressReport } from './coachHub';
import type { NutritionPlanForm, NutritionPlanValidation } from '../types/NutritionPlan';

vi.mock('./axiosInstance', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
  },
}));

const mockedAxios = vi.mocked(axiosInstance);

describe('critical API contracts', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('assigns the selected workout template to the selected athletes', async () => {
    mockedAxios.post.mockResolvedValueOnce({ data: { assignedCount: 2, message: 'Assigned' } });

    const result = await assignWorkoutTemplate(25, { athleteIds: [2, 5] });

    expect(mockedAxios.post).toHaveBeenCalledWith('/workout-templates/25/assign', {
      athleteIds: [2, 5],
    });
    expect(result.assignedCount).toBe(2);
  });

  it('updates a nutrition plan with optimistic content-version protection', async () => {
    const form = {
      name: '2200 calorie plan',
      description: 'Training and rest days use the same target.',
      targetCalories: 2200,
      minimumProteinGrams: 140,
      mealBlocks: [],
      rules: [],
      expectedContentVersion: 7,
    } as NutritionPlanForm;
    mockedAxios.put.mockResolvedValueOnce({ data: { id: 12, ...form } });

    await updateNutritionPlan(12, form);

    expect(mockedAxios.put).toHaveBeenCalledWith('/nutrition-plans/12', form);
  });

  it('returns server validation issues that block nutrition-plan publishing', async () => {
    const validation: NutritionPlanValidation = {
      isValidForPublish: false,
      targetCalories: 2200,
      mealBlockCalories: 2200,
      trainingDayCalories: 2200,
      restDayCalories: 1700,
      issues: [{
        severity: 'Error',
        code: 'rest_day_calorie_mismatch',
        message: 'Rest-day meal blocks total 1700 kcal but the plan target is 2200 kcal.',
        path: 'targetCalories',
      }],
    };
    mockedAxios.get.mockResolvedValueOnce({ data: validation });

    const result = await validateNutritionPlan(12);

    expect(mockedAxios.get).toHaveBeenCalledWith('/nutrition-plans/12/validation');
    expect(result.isValidForPublish).toBe(false);
    expect(result.issues[0]?.code).toBe('rest_day_calorie_mismatch');
  });

  it('includes the photo choice in progress-report preview and PDF requests', async () => {
    mockedAxios.get
      .mockResolvedValueOnce({ data: { athleteId: 9 } })
      .mockResolvedValueOnce({ data: new Blob(['pdf'], { type: 'application/pdf' }) });

    await getAthleteProgressReport(9, {
      weeks: 8,
      includeCoachNotes: false,
      includePhotos: true,
    });
    await downloadAthleteProgressReportPdf(9, {
      weeks: 8,
      includeCoachNotes: false,
      includePhotos: true,
      language: 'ar',
    });

    expect(mockedAxios.get).toHaveBeenNthCalledWith(
      1,
      '/coach-hub/athletes/9/progress-report',
      { params: { weeks: 8, includeCoachNotes: false, includePhotos: true } },
    );
    expect(mockedAxios.get).toHaveBeenNthCalledWith(
      2,
      '/coach-hub/athletes/9/progress-report/pdf',
      {
        params: { weeks: 8, includeCoachNotes: false, includePhotos: true, language: 'ar' },
        responseType: 'blob',
      },
    );
  });
});
