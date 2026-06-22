import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Skeleton } from 'antd';
import { useGetDashboard } from '../../../hooks/useAthlete/useAthlete';
import { useUpdateWater, useUpdateSteps } from '../../../hooks/useDiary/useDiary';
import MacroProgressBar from '../../../components/MacroProgressBar/MacroProgressBar';
import RingProgress from '../../../components/RingProgress/RingProgress';
import { getTodayIso, formatDateDisplay, getDayOfWeek } from '../../../utils/date';
import './Dashboard.scss';

const WORKOUT_STATUS_CONFIG = {
  NoProgram: { label: 'No Program', icon: 'info', className: 'status--none' },
  InProgress: { label: 'In Progress', icon: 'play_circle', className: 'status--active' },
  Completed: { label: 'Completed', icon: 'check_circle', className: 'status--completed' },
  Missed: { label: 'Missed', icon: 'cancel', className: 'status--missed' },
} as const;

const AthleteDashboard: React.FC = () => {
  const navigate = useNavigate();
  const today = getTodayIso();

  const { data, isLoading, error } = useGetDashboard();
  const updateWaterMutation = useUpdateWater(today);
  const updateStepsMutation = useUpdateSteps(today);

  const handleAddWater = () => {
    if (!data) return;
    const newTotal = parseFloat(
      (data.today.waterLitersConsumed + 0.25).toFixed(2),
    );
    updateWaterMutation.mutate({ waterLiters: newTotal });
  };

  if (error) {
    return (
      <div className="dashboard dashboard--error">
        <span className="material-symbols-outlined">error_outline</span>
        <p>Failed to load dashboard. Please refresh the page.</p>
      </div>
    );
  }

  const todayMacros = data?.today;
  const athlete = data?.athlete;
  const workoutStatus = data?.todaysWorkoutStatus ?? 'NoProgram';
  const statusConfig = WORKOUT_STATUS_CONFIG[workoutStatus];

  return (
    <div id="athlete-dashboard-page" className="dashboard animate-fade-in">
      {/* ── Page Header ── */}
      <div className="dashboard__header">
        <div>
          <h1 className="dashboard__greeting">
            {isLoading ? (
              <Skeleton.Input active style={{ width: 200 }} />
            ) : (
              <>Good morning, <span className="dashboard__name">{athlete?.firstName}</span> 👋</>
            )}
          </h1>
          <p className="dashboard__date">
            {getDayOfWeek(today)}, {formatDateDisplay(today)}
          </p>
        </div>

        {/* Streak badge */}
        {!isLoading && athlete && (
          <div className="dashboard__streak">
            <span className="dashboard__streak-flame">🔥</span>
            <div className="dashboard__streak-info">
              <span className="dashboard__streak-count mono">{athlete.currentStreak}</span>
              <span className="dashboard__streak-label">Day Streak</span>
            </div>
          </div>
        )}
      </div>

      {/* ── Bento Grid ── */}
      <div className="dashboard__grid">

        {/* ── Card 1: Daily Macros ── */}
        <div className="dashboard__card dashboard__card--macros">
          <div className="dashboard__card-header">
            <span className="material-symbols-outlined">nutrition</span>
            <h2 className="dashboard__card-title">Daily Macros</h2>
            {todayMacros && (
              <span className="dashboard__card-sub mono">
                {Math.round(todayMacros.caloriesRemaining)} kcal remaining
              </span>
            )}
          </div>

          {isLoading ? (
            <div className="dashboard__skeleton-bars">
              {[1, 2, 3, 4].map((i) => (
                <Skeleton key={i} active paragraph={{ rows: 1, width: '100%' }} title={false} />
              ))}
            </div>
          ) : todayMacros ? (
            <div className="dashboard__macro-bars">
              <MacroProgressBar
                label="Calories"
                consumed={todayMacros.caloriesConsumed}
                target={todayMacros.targetCalories}
                unit=" kcal"
              />
              <MacroProgressBar
                label="Protein"
                consumed={todayMacros.proteinConsumed}
                target={todayMacros.targetProtein}
                unit="g"
              />
              <MacroProgressBar
                label="Carbs"
                consumed={todayMacros.carbsConsumed}
                target={todayMacros.targetCarbs}
                unit="g"
              />
              <MacroProgressBar
                label="Fat"
                consumed={todayMacros.fatConsumed}
                target={todayMacros.targetFat}
                unit="g"
              />
            </div>
          ) : null}

          <button
            className="dashboard__log-btn"
            onClick={() => navigate('/athlete/meal-logger')}
          >
            <span className="material-symbols-outlined">add_circle</span>
            Log a Meal
          </button>
        </div>

        {/* ── Card 2: Today's Session ── */}
        <div className="dashboard__card dashboard__card--session">
          <div className="dashboard__session-icon">
            <span className="material-symbols-outlined">fitness_center</span>
          </div>
          <div className="dashboard__session-body">
            <h2 className="dashboard__card-title dashboard__card-title--inverse">
              Today's Workout
            </h2>
            {isLoading ? (
              <Skeleton active paragraph={{ rows: 2 }} title={false} />
            ) : (
              <>
                <div className={`dashboard__session-status ${statusConfig.className}`}>
                  <span className="material-symbols-outlined">{statusConfig.icon}</span>
                  {statusConfig.label}
                </div>
                {workoutStatus === 'NoProgram' ? (
                  <p className="dashboard__session-hint">No workout program assigned yet. Contact your coach.</p>
                ) : (
                  <p className="dashboard__session-hint">
                    {workoutStatus === 'Completed'
                      ? 'Great work! You crushed today\'s session.'
                      : workoutStatus === 'Missed'
                      ? 'Missed today. Get back on track tomorrow!'
                      : 'Your session is ready. Let\'s go!'}
                  </p>
                )}
              </>
            )}
          </div>
          <button
            className="dashboard__start-btn"
            onClick={() => navigate('/athlete/workouts')}
            disabled={workoutStatus === 'Completed' || workoutStatus === 'NoProgram'}
          >
            <span className="material-symbols-outlined">
              {workoutStatus === 'Completed' ? 'task_alt' : 'play_arrow'}
            </span>
            {workoutStatus === 'Completed' ? 'Completed' : 'Start Workout'}
          </button>
        </div>

        {/* ── Card 3: Daily Targets (Water + Steps) ── */}
        <div className="dashboard__card dashboard__card--targets">
          <div className="dashboard__card-header">
            <span className="material-symbols-outlined">track_changes</span>
            <h2 className="dashboard__card-title">Daily Targets</h2>
          </div>
          {isLoading ? (
            <Skeleton active avatar={{ shape: 'circle', size: 100 }} paragraph={false} />
          ) : todayMacros ? (
            <div className="dashboard__rings">
              <RingProgress
                value={parseFloat(todayMacros.waterLitersConsumed.toFixed(2))}
                max={todayMacros.waterLitersTarget}
                unit="L"
                label="Hydration"
                size={120}
                onIncrement={handleAddWater}
                incrementLabel="+0.25L"
              />
              <RingProgress
                value={todayMacros.stepsWalked}
                max={todayMacros.stepsTarget}
                unit="steps"
                label="Steps"
                size={120}
              />
            </div>
          ) : null}
        </div>

        {/* ── Card 4: Stats / Goal ── */}
        {!isLoading && athlete && (
          <div className="dashboard__card dashboard__card--stats">
            <div className="dashboard__card-header">
              <span className="material-symbols-outlined">emoji_events</span>
              <h2 className="dashboard__card-title">Your Goal</h2>
            </div>
            <div className="dashboard__goal">
              <span className="dashboard__goal-label">{athlete.targetGoal || 'General Fitness'}</span>
            </div>
            <div className="dashboard__streak-stats">
              <div className="dashboard__streak-stat">
                <span className="dashboard__streak-stat-val mono">{athlete.currentStreak}</span>
                <span className="dashboard__streak-stat-label">Current Streak</span>
              </div>
              <div className="dashboard__streak-divider" />
              <div className="dashboard__streak-stat">
                <span className="dashboard__streak-stat-val mono">{athlete.longestStreak}</span>
                <span className="dashboard__streak-stat-label">Longest Streak</span>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AthleteDashboard;
