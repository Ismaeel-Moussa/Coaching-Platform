import { FoodState } from '../types/Diary';

/**
 * State conversion factors for macro calculation.
 * - Raw (0):    macros applied 1:1
 * - Cooked (1): multiply by 1.33 (cooked weight is lighter than raw)
 * - Dry (2):    multiply by 2.5  (dry grains/oats absorb water)
 */
const STATE_FACTORS: Record<FoodState, number> = {
  [FoodState.Raw]: 1.0,
  [FoodState.Cooked]: 1.33,
  [FoodState.Dry]: 2.5,
};

export interface MacroPreview {
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
}

/**
 * Calculates the macros for a given food at a given weight and cooking state.
 *
 * @param per100g - Macro value per 100g of the food (e.g. proteinPer100g)
 * @param grams - The quantity in grams entered by the user
 * @param state - The cooking state (Raw / Cooked / Dry)
 */
export const calcMacro = (per100g: number, grams: number, state: FoodState): number => {
  return (per100g * grams * STATE_FACTORS[state]) / 100;
};

/**
 * Returns a full MacroPreview for a food item at a given weight and state.
 */
export const calcMacroPreview = (
  food: {
    caloriesPer100g: number;
    proteinPer100g: number;
    carbsPer100g: number;
    fatPer100g: number;
  },
  grams: number,
  state: FoodState,
): MacroPreview => ({
  calories: calcMacro(food.caloriesPer100g, grams, state),
  protein: calcMacro(food.proteinPer100g, grams, state),
  carbs: calcMacro(food.carbsPer100g, grams, state),
  fat: calcMacro(food.fatPer100g, grams, state),
});
