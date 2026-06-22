import React, { useState, useEffect } from 'react';
import {
  Modal, Steps, Input, Select, InputNumber, Button, Spin, Empty, Form,
} from 'antd';
import { useSearchFoods } from '../../hooks/useFoods/useFoods';
import { useCreateRecipe } from '../../hooks/useRecipes/useRecipes';
import { calcMacroPreview } from '../../utils/macroCalc';
import { FoodState, FOOD_STATE_LABELS } from '../../types/Diary';
import { RecipeCategory, RECIPE_CATEGORY_LABELS, type CreateRecipeIngredient } from '../../types/Recipe';
import type { FoodDto } from '../../types/Food';
import './CreateRecipeModal.scss';

const { Search } = Input;
const { Option } = Select;

interface IngredientEntry {
  food: FoodDto;
  quantityGrams: number;
  state: FoodState;
}

interface CreateRecipeModalProps {
  open: boolean;
  onClose: () => void;
}

const CreateRecipeModal: React.FC<CreateRecipeModalProps> = ({ open, onClose }) => {
  const [currentStep, setCurrentStep] = useState(0);
  const [ingredients, setIngredients] = useState<IngredientEntry[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [pendingFood, setPendingFood] = useState<FoodDto | null>(null);
  const [pendingQty, setPendingQty] = useState<number>(100);
  const [pendingState, setPendingState] = useState<FoodState>(FoodState.Raw);

  // Recipe metadata (Step 2)
  const [recipeName, setRecipeName] = useState('');
  const [recipeDesc, setRecipeDesc] = useState('');
  const [recipeCategory, setRecipeCategory] = useState<RecipeCategory>(RecipeCategory.Custom);
  const [prepTime, setPrepTime] = useState<number>(5);
  const [cookTime, setCookTime] = useState<number>(0);
  const [servings, setServings] = useState<number>(1);

  const createMutation = useCreateRecipe();

  // Debounce
  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(searchTerm), 400);
    return () => clearTimeout(t);
  }, [searchTerm]);

  const { data: foodsData, isLoading: isSearching } = useSearchFoods(
    { search: debouncedSearch, pageSize: 15 },
    currentStep === 0 && debouncedSearch.length >= 1,
  );

  // ── Computed totals ──────────────────────────────────────────────────────────
  const totals = ingredients.reduce(
    (acc, ing) => {
      const p = calcMacroPreview(ing.food, ing.quantityGrams, ing.state);
      return {
        calories: acc.calories + p.calories,
        protein: acc.protein + p.protein,
        carbs: acc.carbs + p.carbs,
        fat: acc.fat + p.fat,
      };
    },
    { calories: 0, protein: 0, carbs: 0, fat: 0 },
  );

  // ── Step 1: Add ingredient ───────────────────────────────────────────────────
  const handleAddIngredient = () => {
    if (!pendingFood) return;
    setIngredients((prev) => [...prev, { food: pendingFood, quantityGrams: pendingQty, state: pendingState }]);
    setPendingFood(null);
    setSearchTerm('');
    setDebouncedSearch('');
    setPendingQty(100);
    setPendingState(FoodState.Raw);
  };

  const handleRemoveIngredient = (index: number) => {
    setIngredients((prev) => prev.filter((_, i) => i !== index));
  };

  // ── Step 3: Submit ───────────────────────────────────────────────────────────
  const handleSave = () => {
    if (!recipeName.trim() || ingredients.length === 0) return;
    const formIngredients: CreateRecipeIngredient[] = ingredients.map((ing) => ({
      foodId: ing.food.id,
      quantityGrams: ing.quantityGrams,
      state: ing.state,
    }));
    createMutation.mutate(
      {
        name: recipeName.trim(),
        description: recipeDesc.trim() || undefined,
        category: recipeCategory,
        prepTimeMinutes: prepTime,
        cookTimeMinutes: cookTime,
        servings,
        ingredients: formIngredients,
      },
      { onSuccess: handleClose },
    );
  };

  const handleClose = () => {
    setCurrentStep(0);
    setIngredients([]);
    setSearchTerm('');
    setDebouncedSearch('');
    setPendingFood(null);
    setPendingQty(100);
    setPendingState(FoodState.Raw);
    setRecipeName('');
    setRecipeDesc('');
    setRecipeCategory(RecipeCategory.Custom);
    setPrepTime(5);
    setCookTime(0);
    setServings(1);
    onClose();
  };

  const steps = ['Ingredients', 'Details', 'Preview & Save'];

  return (
    <Modal
      open={open}
      onCancel={handleClose}
      title={
        <div className="create-recipe-modal__title">
          <span className="material-symbols-outlined">menu_book</span>
          Create Custom Recipe
        </div>
      }
      footer={null}
      width={680}
      className="create-recipe-modal"
      destroyOnClose
    >
      <div className="create-recipe-modal__body">
        <Steps
          current={currentStep}
          items={steps.map((s) => ({ title: s }))}
          className="create-recipe-modal__steps"
          size="small"
        />

        {/* ── Step 1: Select Ingredients ── */}
        {currentStep === 0 && (
          <div className="create-recipe-modal__step animate-fade-in">
            <div className="create-recipe-modal__search-area">
              <Search
                id="create-recipe-food-search"
                placeholder="Search ingredients..."
                value={searchTerm}
                onChange={(e) => { setSearchTerm(e.target.value); setPendingFood(null); }}
                allowClear
                size="large"
              />

              {debouncedSearch.length >= 1 && !pendingFood && (
                <div className="create-recipe-modal__results">
                  {isSearching ? (
                    <div className="create-recipe-modal__loading"><Spin size="small" /><span>Searching...</span></div>
                  ) : foodsData && foodsData.items.length > 0 ? (
                    <ul className="create-recipe-modal__food-list">
                      {foodsData.items.map((food) => (
                        <li
                          key={food.id}
                          className="create-recipe-modal__food-item"
                          onClick={() => setPendingFood(food)}
                          role="button"
                          tabIndex={0}
                          onKeyDown={(e) => e.key === 'Enter' && setPendingFood(food)}
                        >
                          <span className="create-recipe-modal__food-name">{food.name}</span>
                          <span className="mono create-recipe-modal__food-kcal">{Math.round(food.caloriesPer100g)} kcal/100g</span>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <Empty description="No foods found." image={Empty.PRESENTED_IMAGE_SIMPLE} />
                  )}
                </div>
              )}

              {pendingFood && (
                <div className="create-recipe-modal__pending animate-fade-in">
                  <div className="create-recipe-modal__pending-name">{pendingFood.name}</div>
                  <div className="create-recipe-modal__pending-controls">
                    <InputNumber
                      value={pendingQty}
                      onChange={(v) => setPendingQty(v ?? 100)}
                      min={1}
                      addonAfter="g"
                      size="middle"
                      style={{ width: 130 }}
                    />
                    <Select value={pendingState} onChange={setPendingState} size="middle" style={{ width: 110 }}>
                      {([FoodState.Raw, FoodState.Cooked, FoodState.Dry] as FoodState[]).map((fs) => (
                        <Option key={fs} value={fs}>{FOOD_STATE_LABELS[fs]}</Option>
                      ))}
                    </Select>
                    <Button type="primary" onClick={handleAddIngredient} size="middle">
                      Add
                    </Button>
                    <Button onClick={() => { setPendingFood(null); setSearchTerm(''); }} size="middle">
                      Cancel
                    </Button>
                  </div>
                </div>
              )}
            </div>

            {/* Ingredient list */}
            {ingredients.length > 0 && (
              <div className="create-recipe-modal__ingredient-list">
                <div className="create-recipe-modal__ingredient-header">
                  <span>Ingredient</span>
                  <span className="mono">Quantity</span>
                  <span className="mono">kcal</span>
                  <span />
                </div>
                {ingredients.map((ing, i) => {
                  const p = calcMacroPreview(ing.food, ing.quantityGrams, ing.state);
                  return (
                    <div key={i} className="create-recipe-modal__ingredient-row">
                      <div className="create-recipe-modal__ingredient-food">
                        <span className="create-recipe-modal__ingredient-name">{ing.food.name}</span>
                        <span className="create-recipe-modal__ingredient-state">{FOOD_STATE_LABELS[ing.state]}</span>
                      </div>
                      <span className="mono create-recipe-modal__ingredient-qty">{ing.quantityGrams}g</span>
                      <span className="mono create-recipe-modal__ingredient-kcal">{Math.round(p.calories)}</span>
                      <button
                        className="create-recipe-modal__remove-btn"
                        onClick={() => handleRemoveIngredient(i)}
                        type="button"
                      >
                        <span className="material-symbols-outlined">close</span>
                      </button>
                    </div>
                  );
                })}

                {/* Running total */}
                <div className="create-recipe-modal__running-total">
                  <span>Total ({ingredients.length} ingredients)</span>
                  <div className="create-recipe-modal__total-macros">
                    <span className="mono">{Math.round(totals.calories)} kcal</span>
                    <span className="mono">P {totals.protein.toFixed(1)}g</span>
                    <span className="mono">C {totals.carbs.toFixed(1)}g</span>
                    <span className="mono">F {totals.fat.toFixed(1)}g</span>
                  </div>
                </div>
              </div>
            )}

            <div className="create-recipe-modal__nav">
              <Button
                type="primary"
                onClick={() => setCurrentStep(1)}
                disabled={ingredients.length === 0}
                size="large"
              >
                Next: Recipe Details →
              </Button>
            </div>
          </div>
        )}

        {/* ── Step 2: Recipe Details ── */}
        {currentStep === 1 && (
          <div className="create-recipe-modal__step animate-fade-in">
            <Form layout="vertical" className="create-recipe-modal__form">
              <Form.Item label="Recipe Name" required>
                <Input
                  id="create-recipe-name-input"
                  value={recipeName}
                  onChange={(e) => setRecipeName(e.target.value)}
                  placeholder="e.g. High Protein Wrap"
                  size="large"
                />
              </Form.Item>
              <Form.Item label="Description">
                <Input.TextArea
                  id="create-recipe-desc-input"
                  value={recipeDesc}
                  onChange={(e) => setRecipeDesc(e.target.value)}
                  placeholder="Optional description..."
                  rows={2}
                />
              </Form.Item>
              <Form.Item label="Category">
                <Select
                  id="create-recipe-category-select"
                  value={recipeCategory}
                  onChange={setRecipeCategory}
                  size="large"
                  style={{ width: '100%' }}
                >
                  {([RecipeCategory.MuscleBuilding, RecipeCategory.FatLoss, RecipeCategory.Custom] as RecipeCategory[]).map((rc) => (
                    <Option key={rc} value={rc}>{RECIPE_CATEGORY_LABELS[rc]}</Option>
                  ))}
                </Select>
              </Form.Item>
              <div className="create-recipe-modal__row">
                <Form.Item label="Prep Time (min)" style={{ flex: 1 }}>
                  <InputNumber value={prepTime} onChange={(v) => setPrepTime(v ?? 0)} min={0} size="large" style={{ width: '100%' }} />
                </Form.Item>
                <Form.Item label="Cook Time (min)" style={{ flex: 1 }}>
                  <InputNumber value={cookTime} onChange={(v) => setCookTime(v ?? 0)} min={0} size="large" style={{ width: '100%' }} />
                </Form.Item>
                <Form.Item label="Servings" style={{ flex: 1 }}>
                  <InputNumber value={servings} onChange={(v) => setServings(v ?? 1)} min={1} size="large" style={{ width: '100%' }} />
                </Form.Item>
              </div>
            </Form>
            <div className="create-recipe-modal__nav">
              <Button onClick={() => setCurrentStep(0)} size="large">← Back</Button>
              <Button
                type="primary"
                onClick={() => setCurrentStep(2)}
                disabled={!recipeName.trim()}
                size="large"
              >
                Preview & Save →
              </Button>
            </div>
          </div>
        )}

        {/* ── Step 3: Preview & Save ── */}
        {currentStep === 2 && (
          <div className="create-recipe-modal__step animate-fade-in">
            <div className="create-recipe-modal__preview-card">
              <div className="create-recipe-modal__preview-header">
                <h3 className="create-recipe-modal__preview-name">{recipeName}</h3>
                <span className="create-recipe-modal__preview-category">
                  {RECIPE_CATEGORY_LABELS[recipeCategory]}
                </span>
              </div>
              {recipeDesc && <p className="create-recipe-modal__preview-desc">{recipeDesc}</p>}

              <div className="create-recipe-modal__preview-meta">
                <span><span className="material-symbols-outlined">schedule</span> {prepTime + cookTime} min</span>
                <span><span className="material-symbols-outlined">dining</span> {servings} serving{servings > 1 ? 's' : ''}</span>
              </div>

              {/* Macro summary */}
              <div className="create-recipe-modal__macro-summary">
                <div className="create-recipe-modal__macro-item create-recipe-modal__macro-item--kcal">
                  <span className="create-recipe-modal__macro-val">{Math.round(totals.calories)}</span>
                  <span className="create-recipe-modal__macro-lbl">kcal</span>
                </div>
                <div className="create-recipe-modal__macro-item">
                  <span className="create-recipe-modal__macro-val">{totals.protein.toFixed(1)}g</span>
                  <span className="create-recipe-modal__macro-lbl">Protein</span>
                </div>
                <div className="create-recipe-modal__macro-item">
                  <span className="create-recipe-modal__macro-val">{totals.carbs.toFixed(1)}g</span>
                  <span className="create-recipe-modal__macro-lbl">Carbs</span>
                </div>
                <div className="create-recipe-modal__macro-item">
                  <span className="create-recipe-modal__macro-val">{totals.fat.toFixed(1)}g</span>
                  <span className="create-recipe-modal__macro-lbl">Fat</span>
                </div>
              </div>

              {/* Ingredient list preview */}
              <div className="create-recipe-modal__preview-ingredients">
                {ingredients.map((ing, i) => (
                  <div key={i} className="create-recipe-modal__preview-ingredient-row">
                    <span>{ing.food.name}</span>
                    <span className="mono">{ing.quantityGrams}g ({FOOD_STATE_LABELS[ing.state]})</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="create-recipe-modal__nav">
              <Button onClick={() => setCurrentStep(1)} size="large">← Back</Button>
              <Button
                id="create-recipe-save-btn"
                type="primary"
                size="large"
                onClick={handleSave}
                loading={createMutation.isPending}
              >
                Save Recipe
              </Button>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
};

export default CreateRecipeModal;
