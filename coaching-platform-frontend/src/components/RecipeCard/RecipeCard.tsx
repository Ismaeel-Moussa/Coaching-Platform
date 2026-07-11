import React from 'react';
import { Tooltip } from 'antd';
import { useTranslation } from 'react-i18next';
import { RecipeCategory, RECIPE_CATEGORY_LABELS, type RecipeDto } from '../../types/Recipe';
import { MealType } from '../../types/Diary';
import './RecipeCard.scss';

interface RecipeCardProps {
  recipe: RecipeDto;
  onQuickAdd?: (recipe: RecipeDto) => void;
  onViewDetails?: (recipe: RecipeDto) => void;
  isAdding?: boolean;
}

const CATEGORY_COLORS: Record<RecipeCategory, string> = {
  [RecipeCategory.MuscleBuilding]: '#0b132b', // Navy
  [RecipeCategory.FatLoss]: '#12b76a',         // Green
  [RecipeCategory.Custom]: '#785900',           // Gold text
};

const getRecipeCategoryLabel = (category: RecipeCategory, t: any) => {
  switch (category) {
    case RecipeCategory.MuscleBuilding: return t('athlete:recipeLibrary.categories.muscleBuilding');
    case RecipeCategory.FatLoss: return t('athlete:recipeLibrary.categories.fatLoss');
    case RecipeCategory.Custom: return t('athlete:recipeLibrary.categories.custom');
    default: return RECIPE_CATEGORY_LABELS[category] || String(category);
  }
};

const RecipeCard: React.FC<RecipeCardProps> = ({ recipe, onQuickAdd, onViewDetails, isAdding = false }) => {
  const { t } = useTranslation(['common', 'athlete']);
  const totalTime = recipe.prepTimeMinutes + recipe.cookTimeMinutes;
  const categoryColor = CATEGORY_COLORS[recipe.category];

  return (
    <div className="recipe-card" onClick={() => onViewDetails?.(recipe)}>
      {/* Recipe Image or Placeholder with Overlays */}
      <div className="recipe-card__image-wrapper">
        {recipe.imageUrl ? (
          <img src={recipe.imageUrl} alt={recipe.name} className="recipe-card__image" />
        ) : (
          <div className="recipe-card__image-placeholder">
            <span className="material-symbols-outlined">restaurant</span>
          </div>
        )}

        {/* Floating Badges Overlay */}
        <div className="recipe-card__overlay">
          <span
            className="recipe-card__category-badge"
            style={{ backgroundColor: categoryColor }}
          >
            {getRecipeCategoryLabel(recipe.category, t)}
          </span>
          <div className="recipe-card__band-badges">
            {recipe.videoUrl && (
              <a
                className="recipe-card__video-badge"
                href={recipe.videoUrl}
                target="_blank"
                rel="noopener noreferrer"
                onClick={(e) => e.stopPropagation()}
                title={t('athlete:components.recipeCard.watch')}
              >
                <span className="material-symbols-outlined">smart_display</span>
                {t('athlete:components.recipeCard.watch')}
              </a>
            )}
          </div>
        </div>
      </div>

      {/* Body */}
      <div className="recipe-card__body">
        <h3 className="recipe-card__name">{recipe.name}</h3>
        {recipe.description && (
          <p className="recipe-card__description">{recipe.description}</p>
        )}

        {/* Time row */}
        <div className="recipe-card__meta">
          <span className="recipe-card__time">
            <span className="material-symbols-outlined">schedule</span>
            {totalTime} {t('common:units.minutes')}
          </span>
          <span className="recipe-card__servings">
            <span className="material-symbols-outlined">dining</span>
            {recipe.servings} {recipe.servings > 1 ? t('common:units.servings') : t('common:units.serving')}
          </span>
        </div>

        {/* Macro badges */}
        <div className="recipe-card__macros">
          <Tooltip title={t('athlete:dashboard.dailyMacros.calories')}>
            <span className="recipe-card__macro recipe-card__macro--kcal">
              {Math.round(recipe.totalCalories)} {t('common:units.kcal')}
            </span>
          </Tooltip>
          <Tooltip title={t('athlete:dashboard.dailyMacros.protein')}>
            <span className="recipe-card__macro">P {Math.round(recipe.totalProtein)}g</span>
          </Tooltip>
          <Tooltip title={t('athlete:dashboard.dailyMacros.carbs')}>
            <span className="recipe-card__macro">C {Math.round(recipe.totalCarbs)}g</span>
          </Tooltip>
          <Tooltip title={t('athlete:dashboard.dailyMacros.fat')}>
            <span className="recipe-card__macro">F {Math.round(recipe.totalFat)}g</span>
          </Tooltip>
        </div>

        {/* Quick Add */}
        {onQuickAdd && (
          <button
            className={`recipe-card__quick-add ${isAdding ? 'recipe-card__quick-add--loading' : ''}`}
            onClick={(e) => {
              e.stopPropagation();
              if (isAdding) return;
              onQuickAdd(recipe);
            }}
            disabled={isAdding}
            type="button"
          >
            <span className={`material-symbols-outlined ${isAdding ? 'animate-spin' : ''}`}>
              {isAdding ? 'sync' : 'add_circle'}
            </span>
            {isAdding ? t('athlete:components.recipeCard.adding') : t('athlete:components.recipeCard.quickAdd')}
          </button>
        )}
      </div>
    </div>
  );
};

export default RecipeCard;
