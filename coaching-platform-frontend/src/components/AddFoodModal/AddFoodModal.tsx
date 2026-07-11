import React, { useState, useEffect } from 'react';
import { Modal, Input, Select, InputNumber, Button, Spin, Empty, Segmented, Space } from 'antd';
import { useTranslation } from 'react-i18next';
import { useSearchFoods } from '../../hooks/useFoods/useFoods';
import { useLogFood } from '../../hooks/useDiary/useDiary';
import { useGetRecipes } from '../../hooks/useRecipes/useRecipes';
import { calcMacroPreview } from '../../utils/macroCalc';
import { MealType, MEAL_TYPE_LABELS } from '../../types/Diary';
import type { FoodDto } from '../../types/Food';
import type { RecipeDto } from '../../types/Recipe';
import { RecipeCategory, RECIPE_CATEGORY_LABELS } from '../../types/Recipe';
import './AddFoodModal.scss';

const { Search } = Input;
const { Option } = Select;

interface AddFoodModalProps {
  open: boolean;
  onClose: () => void;
  date: string;
  defaultMealType?: MealType;
}

const getMealTypeLabel = (type: MealType, t: any) => {
  switch (type) {
    case MealType.Breakfast: return t('common:meals.breakfast');
    case MealType.Lunch: return t('common:meals.lunch');
    case MealType.Dinner: return t('common:meals.dinner');
    case MealType.Snack: return t('common:meals.snack');
    case MealType.Suhoor: return t('common:meals.suhoor');
    case MealType.Iftar: return t('common:meals.iftar');
    case MealType.PreWorkout: return t('common:meals.preWorkout');
    case MealType.PostWorkout: return t('common:meals.postWorkout');
    default: return MEAL_TYPE_LABELS[type];
  }
};

const getRecipeCategoryLabel = (category: any, t: any) => {
  switch (category) {
    case 0:
    case 'MuscleBuilding': return t('athlete:recipeLibrary.categories.muscleBuilding');
    case 1:
    case 'FatLoss': return t('athlete:recipeLibrary.categories.fatLoss');
    case 2:
    case 'Custom': return t('athlete:recipeLibrary.categories.custom');
    default: return RECIPE_CATEGORY_LABELS[category as RecipeCategory] || String(category);
  }
};

const getFoodCategoryLabel = (category: string, t: any) => {
  const key = `common:foodCategories.${category.toLowerCase()}`;
  return t(key, { defaultValue: category });
};

const AddFoodModal: React.FC<AddFoodModalProps> = ({
  open,
  onClose,
  date,
  defaultMealType = MealType.Breakfast,
}) => {
  const { t } = useTranslation(['common', 'athlete', 'coach']);
  const [activeTab, setActiveTab] = useState<'food' | 'recipe'>('food');
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [selectedFood, setSelectedFood] = useState<FoodDto | null>(null);
  const [selectedRecipe, setSelectedRecipe] = useState<RecipeDto | null>(null);
  const [quantity, setQuantity] = useState<number>(100);
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
    ? calcMacroPreview(selectedFood, quantity ?? 0)
    : null;

  const handleTabChange = (tab: 'food' | 'recipe') => {
    setActiveTab(tab);
    setSearchTerm('');
    setDebouncedSearch('');
    setSelectedFood(null);
    setSelectedRecipe(null);
    setQuantity(100);
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
          {t('athlete:components.addFoodModal.title')}
        </div>
      }
      footer={null}
      width={600}
      className="add-food-modal"
      destroyOnHidden
    >
      <div className="add-food-modal__body">
        {/* Meal type selector */}
        <div className="add-food-modal__field">
          <label className="add-food-modal__label">{t('athlete:components.addFoodModal.mealType')}</label>
          <Select
            value={mealType}
            onChange={setMealType}
            style={{ width: '100%' }}
            size="large"
            id="add-food-meal-type-select"
          >
            {mealTypeOptions.map((mt) => (
              <Option key={mt} value={mt}>{getMealTypeLabel(mt, t)}</Option>
            ))}
          </Select>
        </div>

        {/* Tab selection */}
        <Segmented
          options={[
            { label: t('athlete:components.addFoodModal.tabFood'), value: 'food', icon: <span className="material-symbols-outlined" style={{ fontSize: '16px', verticalAlign: 'middle', marginRight: '4px' }}>nutrition</span> },
            { label: t('athlete:components.addFoodModal.tabRecipe'), value: 'recipe', icon: <span className="material-symbols-outlined" style={{ fontSize: '16px', verticalAlign: 'middle', marginRight: '4px' }}>menu_book</span> }
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
            {activeTab === 'food' ? t('athlete:components.addFoodModal.tabFood') : t('athlete:components.addFoodModal.tabRecipe')}
          </label>
          <Search
            id="add-food-search-input"
            placeholder={activeTab === 'food' ? t('athlete:components.addFoodModal.searchFoodPlaceholder') : t('athlete:components.addFoodModal.searchRecipePlaceholder')}
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
                <span>{t('common:actions.loading')}</span>
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
                      <span className="add-food-modal__food-category">{getFoodCategoryLabel(food.category, t)}</span>
                    </div>
                    <div className="add-food-modal__food-macros">
                      <span className="mono">{Math.round(food.caloriesPer100g)} {t('common:units.kcal')}</span>
                      <span className="mono add-food-modal__macro-pill">P {food.proteinPer100g}g</span>
                      <span className="mono add-food-modal__macro-pill">C {food.carbsPer100g}g</span>
                      <span className="mono add-food-modal__macro-pill">F {food.fatPer100g}g</span>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty
                description={t('athlete:components.addFoodModal.emptyFood')}
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
                <span>{t('common:actions.loading')}</span>
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
                        {getRecipeCategoryLabel(recipe.category, t)} • {recipe.servings} {t('athlete:components.recipeCard.servings')}
                      </span>
                    </div>
                    <div className="add-food-modal__food-macros">
                      <span className="mono">{Math.round(recipe.totalCalories)} {t('common:units.kcal')}</span>
                      <span className="mono add-food-modal__macro-pill">P {Math.round(recipe.totalProtein)}g</span>
                      <span className="mono add-food-modal__macro-pill">C {Math.round(recipe.totalCarbs)}g</span>
                      <span className="mono add-food-modal__macro-pill">F {Math.round(recipe.totalFat)}g</span>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty
                description={t('athlete:components.addFoodModal.emptyRecipe')}
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
                <div className="add-food-modal__selected-category">{getFoodCategoryLabel(selectedFood.category, t)}</div>
              </div>
              <button
                className="add-food-modal__change-btn"
                onClick={() => {
                  setSelectedFood(null);
                  setSearchTerm('');
                }}
                type="button"
              >
                {t('coach:assignmentHub.changeBtn')}
              </button>
            </div>

            <div className="add-food-modal__row">
              <div className="add-food-modal__field" style={{ width: '100%' }}>
                <label className="add-food-modal__label">{t('athlete:components.addFoodModal.quantity')}</label>
                <Space.Compact style={{ width: '100%' }}>
                  <InputNumber
                    id="add-food-quantity-input"
                    value={quantity}
                    onChange={(v) => setQuantity(v ?? 0)}
                    min={1}
                    max={9999}
                    size="large"
                    style={{ width: '100%' }}
                  />
                  <span style={{
                    background: 'var(--ant-color-fill-alter, #fafafa)',
                    border: '1px solid var(--ant-color-border, #d9d9d9)',
                    borderLeft: 'none',
                    padding: '0 15px',
                    display: 'inline-flex',
                    alignItems: 'center',
                    borderRadius: '0 8px 8px 0',
                    color: 'var(--ant-color-text-description, rgba(0, 0, 0, 0.45))',
                    fontSize: '16px'
                  }}>
                    {t('common:units.grams')}
                  </span>
                </Space.Compact>
              </div>
            </div>

            {/* Macro preview */}
            {preview && quantity > 0 && (
              <div className="add-food-modal__preview">
                <div className="add-food-modal__preview-title">{t('athlete:components.addFoodModal.preview')}</div>
                <div className="add-food-modal__preview-grid">
                  <div className="add-food-modal__preview-item add-food-modal__preview-item--kcal">
                    <span className="add-food-modal__preview-value">{Math.round(preview.calories)}</span>
                    <span className="add-food-modal__preview-label">{t('common:units.kcal')}</span>
                  </div>
                  <div className="add-food-modal__preview-item">
                    <span className="add-food-modal__preview-value">{preview.protein.toFixed(1)}g</span>
                    <span className="add-food-modal__preview-label">{t('common:labels.protein')}</span>
                  </div>
                  <div className="add-food-modal__preview-item">
                    <span className="add-food-modal__preview-value">{preview.carbs.toFixed(1)}g</span>
                    <span className="add-food-modal__preview-label">{t('common:labels.carbs')}</span>
                  </div>
                  <div className="add-food-modal__preview-item">
                    <span className="add-food-modal__preview-value">{preview.fat.toFixed(1)}g</span>
                    <span className="add-food-modal__preview-label">{t('common:labels.fat')}</span>
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
              {logFoodMutation.isPending ? t('athlete:components.addFoodModal.adding') : t('athlete:components.addFoodModal.addBtn')}
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
                  {getRecipeCategoryLabel(selectedRecipe.category, t)} • {selectedRecipe.servings} {t('athlete:components.recipeCard.servings')}
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
                {t('coach:assignmentHub.changeBtn')}
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
                  <span>{t('athlete:components.createRecipeModal.prepTime')}: {selectedRecipe.prepTimeMinutes}{t('common:units.minutes')}</span>
                </div>
                <div className="add-food-modal__meta-item">
                  <span className="material-symbols-outlined">cooking</span>
                  <span>{t('athlete:components.createRecipeModal.cookTime')}: {selectedRecipe.cookTimeMinutes}{t('common:units.minutes')}</span>
                </div>
              </div>

              {selectedRecipe.ingredients && selectedRecipe.ingredients.length > 0 && (
                <div className="add-food-modal__ingredients-list-section">
                  <div className="add-food-modal__ingredients-title">{t('athlete:components.createRecipeModal.stepIngredients')}</div>
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
              <div className="add-food-modal__preview-title">{t('athlete:components.addFoodModal.preview')}</div>
              <div className="add-food-modal__preview-grid">
                <div className="add-food-modal__preview-item add-food-modal__preview-item--kcal">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalCalories)}</span>
                  <span className="add-food-modal__preview-label">{t('common:units.kcal')}</span>
                </div>
                <div className="add-food-modal__preview-item">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalProtein)}g</span>
                  <span className="add-food-modal__preview-label">{t('common:labels.protein')}</span>
                </div>
                <div className="add-food-modal__preview-item">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalCarbs)}g</span>
                  <span className="add-food-modal__preview-label">{t('common:labels.carbs')}</span>
                </div>
                <div className="add-food-modal__preview-item">
                  <span className="add-food-modal__preview-value">{Math.round(selectedRecipe.totalFat)}g</span>
                  <span className="add-food-modal__preview-label">{t('common:labels.fat')}</span>
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
              {logFoodMutation.isPending ? t('athlete:components.addFoodModal.adding') : t('athlete:components.addFoodModal.addBtn')}
            </Button>
          </div>
        )}
      </div>
    </Modal>
  );
};

export default AddFoodModal;
