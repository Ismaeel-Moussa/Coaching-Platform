import React, { useState, useEffect } from 'react';
import { Modal, Input, Select, InputNumber, Button, Spin, Empty, Segmented } from 'antd';
import { useSearchFoods } from '../../hooks/useFoods/useFoods';
import { useLogFood } from '../../hooks/useDiary/useDiary';
import { useGetRecipes } from '../../hooks/useRecipes/useRecipes';
import { calcMacroPreview } from '../../utils/macroCalc';
import { MealType, FoodState, MEAL_TYPE_LABELS, FOOD_STATE_LABELS } from '../../types/Diary';
import type { FoodDto } from '../../types/Food';
import type { RecipeDto } from '../../types/Recipe';
import { RECIPE_CATEGORY_LABELS } from '../../types/Recipe';
import './AddFoodModal.scss';

const { Search } = Input;
const { Option } = Select;

interface AddFoodModalProps {
  open: boolean;
  onClose: () => void;
  date: string;
  defaultMealType?: MealType;
}

const AddFoodModal: React.FC<AddFoodModalProps> = ({
  open,
  onClose,
  date,
  defaultMealType = MealType.Breakfast,
}) => {
  const [activeTab, setActiveTab] = useState<'food' | 'recipe'>('food');
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [selectedFood, setSelectedFood] = useState<FoodDto | null>(null);
  const [selectedRecipe, setSelectedRecipe] = useState<RecipeDto | null>(null);
  const [quantity, setQuantity] = useState<number>(100);
  const [state, setState] = useState<FoodState>(FoodState.Raw);
  const [mealType, setMealType] = useState<MealType>(defaultMealType);

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchTerm), 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const { data: foodsData, isLoading: isSearching } = useSearchFoods(
    { search: debouncedSearch, pageSize: 20 },
    open && activeTab === 'food' && debouncedSearch.length >= 1,
  );

  const { data: recipesData, isLoading: isSearchingRecipes } = useGetRecipes(
    { search: debouncedSearch, pageSize: 20 },
    open && activeTab === 'recipe'
  );

  const logFoodMutation = useLogFood(date);

  const preview = selectedFood
    ? calcMacroPreview(selectedFood, quantity ?? 0, state)
    : null;

  const handleTabChange = (tab: 'food' | 'recipe') => {
    setActiveTab(tab);
    setSearchTerm('');
    setDebouncedSearch('');
    setSelectedFood(null);
    setSelectedRecipe(null);
    setQuantity(100);
    setState(FoodState.Raw);
  };

  const handleAddFood = () => {
    if (activeTab === 'food') {
      if (!selectedFood || !quantity) return;
      logFoodMutation.mutate(
        {
          date,
          mealType,
          foodId: selectedFood.id,
          recipeId: null,
          quantityGrams: quantity,
          state,
        },
        {
          onSuccess: () => {
            handleClose();
          },
        },
      );
    } else {
      if (!selectedRecipe) return;
      const totalWeight = selectedRecipe.ingredients?.reduce((sum, i) => sum + i.quantityGrams, 0) || 100;
      logFoodMutation.mutate(
        {
          date,
          mealType,
          foodId: null,
          recipeId: selectedRecipe.id,
          quantityGrams: totalWeight,
          state: FoodState.Raw,
        },
        {
          onSuccess: () => {
            handleClose();
          },
        },
      );
    }
  };

  const handleClose = () => {
    setSearchTerm('');
    setDebouncedSearch('');
    setSelectedFood(null);
    setSelectedRecipe(null);
    setQuantity(100);
    setState(FoodState.Raw);
    setMealType(defaultMealType);
    setActiveTab('food');
    onClose();
  };

  const mealTypeOptions = [
    MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack,
    MealType.Suhoor, MealType.Iftar, MealType.PreWorkout, MealType.PostWorkout,
  ];

  return (
    <Modal
      open={open}
      onCancel={handleClose}
      title={
        <div className="add-food-modal__title">
          <span className="material-symbols-outlined">restaurant</span>
          Add to Diary
        </div>
      }
      footer={null}
      width={600}
      className="add-food-modal"
      destroyOnClose
    >
      <div className="add-food-modal__body">
        {/* Meal type selector */}
        <div className="add-food-modal__field">
          <label className="add-food-modal__label">Meal</label>
          <Select
            value={mealType}
            onChange={setMealType}
            style={{ width: '100%' }}
            size="large"
            id="add-food-meal-type-select"
          >
            {mealTypeOptions.map((mt) => (
              <Option key={mt} value={mt}>{MEAL_TYPE_LABELS[mt]}</Option>
            ))}
          </Select>
        </div>

        {/* Tab selection */}
        <Segmented
          options={[
            { label: 'Food', value: 'food', icon: <span className="material-symbols-outlined" style={{ fontSize: '16px', verticalAlign: 'middle', marginRight: '4px' }}>nutrition</span> },
            { label: 'Recipe', value: 'recipe', icon: <span className="material-symbols-outlined" style={{ fontSize: '16px', verticalAlign: 'middle', marginRight: '4px' }}>menu_book</span> }
          ]}
          value={activeTab}
          onChange={(val) => handleTabChange(val as 'food' | 'recipe')}
          block
          size="large"
          className="add-food-modal__segmented"
        />

        {/* Search Field */}
        <div className="add-food-modal__field">
          <label className="add-food-modal__label">
            {activeTab === 'food' ? 'Search Food' : 'Search Recipe'}
          </label>
          <Search
            id="add-food-search-input"
            placeholder={activeTab === 'food' ? 'e.g. Chicken Breast, Oats, Rice...' : 'e.g. Protein Pancakes, Oats Bowl...'}
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              if (activeTab === 'food') {
                setSelectedFood(null);
              } else {
                setSelectedRecipe(null);
              }
            }}
            size="large"
            allowClear
          />
        </div>

        {/* Food Search results */}
        {activeTab === 'food' && debouncedSearch.length >= 1 && !selectedFood && (
          <div className="add-food-modal__results">
            {isSearching ? (
              <div className="add-food-modal__loading">
                <Spin size="small" />
                <span>Searching foods...</span>
              </div>
            ) : foodsData && foodsData.items.length > 0 ? (
              <ul className="add-food-modal__list">
                {foodsData.items.map((food) => (
                  <li
                    key={food.id}
                    className="add-food-modal__list-item"
                    onClick={() => setSelectedFood(food)}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => e.key === 'Enter' && setSelectedFood(food)}
                  >
                    <div className="add-food-modal__food-info">
                      <span className="add-food-modal__food-name">{food.name}</span>
                      <span className="add-food-modal__food-category">{food.category}</span>
                    </div>
                    <div className="add-food-modal__food-macros">
                      <span className="mono">{Math.round(food.caloriesPer100g)} kcal</span>
                      <span className="mono add-food-modal__macro-pill">P {food.proteinPer100g}g</span>
                      <span className="mono add-food-modal__macro-pill">C {food.carbsPer100g}g</span>
                      <span className="mono add-food-modal__macro-pill">F {food.fatPer100g}g</span>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty
                description="No foods found. Try a different search term."
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </div>
        )}

        {/* Recipe search results */}
        {activeTab === 'recipe' && !selectedRecipe && (
          <div className="add-food-modal__results">
            {isSearchingRecipes ? (
              <div className="add-food-modal__loading">
                <Spin size="small" />
                <span>Searching recipes...</span>
              </div>
            ) : recipesData && recipesData.items.length > 0 ? (
              <ul className="add-food-modal__list">
                {recipesData.items.map((recipe) => (
                  <li
                    key={recipe.id}
                    className="add-food-modal__list-item"
                    onClick={() => setSelectedRecipe(recipe)}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => e.key === 'Enter' && setSelectedRecipe(recipe)}
                  >
                    <div className="add-food-modal__food-info">
                      <span className="add-food-modal__food-name">{recipe.name}</span>
                      <span className="add-food-modal__food-category">
                        {RECIPE_CATEGORY_LABELS[recipe.category]} • {recipe.servings} Servings
                      </span>
                    </div>
                    <div className="add-food-modal__food-macros">
                      <span className="mono">{Math.round(recipe.totalCalories)} kcal</span>
                      <span className="mono add-food-modal__macro-pill">P {Math.round(recipe.totalProtein)}g</span>
                      <span className="mono add-food-modal__macro-pill">C {Math.round(recipe.totalCarbs)}g</span>
                      <span className="mono add-food-modal__macro-pill">F {Math.round(recipe.totalFat)}g</span>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty
                description="No recipes found. Try a different search term."
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </div>
        )}

        {/* Selected food configuration */}
        {activeTab === 'food' && selectedFood && (
          <div className="add-food-modal__config animate-fade-in">
            <div className="add-food-modal__selected-food">
              <span className="material-symbols-outlined">check_circle</span>
              <div>
                <div className="add-food-modal__selected-name">{selectedFood.name}</div>
                <div className="add-food-modal__selected-category">{selectedFood.category}</div>
              </div>
              <button
                className="add-food-modal__change-btn"
                onClick={() => {
                  setSelectedFood(null);
                  setSearchTerm('');
                }}
                type="button"
              >
                Change
              </button>
            </div>

            <div className="add-food-modal__row">
              <div className="add-food-modal__field add-food-modal__field--half">
                <label className="add-food-modal__label">Quantity (grams)</label>
                <InputNumber
                  id="add-food-quantity-input"
                  value={quantity}
                  onChange={(v) => setQuantity(v ?? 0)}
                  min={1}
                  max={9999}
                  size="large"
                  style={{ width: '100%' }}
                  addonAfter="g"
                />
              </div>
              <div className="add-food-modal__field add-food-modal__field--half">
                <label className="add-food-modal__label">State</label>
                <Select
                  id="add-food-state-select"
                  value={state}
                  onChange={setState}
                  size="large"
                  style={{ width: '100%' }}
                >
                  {([FoodState.Raw, FoodState.Cooked, FoodState.Dry] as FoodState[]).map((fs) => (
                    <Option key={fs} value={fs}>{FOOD_STATE_LABELS[fs]}</Option>
                  ))}
                </Select>
              </div>
            </div>

            {/* Macro preview */}
            {preview && quantity > 0 && (
              <div className="add-food-modal__preview">
                <div className="add-food-modal__preview-title">Macro Preview</div>
                <div className="add-food-modal__preview-grid">
                  <div className="add-food-modal__preview-item add-food-modal__preview-item--kcal">
                    <span className="add-food-modal__preview-value">{Math.round(preview.calories)}</span>
                    <span className="add-food-modal__preview-label">kcal</span>
                  </div>
                  <div className="add-food-modal__preview-item">
                    <span className="add-food-modal__preview-value">{preview.protein.toFixed(1)}g</span>
                    <span className="add-food-modal__preview-label">Protein</span>
                  </div>
                  <div className="add-food-modal__preview-item">
                    <span className="add-food-modal__preview-value">{preview.carbs.toFixed(1)}g</span>
                    <span className="add-food-modal__preview-label">Carbs</span>
                  </div>
                  <div className="add-food-modal__preview-item">
                    <span className="add-food-modal__preview-value">{preview.fat.toFixed(1)}g</span>
                    <span className="add-food-modal__preview-label">Fat</span>
                  </div>
                </div>
              </div>
            )}

            <Button
              id="add-food-submit-btn"
              type="primary"
              size="large"
              block
              onClick={handleAddFood}
              loading={logFoodMutation.isPending}
              disabled={!selectedFood || !quantity}
              className="add-food-modal__submit"
            >
              Add to Diary
            </Button>
          </div>
        )}

        {/* Selected recipe configuration */}
        {activeTab === 'recipe' && selectedRecipe && (
          <div className="add-food-modal__config animate-fade-in">
            <div className="add-food-modal__selected-food add-food-modal__selected-food--recipe">
              <span className="material-symbols-outlined">check_circle</span>
              <div style={{ flex: 1 }}>
                <div className="add-food-modal__selected-name">{selectedRecipe.name}</div>
                <div className="add-food-modal__selected-category">
                  {RECIPE_CATEGORY_LABELS[selectedRecipe.category]} • {selectedRecipe.servings} Servings
                </div>
              </div>
              <button
                className="add-food-modal__change-btn"
                onClick={() => {
                  setSelectedRecipe(null);
                  setSearchTerm('');
                }}
                type="button"
              >
                Change
              </button>
            </div>

            {/* Recipe details */}
            <div className="add-food-modal__recipe-details">
              {selectedRecipe.description && (
                <div className="add-food-modal__recipe-desc">
                  {selectedRecipe.description}
                </div>
              )}
              
              <div className="add-food-modal__recipe-meta">
                <div className="add-food-modal__meta-item">
                  <span className="material-symbols-outlined">schedule</span>
                  <span>Prep: {selectedRecipe.prepTimeMinutes}m</span>
                </div>
                <div className="add-food-modal__meta-item">
                  <span className="material-symbols-outlined">cooking</span>
                  <span>Cook: {selectedRecipe.cookTimeMinutes}m</span>
                </div>
              </div>

              {selectedRecipe.ingredients && selectedRecipe.ingredients.length > 0 && (
                <div className="add-food-modal__ingredients-list-section">
                  <div className="add-food-modal__ingredients-title">Ingredients</div>
                  <ul className="add-food-modal__ingredients-list">
                    {selectedRecipe.ingredients.map((ing, idx) => (
                      <li key={idx} className="add-food-modal__ingredient-item">
                        <span>{ing.foodName}</span>
                        <span className="mono">{ing.quantityGrams}g</span>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>

            {/* Macro preview */}
            <div className="add-food-modal__preview">
              <div className="add-food-modal__preview-title">Total Recipe Macros</div>
              <div className="add-food-modal__preview-grid">
                <div className="add-food-modal__preview-item add-food-modal__preview-item--kcal">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalCalories)}</span>
                  <span className="add-food-modal__preview-label">kcal</span>
                </div>
                <div className="add-food-modal__preview-item">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalProtein)}g</span>
                  <span className="add-food-modal__preview-label">Protein</span>
                </div>
                <div className="add-food-modal__preview-item">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalCarbs)}g</span>
                  <span className="add-food-modal__preview-label">Carbs</span>
                </div>
                <div className="add-food-modal__preview-item">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalFat)}g</span>
                  <span className="add-food-modal__preview-label">Fat</span>
                </div>
              </div>
            </div>

            <Button
              id="add-recipe-submit-btn"
              type="primary"
              size="large"
              block
              onClick={handleAddFood}
              loading={logFoodMutation.isPending}
              className="add-food-modal__submit"
            >
              Add Recipe to Diary
            </Button>
          </div>
        )}
      </div>
    </Modal>
  );
};

export default AddFoodModal;
