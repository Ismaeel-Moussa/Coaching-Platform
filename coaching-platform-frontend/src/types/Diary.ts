// ── Diary Types ────────────────────────────────────────────────────────────────

export enum MealType {
  Breakfast = 0,
  Lunch = 1,
  Dinner = 2,
  Snack = 3,
  Suhoor = 4,
  Iftar = 5,
  PreWorkout = 6,
  PostWorkout = 7,
}

export const MEAL_TYPE_LABELS: Record<MealType, string> = {
  [MealType.Breakfast]: 'Breakfast',
  [MealType.Lunch]: 'Lunch',
  [MealType.Dinner]: 'Dinner',
  [MealType.Snack]: 'Snack',
  [MealType.Suhoor]: 'Suhoor',
  [MealType.Iftar]: 'Iftar',
  [MealType.PreWorkout]: 'Pre-Workout',
  [MealType.PostWorkout]: 'Post-Workout',
};

export interface MealLogFoodRef {
  id: number;
  name: string;
  category: string;
}

export interface MealLogDto {
  id: number;
  mealType: MealType;
  food: MealLogFoodRef | null;
  recipe: { id: number; name: string } | null;
  name?: string | null;
  nameAr?: string | null;
  nutritionPlanDiaryEntryId?: number | null;
  quantityGrams: number;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  loggedAt: string;
}

export interface DailyDiaryDto {
  id: number;
  date: string;
  targetCalories: number;
  targetProtein: number;
  targetCarbs: number;
  targetFat: number;
  waterLitersConsumed: number;
  waterLitersTarget: number;
  stepsWalked: number;
  stepsTarget: number;
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
  breakfast: MealLogDto[];
  lunch: MealLogDto[];
  dinner: MealLogDto[];
  snack: MealLogDto[];
  suhoor: MealLogDto[];
  iftar: MealLogDto[];
  preWorkout: MealLogDto[];
  postWorkout: MealLogDto[];
}

export interface LogFoodForm {
  date: string;
  mealType: MealType;
  foodId?: number | null;
  recipeId?: number | null;
  quantityGrams: number;
}

export interface BulkLogFoodItem {
  foodId: number | null;
  recipeId: number | null;
  quantityGrams: number;
}

export interface BulkLogFoodForm {
  date: string;
  mealType: MealType;
  items: BulkLogFoodItem[];
}

export interface LogNutritionPlanOptionForm {
  assignmentId: number;
  mealBlockId: number;
  mealOptionId: number;
  date: string;
  mealType: MealType;
  servings: number;
  selectedAlternativeItemIds: number[];
}

export interface NutritionPlanDiaryEntry {
  id: number;
  assignmentId: number;
  mealBlockId: number;
  mealOptionId: number;
  date: string;
  mealType: MealType;
  servings: number;
  loggedAt: string;
  mealLogs: MealLogDto[];
}

export interface UpdateWaterForm {
  waterLiters: number;
}

export interface UpdateStepsForm {
  steps: number;
}
