import React, { useState } from 'react';
import { Tabs, Skeleton, Empty, Select } from 'antd';
import { useTranslation } from 'react-i18next';
import { useGetRecipes, useQuickAddRecipe } from '../../../hooks/useRecipes/useRecipes';
import RecipeCard from '../../../components/RecipeCard/RecipeCard';
import CreateRecipeModal from '../../../components/CreateRecipeModal/CreateRecipeModal';
import { RecipeCategory, type RecipeDto } from '../../../types/Recipe';
import { MealType, MEAL_TYPE_LABELS } from '../../../types/Diary';
import { getTodayIso } from '../../../utils/date';
import './RecipeLibrary.scss';

const { Option } = Select;

const MEAL_TYPE_OPTIONS = [
  MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack,
];

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

interface RecipeGridProps {
  category?: RecipeCategory;
  isJokerRecipe?: boolean;
  today: string;
  targetMealType: MealType;
}

const RecipeGrid: React.FC<RecipeGridProps> = ({ category, isJokerRecipe, today, targetMealType }) => {
  const { t } = useTranslation(['athlete']);
  const { data, isLoading } = useGetRecipes({ category, isJokerRecipe, pageSize: 30 });
  const quickAddMutation = useQuickAddRecipe(today);

  const handleQuickAdd = (recipe: RecipeDto) => {
    quickAddMutation.mutate({ id: recipe.id, mealType: targetMealType });
  };

  if (isLoading) {
    return (
      <div className="recipe-grid">
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <div key={i} className="recipe-grid__skeleton">
            <Skeleton active paragraph={{ rows: 4 }} />
          </div>
        ))}
      </div>
    );
  }

  if (!data || data.items.length === 0) {
    return (
      <Empty
        description={
          isJokerRecipe
            ? t('athlete:recipeLibrary.emptyCoach')
            : t('athlete:recipeLibrary.emptyCustom')
        }
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        className="recipe-library__empty"
      />
    );
  }

  return (
    <div className="recipe-grid">
      {data.items.map((recipe) => (
        <RecipeCard
          key={recipe.id}
          recipe={recipe}
          onQuickAdd={handleQuickAdd}
          isAdding={quickAddMutation.isPending && quickAddMutation.variables?.id === recipe.id}
        />
      ))}
    </div>
  );
};

const RecipeLibrary: React.FC = () => {
  const { t } = useTranslation(['common', 'athlete']);
  const today = getTodayIso();
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [targetMealType, setTargetMealType] = useState<MealType>(MealType.Lunch);

  const recipeTabs: Array<{
    key: string;
    label: string;
    category?: RecipeCategory;
    isJokerRecipe?: boolean;
  }> = [
    {
      key: 'coach',
      label: t('athlete:recipeLibrary.categories.coach'),
      isJokerRecipe: true,
    },
    {
      key: String(RecipeCategory.Custom),
      label: t('athlete:recipeLibrary.categories.custom'),
      category: RecipeCategory.Custom,
      isJokerRecipe: false,
    },
  ];

  const tabItems = recipeTabs.map((tab) => ({
    key: tab.key,
    label: (
      <span className="recipe-library__tab-label">
        <span className={`recipe-library__tab-dot recipe-library__tab-dot--${tab.key}`} />
        {tab.label}
      </span>
    ),
    children: (
      <div className="recipe-library__tab-content">
        <RecipeGrid
          category={tab.category}
          isJokerRecipe={tab.isJokerRecipe}
          today={today}
          targetMealType={targetMealType}
        />
      </div>
    ),
  }));

  return (
    <div id="recipe-library-page" className="recipe-library animate-fade-in">
      {/* ── Page Header ── */}
      <div className="recipe-library__header">
        <div>
          <h1 className="recipe-library__title">{t('athlete:recipeLibrary.title')}</h1>
          <p className="recipe-library__sub">
            {t('athlete:recipeLibrary.sub')}
          </p>
        </div>

        <div className="recipe-library__actions">
          {/* Meal type picker for quick-add target */}
          <div className="recipe-library__meal-picker">
            <span className="recipe-library__meal-picker-label">{t('athlete:recipeLibrary.quickAddTo')}</span>
            <Select
              id="recipe-quick-add-meal-select"
              value={targetMealType}
              onChange={setTargetMealType}
              size="middle"
              style={{ width: 140 }}
            >
              {MEAL_TYPE_OPTIONS.map((mt) => (
                <Option key={mt} value={mt}>{getMealTypeLabel(mt, t)}</Option>
              ))}
            </Select>
          </div>

          <button
            id="create-recipe-btn"
            className="recipe-library__create-btn"
            onClick={() => setCreateModalOpen(true)}
          >
            <span className="material-symbols-outlined">add</span>
            {t('athlete:recipeLibrary.createRecipe')}
          </button>
        </div>
      </div>

      {/* ── Recipe Tabs ── */}
      <div className="recipe-library__tabs-card">
        <Tabs
          items={tabItems}
          className="recipe-library__tabs"
          defaultActiveKey="coach"
          size="large"
        />
      </div>

      {/* ── Create Recipe Modal ── */}
      <CreateRecipeModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        source="athlete"
      />
    </div>
  );
};

export default RecipeLibrary;
