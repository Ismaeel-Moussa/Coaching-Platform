import axiosInstance from './axiosInstance';
import type {
  DailyDiaryDto,
  LogFoodForm,
  BulkLogFoodForm,
  MealLogDto,
  UpdateWaterForm,
  UpdateStepsForm,
  LogNutritionPlanOptionForm,
  NutritionPlanDiaryEntry,
} from '../types/Diary';
import type { MacroSummaryDto } from '../types/Athlete';
import type { FoodDto } from '../types/Food';
import type { RecipeDto } from '../types/Recipe';

export const getDiary = async (date: string): Promise<DailyDiaryDto> => {
  const response = await axiosInstance.get<DailyDiaryDto>(`/diary/${date}`);
  return response.data;
};

export const getMacroSummary = async (date: string): Promise<MacroSummaryDto> => {
  const response = await axiosInstance.get<MacroSummaryDto>(`/diary/summary/${date}`);
  return response.data;
};

export const logFood = async (form: LogFoodForm): Promise<MealLogDto> => {
  const response = await axiosInstance.post<MealLogDto>('/diary/log', form);
  return response.data;
};

export const bulkLogFood = async (form: BulkLogFoodForm): Promise<MealLogDto[]> => {
  const response = await axiosInstance.post<MealLogDto[]>('/diary/log/bulk', form);
  return response.data;
};

export const logNutritionPlanOption = async (
  form: LogNutritionPlanOptionForm,
): Promise<NutritionPlanDiaryEntry> => {
  const response = await axiosInstance.post<NutritionPlanDiaryEntry>('/diary/log/nutrition-plan', form);
  return response.data;
};

export const getNutritionPlanEntries = async (
  assignmentId: number,
  date: string,
): Promise<NutritionPlanDiaryEntry[]> => {
  const response = await axiosInstance.get<NutritionPlanDiaryEntry[]>(
    `/diary/nutrition-plan/${assignmentId}/${date}`,
  );
  return response.data;
};

export const getFilteredItems = async (
  type: 'food' | 'recipe',
  source: 'recent' | 'frequent' | 'favorites',
): Promise<(FoodDto | RecipeDto)[]> => {
  const response = await axiosInstance.get<(FoodDto | RecipeDto)[]>('/diary/filters', { params: { type, source } });
  return response.data;
};

export const toggleFavoriteFood = async (id: number): Promise<{ isFavorite: boolean }> => {
  const response = await axiosInstance.post<{ isFavorite: boolean }>(`/diary/favorites/food/${id}/toggle`);
  return response.data;
};

export const toggleFavoriteRecipe = async (id: number): Promise<{ isFavorite: boolean }> => {
  const response = await axiosInstance.post<{ isFavorite: boolean }>(`/diary/favorites/recipe/${id}/toggle`);
  return response.data;
};

export const removeLogEntry = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/diary/log/${id}`);
};

export const removeNutritionPlanEntry = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/diary/nutrition-plan/${id}`);
};

export const updateWater = async (date: string, body: UpdateWaterForm): Promise<void> => {
  await axiosInstance.patch(`/diary/${date}/water`, body);
};

export const updateSteps = async (date: string, body: UpdateStepsForm): Promise<void> => {
  await axiosInstance.patch(`/diary/${date}/steps`, body);
};
