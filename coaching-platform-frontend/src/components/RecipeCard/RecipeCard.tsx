import React from 'react';
import { Tooltip } from 'antd';
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

const RecipeCard: React.FC<RecipeCardProps> = ({ recipe, onQuickAdd, onViewDetails, isAdding = false }) => {
  const totalTime = recipe.prepTimeMinutes + recipe.cookTimeMinutes;
  const categoryColor = CATEGORY_COLORS[recipe.category];

  return (
    <div className="recipe-card" onClick={() => onViewDetails?.(recipe)}>
      {/* Color band header */}
      <div
        className="recipe-card__band"
        style={{ backgroundColor: categoryColor }}
      >
        <span className="recipe-card__band-label">
          {RECIPE_CATEGORY_LABELS[recipe.category]}
        </span>
        {recipe.isJokerRecipe && (
          <span className="recipe-card__joker-badge">
            <span className="material-symbols-outlined">verified</span>
            Joker
          </span>
        )}
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
            {totalTime} min
          </span>
          <span className="recipe-card__servings">
            <span className="material-symbols-outlined">dining</span>
            {recipe.servings} serving{recipe.servings > 1 ? 's' : ''}
          </span>
        </div>

        {/* Macro badges */}
        <div className="recipe-card__macros">
          <Tooltip title="Calories">
            <span className="recipe-card__macro recipe-card__macro--kcal">
              {Math.round(recipe.totalCalories)} kcal
            </span>
          </Tooltip>
          <Tooltip title="Protein">
            <span className="recipe-card__macro">P {Math.round(recipe.totalProtein)}g</span>
          </Tooltip>
          <Tooltip title="Carbs">
            <span className="recipe-card__macro">C {Math.round(recipe.totalCarbs)}g</span>
          </Tooltip>
          <Tooltip title="Fat">
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
            {isAdding ? 'Adding...' : 'Quick Add to Diary'}
          </button>
        )}
      </div>
    </div>
  );
};

export default RecipeCard;
