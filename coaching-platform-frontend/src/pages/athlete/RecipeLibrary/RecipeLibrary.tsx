import React, { useState } from 'react';
import { Tabs, Skeleton, Empty, Modal, Radio } from 'antd';
import { useTranslation } from 'react-i18next';
import { useGetRecipes, useQuickAddRecipe } from '../../../hooks/useRecipes/useRecipes';
import RecipeCard from '../../../components/RecipeCard/RecipeCard';
import CreateRecipeModal from '../../../components/CreateRecipeModal/CreateRecipeModal';
import { RecipeCategory, type RecipeDto } from '../../../types/Recipe';
import { MealType, MEAL_TYPE_LABELS } from '../../../types/Diary';
import { getTodayIso } from '../../../utils/date';
import { useLanguage } from '../../../contexts/LanguageContext';
import './RecipeLibrary.scss';

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
  onQuickAdd: (recipe: RecipeDto) => void;
  addingRecipeId?: number;
}

const RecipeGrid: React.FC<RecipeGridProps> = ({ category, isJokerRecipe, onQuickAdd, addingRecipeId }) => {
  const { t } = useTranslation(['athlete']);
  const { data, isLoading } = useGetRecipes({ category, isJokerRecipe, pageSize: 30 });

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
          onQuickAdd={onQuickAdd}
          isAdding={addingRecipeId === recipe.id}
        />
      ))}
    </div>
  );
};

const RecipeLibrary: React.FC = () => {
  const { t } = useTranslation(['common', 'athlete']);
  const { language } = useLanguage();
  const today = getTodayIso();
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [selectedRecipe, setSelectedRecipe] = useState<RecipeDto | null>(null);
  const [selectedMealType, setSelectedMealType] = useState<MealType>(MealType.Lunch);
  const quickAddMutation = useQuickAddRecipe(today);

  const openMealPicker = (recipe: RecipeDto) => {
    setSelectedRecipe(recipe);
    setSelectedMealType(MealType.Lunch);
  };

  const closeMealPicker = () => {
    if (!quickAddMutation.isPending) setSelectedRecipe(null);
  };

  const handleQuickAdd = async () => {
    if (!selectedRecipe) return;

    try {
      await quickAddMutation.mutateAsync({
        id: selectedRecipe.id,
        mealType: selectedMealType,
      });
      setSelectedRecipe(null);
    } catch {
      // The mutation hook displays the error message.
    }
  };

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
          onQuickAdd={openMealPicker}
          addingRecipeId={quickAddMutation.isPending ? selectedRecipe?.id : undefined}
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
      <Modal
        open={selectedRecipe !== null}
        title={t('athlete:recipeLibrary.chooseMealTitle')}
        okText={t('athlete:recipeLibrary.addToDiary')}
        cancelText={t('common:actions.cancel')}
        onOk={handleQuickAdd}
        onCancel={closeMealPicker}
        confirmLoading={quickAddMutation.isPending}
        closable={!quickAddMutation.isPending}
        maskClosable={!quickAddMutation.isPending}
        className="recipe-library__meal-modal"
        destroyOnHidden
      >
        <p className="recipe-library__meal-modal-copy">
          {t('athlete:recipeLibrary.chooseMealDescription', {
            name: language === 'ar' && selectedRecipe?.nameAr
              ? selectedRecipe.nameAr
              : selectedRecipe?.name,
          })}
        </p>
        <Radio.Group
          value={selectedMealType}
          onChange={(event) => setSelectedMealType(event.target.value)}
          className="recipe-library__meal-options"
          optionType="button"
          buttonStyle="solid"
        >
          {MEAL_TYPE_OPTIONS.map((mealType) => (
            <Radio.Button key={mealType} value={mealType}>
              {getMealTypeLabel(mealType, t)}
            </Radio.Button>
          ))}
        </Radio.Group>
      </Modal>

      <CreateRecipeModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        source="athlete"
      />
    </div>
  );
};

export default RecipeLibrary;
