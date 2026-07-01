import axiosInstance from './axiosInstance';
import type {
  RecipeDto,
  RecipesPagedResult,
  CreateRecipeForm,
  GetRecipesParams,
} from '../types/Recipe';
import type { DailyDiaryDto } from '../types/Diary';

export const getRecipes = async (params: GetRecipesParams): Promise<RecipesPagedResult> => {
  const response = await axiosInstance.get<RecipesPagedResult>('/recipes', { params });
  return response.data;
};

export const getRecipeById = async (id: number): Promise<RecipeDto> => {
  const response = await axiosInstance.get<RecipeDto>(`/recipes/${id}`);
  return response.data;
};

export const createRecipe = async (form: CreateRecipeForm): Promise<RecipeDto> => {
  const response = await axiosInstance.post<RecipeDto>('/recipes', form);
  return response.data;
};

export const quickAddRecipeToDiary = async (
  id: number,
  mealType: number,
): Promise<DailyDiaryDto> => {
  const response = await axiosInstance.post<DailyDiaryDto>(
    `/recipes/${id}/add-to-diary`,
    null,
    { params: { mealType } },
  );
  return response.data;
};

export const deleteRecipe = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/recipes/${id}`);
};

