import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Skeleton } from 'antd';
import { useTranslation, Trans } from 'react-i18next';
import { useGetDashboard } from '../../../hooks/useAthlete/useAthlete';
import { useUpdateWater, useUpdateSteps } from '../../../hooks/useDiary/useDiary';
import MacroProgressBar from '../../../components/MacroProgressBar/MacroProgressBar';
import RingProgress from '../../../components/RingProgress/RingProgress';
import { getTodayIso, formatDateDisplay, getDayOfWeek } from '../../../utils/date';
import './Dashboard.scss';

const WORKOUT_STATUS_CONFIG = {
  NoProgram: { labelKey: 'noProgram', icon: 'info', className: 'status--none' },
  InProgress: { labelKey: 'inProgress', icon: 'play_circle', className: 'status--active' },
  Completed: { labelKey: 'completed', icon: 'check_circle', className: 'status--completed' },
  Missed: { labelKey: 'missed', icon: 'cancel', className: 'status--missed' },
} as const;

const AthleteDashboard: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const today = getTodayIso();

  const { data, isLoading, error } = useGetDashboard();
  const updateWaterMutation = useUpdateWater(today);
  const updateStepsMutation = useUpdateSteps(today);

  React.useEffect(() => {
    if (window.location.hash === '#coach-feedback' && data) {
      const timer = setTimeout(() => {
        const element = document.getElementById('coach-feedback-card');
        if (element) {
          element.scrollIntoView({ behavior: 'smooth' });
          element.classList.add('pulse-highlight');
          setTimeout(() => {
            element.classList.remove('pulse-highlight');
          }, 2000);
        }
      }, 300);
      return () => clearTimeout(timer);
    }
  }, [data, window.location.hash]);

  const handleAddWater = () => {
    if (!data) return;
    const newTotal = parseFloat(
      (data.today.waterLitersConsumed + 0.25).toFixed(2),
    );
    updateWaterMutation.mutate({ waterLiters: newTotal });
  };

  const handleAddSteps = () => {
    if (!data) return;
    const newTotal = data.today.stepsWalked + 1000;
    updateStepsMutation.mutate({ steps: newTotal });
  };

  if (error) {
    return (
      <div className="dashboard dashboard--error">
        <span className="material-symbols-outlined">error_outline</span>
        <p>{t('athlete:dashboard.errorMsg')}</p>
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
              <Trans
                i18nKey="athlete:dashboard.greeting"
                values={{ name: athlete?.firstName }}
                components={{ span: <span className="dashboard__name" /> }}
              />
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
              <span className="dashboard__streak-label">{t('athlete:dashboard.streakLabel')}</span>
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
            <h2 className="dashboard__card-title">{t('athlete:dashboard.dailyMacros.title')}</h2>
            {todayMacros && (
              <span className="dashboard__card-sub mono">
                {t('athlete:dashboard.dailyMacros.kcalRemaining', { count: Math.round(todayMacros.caloriesRemaining) })}
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
                label={t('athlete:dashboard.dailyMacros.calories')}
                consumed={todayMacros.caloriesConsumed}
                target={todayMacros.targetCalories}
                unit=" kcal"
              />
              <MacroProgressBar
                label={t('athlete:dashboard.dailyMacros.protein')}
                consumed={todayMacros.proteinConsumed}
                target={todayMacros.targetProtein}
                unit="g"
              />
              <MacroProgressBar
                label={t('athlete:dashboard.dailyMacros.carbs')}
                consumed={todayMacros.carbsConsumed}
                target={todayMacros.targetCarbs}
                unit="g"
              />
              <MacroProgressBar
                label={t('athlete:dashboard.dailyMacros.fat')}
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
            {t('athlete:dashboard.dailyMacros.logMealBtn')}
          </button>
        </div>

        {/* ── Card 2: Today's Session ── */}
        <div className="dashboard__card dashboard__card--session">
          <div className="dashboard__session-icon">
            <span className="material-symbols-outlined">fitness_center</span>
          </div>
          <div className="dashboard__session-body">
            <h2 className="dashboard__card-title dashboard__card-title--inverse">
              {t('athlete:dashboard.workout.title')}
            </h2>
            {isLoading ? (
              <Skeleton active paragraph={{ rows: 2 }} title={false} />
            ) : (
              <>
                <div className={`dashboard__session-status ${statusConfig.className}`}>
                  <span className="material-symbols-outlined">{statusConfig.icon}</span>
                  {t('athlete:dashboard.workout.' + statusConfig.labelKey)}
                </div>
                {workoutStatus === 'NoProgram' ? (
                  <p className="dashboard__session-hint">{t('athlete:dashboard.workout.noProgramDesc')}</p>
                ) : (
                  <p className="dashboard__session-hint">
                    {workoutStatus === 'Completed'
                      ? t('athlete:dashboard.workout.completedDesc')
                      : workoutStatus === 'Missed'
                      ? t('athlete:dashboard.workout.missedDesc')
                      : t('athlete:dashboard.workout.readyDesc')}
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
            {workoutStatus === 'Completed'
              ? t('athlete:dashboard.workout.completedBtn')
              : t('athlete:dashboard.workout.startBtn')}
          </button>
        </div>

        {/* ── Card 3: Daily Targets (Water + Steps) ── */}
        <div className="dashboard__card dashboard__card--targets">
          <div className="dashboard__card-header">
            <span className="material-symbols-outlined">track_changes</span>
            <h2 className="dashboard__card-title">{t('athlete:dashboard.targets.title')}</h2>
          </div>
          {isLoading ? (
            <Skeleton active avatar={{ shape: 'circle', size: 100 }} paragraph={false} />
          ) : todayMacros ? (
            <div className="dashboard__rings">
              <RingProgress
                value={parseFloat(todayMacros.waterLitersConsumed.toFixed(2))}
                max={todayMacros.waterLitersTarget}
                unit="L"
                label={t('athlete:dashboard.targets.hydration')}
                size={120}
                onIncrement={handleAddWater}
                incrementLabel="+0.25L"
              />
              <RingProgress
                value={todayMacros.stepsWalked}
                max={todayMacros.stepsTarget}
                unit={t('athlete:dashboard.targets.steps').toLowerCase()}
                label={t('athlete:dashboard.targets.steps')}
                size={120}
                onIncrement={handleAddSteps}
                incrementLabel="+1000"
              />
            </div>
          ) : null}
        </div>

        {/* ── Card 4: Stats / Goal ── */}
        {!isLoading && athlete && (
          <div className="dashboard__card dashboard__card--stats">
            <div className="dashboard__card-header">
              <span className="material-symbols-outlined">emoji_events</span>
              <h2 className="dashboard__card-title">{t('athlete:dashboard.goal')}</h2>
            </div>
            <div className="dashboard__goal">
              <span className="dashboard__goal-label">{athlete.targetGoal || t('athlete:dashboard.generalFitness')}</span>
            </div>
            <div className="dashboard__streak-stats">
              <div className="dashboard__streak-stat">
                <span className="dashboard__streak-stat-val mono">{athlete.currentStreak}</span>
                <span className="dashboard__streak-stat-label">{t('profile:insights.currentStreak')}</span>
              </div>
              <div className="dashboard__streak-divider" />
              <div className="dashboard__streak-stat">
                <span className="dashboard__streak-stat-val mono">{athlete.longestStreak}</span>
                <span className="dashboard__streak-stat-label">{t('profile:insights.longestStreak')}</span>
              </div>
            </div>
          </div>
        )}

        {/* ── Card 5: Coach Feedback ── */}
        <div id="coach-feedback-card" className="dashboard__card dashboard__card--feedback">
          <div className="dashboard__card-header">
            <span className="material-symbols-outlined">chat</span>
            <h2 className="dashboard__card-title">{t('athlete:dashboard.feedback.title')}</h2>
          </div>
          {isLoading ? (
            <Skeleton active paragraph={{ rows: 3 }} title={false} />
          ) : data?.recentFeedbackNotes && data.recentFeedbackNotes.length > 0 ? (
            <div className="dashboard__feedback-list">
              {data.recentFeedbackNotes.map((note: any) => (
                <div key={note.id} className="dashboard__feedback-item">
                  <div className="dashboard__feedback-date">
                    {new Date(note.createdAt).toLocaleDateString(undefined, {
                      month: 'short',
                      day: 'numeric',
                      year: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </div>
                  <p className="dashboard__feedback-text">{note.noteText}</p>
                </div>
              ))}
            </div>
          ) : (
            <div className="dashboard__feedback-empty">
              <span className="material-symbols-outlined">forum</span>
              <p>{t('athlete:dashboard.feedback.empty')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AthleteDashboard;
