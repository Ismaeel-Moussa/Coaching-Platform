// ── Recipe Types ───────────────────────────────────────────────────────────────

export enum RecipeCategory {
  MuscleBuilding = 0,
  FatLoss = 1,
  Custom = 2,
}

export const RECIPE_CATEGORY_LABELS: Record<RecipeCategory, string> = {
  [RecipeCategory.MuscleBuilding]: 'Muscle Building',
  [RecipeCategory.FatLoss]: 'Fat Loss',
  [RecipeCategory.Custom]: 'Custom',
};

export interface RecipeIngredientDto {
  foodId: number;
  foodName: string;
  quantityGrams: number;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
}

export interface RecipeDto {
  id: number;
  name: string;
  description: string;
  category: RecipeCategory;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  isJokerRecipe: boolean;
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
  createdAt: string;
  imageUrl?: string;
  videoUrl?: string;
  ingredients: RecipeIngredientDto[];
}

export interface RecipesPagedResult {
  items: RecipeDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CreateRecipeIngredient {
  foodId: number;
  quantityGrams: number;
}

export interface CreateRecipeForm {
  name: string;
  description?: string;
  category: RecipeCategory;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  videoUrl?: string;
  ingredients: CreateRecipeIngredient[];
}

export type UpdateRecipeForm = CreateRecipeForm;

export interface GetRecipesParams {
  category?: RecipeCategory;
  search?: string;
  page?: number;
  pageSize?: number;
}
