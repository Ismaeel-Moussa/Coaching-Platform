export type ContentStatus = 'Draft' | 'InReview' | 'Published' | 'Archived';
export type MealType = 'Breakfast' | 'Lunch' | 'Dinner' | 'Snack' | 'Suhoor' | 'Iftar' | 'PreWorkout' | 'PostWorkout';
export type IngredientUnit = 'Gram' | 'Milliliter' | 'Piece' | 'Tablespoon' | 'Teaspoon' | 'Cup' | 'Scoop';
export type FoodPreparationState = 'Unspecified' | 'Raw' | 'Cooked' | 'Drained';

export interface NutritionOptionItem {
  id?: number;
  orderIndex?: number;
  foodId?: number | null;
  foodName?: string | null;
  foodNameAr?: string | null;
  recipeId?: number | null;
  recipeName?: string | null;
  recipeNameAr?: string | null;
  itemName?: string | null;
  quantity: number;
  unit: IngredientUnit;
  measurementState: FoodPreparationState;
  alternativeGroupKey?: string | null;
}

export interface NutritionMealOption {
  id?: number;
  orderIndex?: number;
  label: string;
  isCompleteOption: boolean;
  items: NutritionOptionItem[];
}

export interface NutritionMealBlock {
  id?: number;
  orderIndex?: number;
  mealType: MealType;
  label: string;
  targetCalories: number | null;
  trainingDayOnly: boolean;
  restDayOnly: boolean;
  instructions?: string | null;
  options: NutritionMealOption[];
}

export interface NutritionPlanRule {
  id?: number;
  orderIndex?: number;
  ruleType: string;
  text?: string | null;
}

export interface NutritionPlanSummary {
  id: number;
  seedKey: string;
  name: string;
  targetCalories: number;
  minimumProteinGrams: number;
  contentStatus: ContentStatus;
  contentVersion: number;
  mealBlockCount: number;
  mealBlockCalories: number;
  trainingDayCalories: number;
  restDayCalories: number;
  activeAssignmentCount: number;
  isManuallyEdited: boolean;
  updatedAt?: string | null;
}

export interface NutritionPlan extends NutritionPlanSummary {
  description?: string | null;
  sourceDocument?: string | null;
  sourcePage?: number | null;
  mealBlocks: NutritionMealBlock[];
  rules: NutritionPlanRule[];
}

export type NutritionPlanForm = Pick<
  NutritionPlan,
  'name' | 'description' | 'targetCalories' | 'minimumProteinGrams' | 'mealBlocks' | 'rules'
> & { expectedContentVersion?: number };

export interface NutritionPlanValidationIssue {
  severity: 'Error' | 'Warning';
  code: string;
  message: string;
  path?: string | null;
}

export interface NutritionPlanValidation {
  isValidForPublish: boolean;
  targetCalories: number;
  mealBlockCalories: number;
  trainingDayCalories: number;
  restDayCalories: number;
  issues: NutritionPlanValidationIssue[];
}

export interface NutritionPlanAssignment {
  id: number;
  athleteId: number;
  templateId: number;
  templateName: string;
  startDate: string;
  endDate?: string | null;
  isActive: boolean;
  notes?: string | null;
  assignedAt: string;
  plan: NutritionPlan;
}

export interface PagedNutritionPlans {
  items: NutritionPlanSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}
