// ── Food Types ─────────────────────────────────────────────────────────────────

export type FoodCategory = 'Protein' | 'Carbs' | 'Fat' | 'Vegetable' | 'Dairy' | 'Fruit' | 'Meat' | 'Bakery' | 'Cheese' | 'Poultry' | 'Nuts' | 'Oils' | 'Condiments' | 'Juice' | 'Snacks';
export type FoodState = 'Raw' | 'Cooked' | 'Dry';

export interface FoodDto {
  id: number;
  name: string;
  category: FoodCategory;
  state: FoodState;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
  fiberPer100g: number;
  isCustom: boolean;
}

export interface FoodsPagedResult {
  items: FoodDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface SearchFoodsParams {
  search?: string;
  category?: FoodCategory;
  state?: FoodState;
  page?: number;
  pageSize?: number;
}

export interface CreateFoodForm {
  name: string;
  category: FoodCategory;
  state: FoodState;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
  fiberPer100g: number;
}

export interface BulkImportResultDto {
  insertedCount: number;
  skippedCount: number;
  errors: string[];
}

