export interface MacroPreview {
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
}

/**
 * Calculates the macros for a given food at a given weight.
 *
 * @param per100g - Macro value per 100g of the food (e.g. proteinPer100g)
 * @param grams - The quantity in grams entered by the user
 */
export const calcMacro = (per100g: number, grams: number): number => {
  return (per100g * grams) / 100;
};

/**
 * Returns a full MacroPreview for a food item at a given weight.
 */
export const calcMacroPreview = (
  food: {
    caloriesPer100g: number;
    proteinPer100g: number;
    carbsPer100g: number;
    fatPer100g: number;
  },
  grams: number,
): MacroPreview => ({
  calories: calcMacro(food.caloriesPer100g, grams),
  protein: calcMacro(food.proteinPer100g, grams),
  carbs: calcMacro(food.carbsPer100g, grams),
  fat: calcMacro(food.fatPer100g, grams),
});
