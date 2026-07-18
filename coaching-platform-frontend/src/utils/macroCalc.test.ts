import { describe, expect, it } from 'vitest';
import { calcMacro, calcMacroPreview } from './macroCalc';

describe('macro calculations', () => {
  it('scales a per-100g value to the selected quantity', () => {
    expect(calcMacro(24, 150)).toBe(36);
    expect(calcMacro(24, 0)).toBe(0);
  });

  it('calculates the complete macro preview', () => {
    expect(calcMacroPreview({
      caloriesPer100g: 200,
      proteinPer100g: 20,
      carbsPer100g: 10,
      fatPer100g: 5,
    }, 250)).toEqual({
      calories: 500,
      protein: 50,
      carbs: 25,
      fat: 12.5,
    });
  });
});
