import React, { useMemo, useState } from 'react';
import { Tabs, Skeleton, Empty, Tooltip, Popconfirm } from 'antd';
import { useTranslation } from 'react-i18next';
import { useGetDiary, useGetMacroSummary, useRemoveLogEntry } from '../../../hooks/useDiary/useDiary';
import MacroProgressBar from '../../../components/MacroProgressBar/MacroProgressBar';
import AddFoodModal from '../../../components/AddFoodModal/AddFoodModal';
import { MealType, MEAL_TYPE_LABELS, type MealLogDto } from '../../../types/Diary';
import { getTodayIso } from '../../../utils/date';
import './MealLogger.scss';

const MEAL_ICONS: Record<MealType, string> = {
  [MealType.Breakfast]: '☕',
  [MealType.Lunch]: '🍝',
  [MealType.Dinner]: '🥗',
  [MealType.Snack]: '🍎',
  [MealType.Suhoor]: '🌙',
  [MealType.Iftar]: '🍇',
  [MealType.PreWorkout]: '⚡',
  [MealType.PostWorkout]: '💪',
};

const getMealEntries = (diary: ReturnType<typeof useGetDiary>['data'], mealType: MealType): MealLogDto[] => {
  if (!diary) return [];
  switch (mealType) {
    case MealType.Breakfast: return diary.breakfast ?? [];
    case MealType.Lunch: return diary.lunch ?? [];
    case MealType.Dinner: return diary.dinner ?? [];
    case MealType.Snack: return diary.snack ?? [];
    case MealType.Suhoor: return diary.suhoor ?? [];
    case MealType.Iftar: return diary.iftar ?? [];
    case MealType.PreWorkout: return diary.preWorkout ?? [];
    case MealType.PostWorkout: return diary.postWorkout ?? [];
    default: return [];
  }
};

interface MealSectionProps {
  entries: MealLogDto[];
  isLoading: boolean;
  mealType: MealType;
  date: string;
  onAddFood?: (mealType: MealType) => void;
}

const MealSection: React.FC<MealSectionProps> = ({ entries, isLoading, mealType, date, onAddFood }) => {
  const { t } = useTranslation(['common', 'athlete']);
  const removeMutation = useRemoveLogEntry(date);

  const subtotals = entries.reduce(
    (acc, e) => ({ cal: acc.cal + e.calories, p: acc.p + e.protein, c: acc.c + e.carbs, f: acc.f + e.fat }),
    { cal: 0, p: 0, c: 0, f: 0 },
  );

  return (
    <div className="meal-section">
      {isLoading ? (
        <Skeleton active paragraph={{ rows: 3 }} />
      ) : entries.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={t('athlete:mealLogger.empty')}
          className="meal-section__empty"
        />
      ) : (
        <div className="meal-section__list">
          {/* Column headers */}
          <div className="meal-section__list-header">
            <span>{t('athlete:mealLogger.foodHeader')}</span>
            <span className="mono">P</span>
            <span className="mono">C</span>
            <span className="mono">F</span>
            <span className="mono">{t('athlete:mealLogger.kcalHeader')}</span>
            <span />
          </div>

          {entries.map((entry) => (
            <div key={entry.id} className="meal-section__row">
              <div className="meal-section__food-info">
                <span className="meal-section__food-name">
                  {entry.food?.name ?? entry.recipe?.name ?? t('common:status.unknown')}
                </span>
                <span className="meal-section__food-qty mono">
                  {entry.quantityGrams}g
                </span>
              </div>
              <div className="meal-section__row-macros">
                <span className="mono meal-section__macro">
                  <span className="meal-section__macro-label">P:</span>
                  {entry.protein.toFixed(1)}g
                </span>
                <span className="mono meal-section__macro">
                  <span className="meal-section__macro-label">C:</span>
                  {entry.carbs.toFixed(1)}g
                </span>
                <span className="mono meal-section__macro">
                  <span className="meal-section__macro-label">F:</span>
                  {entry.fat.toFixed(1)}g
                </span>
                <span className="mono meal-section__macro meal-section__macro--kcal">
                  {Math.round(entry.calories)}
                  <span className="meal-section__kcal-suffix"> {t('athlete:mealLogger.kcalHeader')}</span>
                </span>
              </div>
              <Popconfirm
                title={t('athlete:mealLogger.removeConfirm')}
                onConfirm={() => removeMutation.mutate(entry.id)}
                okText={t('athlete:mealLogger.remove')}
                okButtonProps={{ danger: true }}
              >
                <button className="meal-section__delete-btn" aria-label="Remove entry">
                  <span className="material-symbols-outlined">delete</span>
                </button>
              </Popconfirm>
            </div>
          ))}

          {/* Subtotals */}
          <div className="meal-section__subtotal">
            <span className="meal-section__subtotal-label">{t('athlete:mealLogger.subtotal')}</span>
            <div className="meal-section__subtotal-macros">
              <span className="mono">
                <span className="meal-section__macro-label">P:</span>
                {subtotals.p.toFixed(1)}g
              </span>
              <span className="mono">
                <span className="meal-section__macro-label">C:</span>
                {subtotals.c.toFixed(1)}g
              </span>
              <span className="mono">
                <span className="meal-section__macro-label">F:</span>
                {subtotals.f.toFixed(1)}g
              </span>
              <span className="mono meal-section__subtotal-kcal">
                {Math.round(subtotals.cal)} {t('athlete:mealLogger.kcalHeader')}
              </span>
            </div>
            <span className="meal-section__subtotal-spacer" />
          </div>
        </div>
      )}

      {onAddFood && (
        <button
          className="meal-section__add-btn"
          onClick={() => onAddFood(mealType)}
          id={`add-food-btn-${mealType}`}
        >
          <span className="material-symbols-outlined">add</span>
          {t('athlete:mealLogger.addFood')}
        </button>
      )}
    </div>
  );
};

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

const MealLogger: React.FC = () => {
  const { t } = useTranslation(['common', 'athlete']);
  const today = getTodayIso();
  const [addFoodModal, setAddFoodModal] = useState<{ open: boolean; mealType: MealType }>({
    open: false,
    mealType: MealType.Breakfast,
  });
  const [expandedMeal, setExpandedMeal] = useState<MealType | null>(null);

  const { data: diary, isLoading: isDiaryLoading } = useGetDiary(today);
  const { data: summary, isLoading: isSummaryLoading } = useGetMacroSummary(today);

  const handleOpenAddFood = (mealType: MealType) => {
    setAddFoodModal({ open: true, mealType });
    setExpandedMeal(mealType);
  };

  const toggleExpand = (mealType: MealType) => {
    setExpandedMeal(prev => prev === mealType ? null : mealType);
  };

  const activeMealTypes = useMemo(() => {
    const types = new Set([MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack]);
    if (diary) {
      if (diary.suhoor?.length) types.add(MealType.Suhoor);
      if (diary.iftar?.length) types.add(MealType.Iftar);
      if (diary.preWorkout?.length) types.add(MealType.PreWorkout);
      if (diary.postWorkout?.length) types.add(MealType.PostWorkout);
    }
    return Array.from(types);
  }, [diary]);

  return (
    <div id="meal-logger-page" className="meal-logger animate-fade-in">
      {/* ── Page Header ── */}
      <div className="meal-logger__header">
        <div>
          <h1 className="meal-logger__title">{t('athlete:mealLogger.title')}</h1>
          <p className="meal-logger__sub">{t('athlete:mealLogger.sub')}</p>
        </div>
      </div>

      {/* ── Macro Summary Bar ── */}
      <div className="meal-logger__summary-card">
        <div className="meal-logger__summary-header">
          <span className="material-symbols-outlined">pie_chart</span>
          <h2 className="meal-logger__summary-title">{t('athlete:mealLogger.todayProgress')}</h2>
          {summary && (
            <span className="meal-logger__summary-remaining mono">
              {t('athlete:mealLogger.kcalLeft', { count: Math.round(summary.caloriesRemaining >= 0 ? summary.caloriesRemaining : 0) })}
            </span>
          )}
        </div>
        {isSummaryLoading ? (
          <div className="meal-logger__summary-skeleton">
            {[1, 2, 3, 4].map((i) => (
              <Skeleton key={i} active paragraph={{ rows: 1, width: '100%' }} title={false} />
            ))}
          </div>
        ) : summary ? (
          <div className="meal-logger__summary-bars">
            <MacroProgressBar
              label={t('athlete:dashboard.dailyMacros.calories')}
              consumed={summary.caloriesConsumed}
              target={summary.targetCalories}
              unit={` ${t('athlete:mealLogger.kcalHeader')}`}
            />
            <MacroProgressBar
              label={t('athlete:dashboard.dailyMacros.protein')}
              consumed={summary.proteinConsumed}
              target={summary.targetProtein}
              unit="g"
            />
            <MacroProgressBar
              label={t('athlete:dashboard.dailyMacros.carbs')}
              consumed={summary.carbsConsumed}
              target={summary.targetCarbs}
              unit="g"
            />
            <MacroProgressBar
              label={t('athlete:dashboard.dailyMacros.fat')}
              consumed={summary.fatConsumed}
              target={summary.targetFat}
              unit="g"
            />
          </div>
        ) : null}
      </div>

      {/* ── Meals Dashboard Card ── */}
      <div className="meal-logger__dashboard-card">
        {activeMealTypes.map(type => {
          const entries = getMealEntries(diary, type);
          const totalCalories = entries.reduce((sum, e) => sum + e.calories, 0);
          
          const dailyTarget = summary?.targetCalories ?? 0;
          const percent = dailyTarget > 0 ? Math.min(100, Math.round((totalCalories / dailyTarget) * 100)) : 0;
          
          const isExpanded = expandedMeal === type;
          const foodNames = entries.map(e => e.food?.name ?? e.recipe?.name ?? '').filter(Boolean).join(', ');
          
          return (
            <div key={type} className={`meal-logger__row-wrapper ${isExpanded ? 'is-expanded' : ''}`}>
              <div 
                className="meal-logger__row" 
                role="button" 
                tabIndex={0} 
                onClick={() => toggleExpand(type)}
                onKeyDown={e => e.key === 'Enter' && toggleExpand(type)}
              >
                {/* SVG Progress Ring */}
                <div className="meal-logger__ring-container">
                  <svg width="48" height="48" viewBox="0 0 36 36" className="meal-logger__ring-svg">
                    <circle
                      className="meal-logger__ring-bg"
                      cx="18"
                      cy="18"
                      r="15.915"
                      fill="transparent"
                      stroke="rgba(255, 255, 255, 0.08)"
                      strokeWidth="2.8"
                    />
                    <circle
                      className="meal-logger__ring-fill"
                      cx="18"
                      cy="18"
                      r="15.915"
                      fill="transparent"
                      stroke="var(--color-success)"
                      strokeWidth="2.8"
                      strokeDasharray={`${percent} ${100 - percent}`}
                      strokeDashoffset="25"
                      strokeLinecap="round"
                    />
                  </svg>
                  <span className="meal-logger__ring-icon">{MEAL_ICONS[type] ?? '🍽️'}</span>
                </div>

                {/* Text Copy */}
                <div className="meal-logger__meal-info">
                  <div className="meal-logger__meal-title-row">
                    <h3>{getMealTypeLabel(type, t)}</h3>
                    <span className="material-symbols-outlined meal-logger__chevron">arrow_forward</span>
                  </div>
                  <span className="meal-logger__meal-calories mono">{Math.round(totalCalories)} {t('athlete:mealLogger.kcalHeader')}</span>
                  <span className="meal-logger__meal-summary" title={foodNames}>
                    {foodNames || t('athlete:mealLogger.emptyPlaceholder', { defaultValue: 'No foods logged yet' })}
                  </span>
                </div>

                {/* Quick Add Button */}
                <button
                  type="button"
                  className="meal-logger__quick-add-btn"
                  aria-label={`Add food to ${getMealTypeLabel(type, t)}`}
                  onClick={e => {
                    e.stopPropagation();
                    handleOpenAddFood(type);
                  }}
                >
                  <span className="material-symbols-outlined">add</span>
                </button>
              </div>

              {/* Collapsible Panel */}
              <div className="meal-logger__expanded-panel">
                <div>
                  <MealSection
                    entries={entries}
                    isLoading={isDiaryLoading}
                    mealType={type}
                    date={today}
                  />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* ── Add Food Modal ── */}
      <AddFoodModal
        open={addFoodModal.open}
        onClose={() => setAddFoodModal((s) => ({ ...s, open: false }))}
        date={today}
        defaultMealType={addFoodModal.mealType}
      />
    </div>
  );
};

export default MealLogger;
