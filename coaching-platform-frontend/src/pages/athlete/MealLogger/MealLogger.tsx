import React, { useState } from 'react';
import { Tabs, Skeleton, Empty, Tooltip, Popconfirm } from 'antd';
import { useGetDiary, useGetMacroSummary, useRemoveLogEntry } from '../../../hooks/useDiary/useDiary';
import MacroProgressBar from '../../../components/MacroProgressBar/MacroProgressBar';
import AddFoodModal from '../../../components/AddFoodModal/AddFoodModal';
import { MealType, MEAL_TYPE_LABELS, type MealLogDto } from '../../../types/Diary';
import { getTodayIso } from '../../../utils/date';
import './MealLogger.scss';

const MEAL_TABS = [
  { type: MealType.Breakfast, icon: 'wb_sunny' },
  { type: MealType.Lunch, icon: 'partly_cloudy_day' },
  { type: MealType.Dinner, icon: 'dinner_dining' },
  { type: MealType.Snack, icon: 'cookie' },
];

const getMealEntries = (diary: ReturnType<typeof useGetDiary>['data'], mealType: MealType): MealLogDto[] => {
  if (!diary) return [];
  switch (mealType) {
    case MealType.Breakfast: return diary.breakfast;
    case MealType.Lunch: return diary.lunch;
    case MealType.Dinner: return diary.dinner;
    case MealType.Snack: return diary.snack;
    default: return [];
  }
};

interface MealSectionProps {
  entries: MealLogDto[];
  isLoading: boolean;
  mealType: MealType;
  date: string;
  onAddFood: (mealType: MealType) => void;
}

const MealSection: React.FC<MealSectionProps> = ({ entries, isLoading, mealType, date, onAddFood }) => {
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
          description="Nothing logged yet"
          className="meal-section__empty"
        />
      ) : (
        <div className="meal-section__list">
          {/* Column headers */}
          <div className="meal-section__list-header">
            <span>Food</span>
            <span className="mono">P</span>
            <span className="mono">C</span>
            <span className="mono">F</span>
            <span className="mono">kcal</span>
            <span />
          </div>

          {entries.map((entry) => (
            <div key={entry.id} className="meal-section__row">
              <div className="meal-section__food-info">
                <span className="meal-section__food-name">
                  {entry.food?.name ?? entry.recipe?.name ?? 'Unknown'}
                </span>
                <span className="meal-section__food-qty mono">
                  {entry.quantityGrams}g
                </span>
              </div>
              <span className="mono meal-section__macro">{entry.protein.toFixed(1)}g</span>
              <span className="mono meal-section__macro">{entry.carbs.toFixed(1)}g</span>
              <span className="mono meal-section__macro">{entry.fat.toFixed(1)}g</span>
              <span className="mono meal-section__macro meal-section__macro--kcal">{Math.round(entry.calories)}</span>
              <Popconfirm
                title="Remove this entry?"
                onConfirm={() => removeMutation.mutate(entry.id)}
                okText="Remove"
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
            <span className="meal-section__subtotal-label">Subtotal</span>
            <span className="mono">{subtotals.p.toFixed(1)}g</span>
            <span className="mono">{subtotals.c.toFixed(1)}g</span>
            <span className="mono">{subtotals.f.toFixed(1)}g</span>
            <span className="mono meal-section__subtotal-kcal">{Math.round(subtotals.cal)} kcal</span>
            <span />
          </div>
        </div>
      )}

      <button
        className="meal-section__add-btn"
        onClick={() => onAddFood(mealType)}
        id={`add-food-btn-${mealType}`}
      >
        <span className="material-symbols-outlined">add</span>
        Add Food
      </button>
    </div>
  );
};

const MealLogger: React.FC = () => {
  const today = getTodayIso();
  const [addFoodModal, setAddFoodModal] = useState<{ open: boolean; mealType: MealType }>({
    open: false,
    mealType: MealType.Breakfast,
  });

  const { data: diary, isLoading: isDiaryLoading } = useGetDiary(today);
  const { data: summary, isLoading: isSummaryLoading } = useGetMacroSummary(today);

  const handleOpenAddFood = (mealType: MealType) => {
    setAddFoodModal({ open: true, mealType });
  };

  const tabItems = MEAL_TABS.map(({ type, icon }) => ({
    key: String(type),
    label: (
      <span className="meal-logger__tab-label">
        <span className="material-symbols-outlined">{icon}</span>
        {MEAL_TYPE_LABELS[type]}
      </span>
    ),
    children: (
      <MealSection
        entries={getMealEntries(diary, type)}
        isLoading={isDiaryLoading}
        mealType={type}
        date={today}
        onAddFood={handleOpenAddFood}
      />
    ),
  }));

  return (
    <div id="meal-logger-page" className="meal-logger animate-fade-in">
      {/* ── Page Header ── */}
      <div className="meal-logger__header">
        <div>
          <h1 className="meal-logger__title">Meal Logger</h1>
          <p className="meal-logger__sub">Track your daily nutrition</p>
        </div>
      </div>

      {/* ── Macro Summary Bar ── */}
      <div className="meal-logger__summary-card">
        <div className="meal-logger__summary-header">
          <span className="material-symbols-outlined">pie_chart</span>
          <h2 className="meal-logger__summary-title">Today's Progress</h2>
          {summary && (
            <span className="meal-logger__summary-remaining mono">
              {Math.round(summary.caloriesRemaining >= 0 ? summary.caloriesRemaining : 0)} kcal left
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
              label="Calories"
              consumed={summary.caloriesConsumed}
              target={summary.targetCalories}
              unit=" kcal"
            />
            <MacroProgressBar
              label="Protein"
              consumed={summary.proteinConsumed}
              target={summary.targetProtein}
              unit="g"
            />
            <MacroProgressBar
              label="Carbs"
              consumed={summary.carbsConsumed}
              target={summary.targetCarbs}
              unit="g"
            />
            <MacroProgressBar
              label="Fat"
              consumed={summary.fatConsumed}
              target={summary.targetFat}
              unit="g"
            />
          </div>
        ) : null}
      </div>

      {/* ── Meal Tabs ── */}
      <div className="meal-logger__tabs-card">
        <Tabs
          items={tabItems}
          className="meal-logger__tabs"
          defaultActiveKey={String(MealType.Breakfast)}
          size="large"
        />
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
