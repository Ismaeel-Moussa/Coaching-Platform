import React from 'react';
import { Card, Tag, Empty, Spin, Row, Col, Divider } from 'antd';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useGetDailyLog } from '../../hooks/useAthlete/useAthlete';
import MacroProgressBar from '../MacroProgressBar/MacroProgressBar';
import { MealType } from '../../types/Diary';
import { WorkoutStatus } from '../../types/Workout';
import './DailyLogHistoryView.scss';

interface DailyLogHistoryViewProps {
  athleteId: number;
  date: string; // ISO format: YYYY-MM-DD
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
    default: return 'Meal';
  }
};

const getStatusTagColor = (status: string) => {
  switch (status) {
    case WorkoutStatus.Completed: return 'success';
    case WorkoutStatus.InProgress: return 'processing';
    case WorkoutStatus.Missed: return 'error';
    default: return 'default';
  }
};

const getStatusLabel = (status: string, t: any) => {
  switch (status) {
    case WorkoutStatus.Completed: return t('athlete:dashboard.workout.completed');
    case WorkoutStatus.InProgress: return t('athlete:dashboard.workout.inProgress');
    case WorkoutStatus.Missed: return t('athlete:dashboard.workout.missed');
    default: return status;
  }
};

const DailyLogHistoryView: React.FC<DailyLogHistoryViewProps> = ({ athleteId, date }) => {
  const { t, i18n } = useTranslation(['common', 'athlete', 'coach']);
  const { data, isLoading, error } = useGetDailyLog(athleteId, date);

  const todayStr = dayjs().format('YYYY-MM-DD');
  const isPastDate = date < todayStr;

  if (isLoading) {
    return (
      <div className="daily-history-view__loader">
        <Spin size="large" />
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="daily-history-view__error">
        <Empty description={t('athlete:history.errorLoading')} />
      </div>
    );
  }

  const { workout, nutrition, supplements } = data;

  const hasMeals = nutrition && (
    nutrition.breakfast.length > 0 ||
    nutrition.lunch.length > 0 ||
    nutrition.dinner.length > 0 ||
    nutrition.snack.length > 0 ||
    nutrition.suhoor.length > 0 ||
    nutrition.iftar.length > 0 ||
    nutrition.preWorkout.length > 0 ||
    nutrition.postWorkout.length > 0
  );

  return (
    <div className="daily-history-view">
      <Row gutter={[24, 24]}>
        {/* ── WORKOUT CARD ── */}
        <Col xs={24} lg={12}>
          <Card 
            title={
              <div className="daily-history-view__card-header">
                <span className="material-symbols-outlined icon text-gold">fitness_center</span>
                <span>{t('athlete:history.workoutTab')}</span>
              </div>
            }
            className="daily-history-view__card"
          >
            {workout && workout.day ? (
              <div className="daily-history-view__workout">
                <div className="daily-history-view__workout-header">
                  <h3 className="daily-history-view__workout-title">
                    {workout.day.dayLabel} 
                    {workout.day.isRestDay && <span className="rest-tag">({t('athlete:history.restDay', 'Rest Day')})</span>}
                  </h3>
                  <div className="daily-history-view__workout-meta">
                    <Tag color={getStatusTagColor(workout.status)}>
                      {getStatusLabel(workout.status, t)}
                    </Tag>
                    {workout.completedAt && (
                      <span className="completed-time mono">
                        {t('athlete:history.completedAt', { time: new Date(workout.completedAt).toLocaleTimeString(i18n.language, { hour: '2-digit', minute: '2-digit' }) })}
                      </span>
                    )}
                  </div>
                </div>

                {!workout.day.isRestDay && (
                  <div className="daily-history-view__workout-sections">
                    {[
                      { name: t('athlete:workoutLogger.warmup', 'Warm-Up'), exercises: workout.day.warmUp },
                      { name: t('athlete:workoutLogger.main', 'Main Exercises'), exercises: workout.day.main },
                      { name: t('athlete:workoutLogger.cooldown', 'Cool-Down'), exercises: workout.day.coolDown }
                    ].map(section => {
                      if (section.exercises.length === 0) return null;
                      return (
                        <div key={section.name} className="daily-history-view__workout-section">
                          <h4 className="section-title">{section.name}</h4>
                          <div className="daily-history-view__exercise-list">
                            {section.exercises.map(te => {
                              const exerciseLogs = workout.loggedSets.filter(s => s.exerciseId === te.exercise.id);
                              return (
                                <div key={te.id} className="daily-history-view__exercise-item">
                                  <div className="exercise-info">
                                    <div className="exercise-name">{te.exercise.name}</div>
                                    <div className="exercise-target">
                                      {t('athlete:history.targetReps', { sets: te.targetSets, reps: te.targetReps })}
                                      {te.progressiveOverloadTargetKg && ` • ${t('athlete:history.progressiveOverload', { weight: te.progressiveOverloadTargetKg })}`}
                                    </div>
                                  </div>

                                  {exerciseLogs.length > 0 ? (
                                    <div className="exercise-sets">
                                      <div className="sets-label">{t('athlete:history.setsLogged')}</div>
                                      <div className="sets-grid mono">
                                        {exerciseLogs.map(set => (
                                          <div key={set.id} className={`set-pill ${set.isCompleted ? 'set-pill--completed' : ''}`}>
                                            <span className="num">{set.setNumber}</span>
                                            <span className="val">{set.weightKg}kg × {set.reps}</span>
                                          </div>
                                        ))}
                                      </div>
                                    </div>
                                  ) : (
                                    <div className="no-sets text-secondary">{t('athlete:workoutLogger.noSets', 'No sets logged')}</div>
                                  )}
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            ) : (
              <Empty description={t('athlete:history.noWorkout')} style={{ padding: '24px 0' }} />
            )}
          </Card>
        </Col>

        {/* ── NUTRITION & MACROS CARD ── */}
        <Col xs={24} lg={12}>
          <Card 
            title={
              <div className="daily-history-view__card-header">
                <span className="material-symbols-outlined icon text-gold">restaurant</span>
                <span>{t('athlete:history.nutritionTab')}</span>
              </div>
            }
            className="daily-history-view__card"
          >
            {nutrition ? (
              <div className="daily-history-view__nutrition">
                {/* Macros Progress List */}
                <div className="daily-history-view__macros-list">
                  <MacroProgressBar
                    label={t('athlete:dashboard.dailyMacros.calories')}
                    consumed={nutrition.totalCalories}
                    target={nutrition.targetCalories}
                    unit=" kcal"
                  />
                  <Row gutter={16} style={{ marginTop: '16px' }}>
                    <Col span={8}>
                      <MacroProgressBar
                        label={t('athlete:dashboard.dailyMacros.protein')}
                        consumed={nutrition.totalProtein}
                        target={nutrition.targetProtein}
                        unit="g"
                        color="var(--color-success)"
                      />
                    </Col>
                    <Col span={8}>
                      <MacroProgressBar
                        label={t('athlete:dashboard.dailyMacros.carbs')}
                        consumed={nutrition.totalCarbs}
                        target={nutrition.targetCarbs}
                        unit="g"
                        color="#40a9ff"
                      />
                    </Col>
                    <Col span={8}>
                      <MacroProgressBar
                        label={t('athlete:dashboard.dailyMacros.fat')}
                        consumed={nutrition.totalFat}
                        target={nutrition.targetFat}
                        unit="g"
                        color="#ff7875"
                      />
                    </Col>
                  </Row>
                </div>

                <Divider style={{ margin: '20px 0' }} />

                {/* Water and Steps Metrics */}
                <div className="daily-history-view__activity-row">
                  <div className="activity-metric">
                    <span className="material-symbols-outlined icon water">water_drop</span>
                    <div className="details">
                      <span className="value mono">{nutrition.waterLitersConsumed.toFixed(1)}/{nutrition.waterLitersTarget}L</span>
                      <span className="label">{t('athlete:history.hydration')}</span>
                    </div>
                  </div>
                  <div className="activity-metric">
                    <span className="material-symbols-outlined icon steps">directions_run</span>
                    <div className="details">
                      <span className="value mono">{nutrition.stepsWalked.toLocaleString()}/{nutrition.stepsTarget.toLocaleString()}</span>
                      <span className="label">{t('athlete:history.steps')}</span>
                    </div>
                  </div>
                </div>

                <Divider style={{ margin: '20px 0' }} />

                {/* Logged Meals List */}
                <div className="daily-history-view__logged-meals">
                  <h4 className="section-title">{t('athlete:history.loggedMeals')}</h4>
                  {hasMeals ? (
                    <div className="daily-history-view__meals-list">
                      {[
                        MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack,
                        MealType.Suhoor, MealType.Iftar, MealType.PreWorkout, MealType.PostWorkout
                      ].map(type => {
                        const key = type === MealType.Breakfast ? 'breakfast' :
                                    type === MealType.Lunch ? 'lunch' :
                                    type === MealType.Dinner ? 'dinner' :
                                    type === MealType.Snack ? 'snack' :
                                    type === MealType.Suhoor ? 'suhoor' :
                                    type === MealType.Iftar ? 'iftar' :
                                    type === MealType.PreWorkout ? 'preWorkout' : 'postWorkout';
                        const mealEntries = nutrition[key] || [];
                        if (mealEntries.length === 0) return null;

                        return (
                          <div key={type} className="daily-history-view__meal-slot">
                            <h5 className="meal-slot-title">{getMealTypeLabel(type, t)}</h5>
                            <div className="meal-slot-entries">
                              {mealEntries.map((e: any) => (
                                <div key={e.id} className="meal-entry-item">
                                  <div className="info">
                                    <span className="name">{e.food?.name ?? e.recipe?.name ?? 'Food'}</span>
                                    <span className="qty mono">{e.quantityGrams}g</span>
                                  </div>
                                  <div className="macros mono">
                                    <span>P:{e.protein.toFixed(0)}g</span>
                                    <span>C:{e.carbs.toFixed(0)}g</span>
                                    <span>F:{e.fat.toFixed(0)}g</span>
                                    <span className="kcal">{Math.round(e.calories)} kcal</span>
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  ) : (
                    <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t('athlete:history.noNutrition')} />
                  )}
                </div>
              </div>
            ) : (
              <Empty description={t('athlete:history.noNutrition')} style={{ padding: '24px 0' }} />
            )}
          </Card>

          {/* ── SUPPLEMENTS CARD ── */}
          <Card 
            title={
              <div className="daily-history-view__card-header">
                <span className="material-symbols-outlined icon text-gold">medication</span>
                <span>{t('athlete:history.supplementsTab')}</span>
              </div>
            }
            className="daily-history-view__card daily-history-view__card--supplements"
            style={{ marginTop: '24px' }}
          >
            {supplements.length > 0 ? (
              <div className="daily-history-view__supplements">
                <div className="daily-history-view__supplements-grid">
                  {supplements.map(item => {
                    const cardClass = item.isTakenToday 
                      ? 'supplement-log-card--taken' 
                      : (isPastDate ? 'supplement-log-card--missed' : '');
                    const tagColor = item.isTakenToday 
                      ? 'success' 
                      : (isPastDate ? 'error' : 'warning');
                    const tagLabel = item.isTakenToday 
                      ? t('athlete:history.taken') 
                      : (isPastDate ? t('athlete:history.notTaken', 'Not Taken') : t('athlete:history.pending'));

                    return (
                      <div key={item.id} className={`supplement-log-card ${cardClass}`}>
                        <div className="supplement-log-card__header">
                          <span className="supplement-name">{item.name}</span>
                          <Tag color={tagColor}>
                            {tagLabel}
                          </Tag>
                        </div>
                        <div className="supplement-log-card__body">
                          {item.dosage && (
                            <div className="detail-row">
                              <span className="label">{t('athlete:history.dosage')}:</span>
                              <span className="value">{item.dosage}</span>
                            </div>
                          )}
                          {item.notes && (
                            <div className="detail-row">
                              <span className="label">{t('athlete:history.notes')}:</span>
                              <span className="value">{item.notes}</span>
                            </div>
                          )}
                          {item.isTakenToday && item.takenAt && (
                            <div className="detail-row completed-at">
                              <span className="material-symbols-outlined icon">check_circle</span>
                              <span className="value">
                                {t('athlete:history.takenAt', { time: new Date(item.takenAt).toLocaleTimeString(i18n.language, { hour: '2-digit', minute: '2-digit' }) })}
                              </span>
                            </div>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            ) : (
              <Empty description={t('athlete:history.noSupplements')} style={{ padding: '24px 0' }} />
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DailyLogHistoryView;
