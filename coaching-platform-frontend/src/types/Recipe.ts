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
  displayQuantity?: number;
  unit?: string;
  measurementState?: string;
  displayText?: string;
  displayTextAr?: string;
  isOptional?: boolean;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
}

export interface RecipeStepDto {
  orderIndex: number;
  instruction?: string;
  instructionAr: string;
  mediaUrl?: string;
}

export interface RecipeDto {
  id: number;
  seedKey?: string;
  name: string;
  nameAr?: string;
  description: string;
  descriptionAr?: string;
  usageNotes?: string;
  usageNotesAr?: string;
  contentStatus?: 'Draft' | 'InReview' | 'Published' | 'Archived';
  category: RecipeCategory;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  isJokerRecipe: boolean;
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
  declaredCalories?: number;
  declaredProtein?: number;
  createdAt: string;
  imageUrl?: string;
  videoUrl?: string;
  ingredients: RecipeIngredientDto[];
  steps?: RecipeStepDto[];
  isFavorite?: boolean;
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
  isJokerRecipe?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}
