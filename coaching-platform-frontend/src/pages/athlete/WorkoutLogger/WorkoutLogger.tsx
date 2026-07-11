import React, { useState } from 'react';
import { Skeleton, Empty, Popconfirm, Button, Tag } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useGetTodaysWorkout, useCompleteWorkout } from '../../../hooks/useWorkout/useWorkout';
import ExerciseCard from '../../../components/ExerciseCard/ExerciseCard';
import VideoDemoModal from '../../../components/VideoDemoModal/VideoDemoModal';
import { WorkoutStatus, type TemplateExerciseDto, type SetLogDto } from '../../../types/Workout';
import './WorkoutLogger.scss';


// ── Section Group ─────────────────────────────────────────────────────────────
interface SectionGroupProps {
  title: string;
  icon: string;
  exercises: TemplateExerciseDto[];
  loggedSets: SetLogDto[];
  workoutLogId: number;
  onVideoPlay: (videoId: string, exerciseName: string) => void;
  accentClass: string;
}

const SectionGroup: React.FC<SectionGroupProps> = ({
  title,
  icon,
  exercises,
  loggedSets,
  workoutLogId,
  onVideoPlay,
  accentClass,
}) => {
  const { t } = useTranslation(['athlete']);
  if (exercises.length === 0) return null;

  return (
    <div className="workout-section">
      <div className={`workout-section__header ${accentClass}`}>
        <span className="material-symbols-outlined">{icon}</span>
        <h2 className="workout-section__title">{title}</h2>
        <span className="workout-section__count mono">{t('athlete:workoutLogger.exercisesCount', { count: exercises.length })}</span>
      </div>
      <div className="workout-section__cards">
        {exercises.map((ex) => (
          <ExerciseCard
            key={ex.id}
            exercise={ex}
            loggedSets={loggedSets ?? []}
            workoutLogId={workoutLogId}
            onVideoPlay={onVideoPlay}
          />
        ))}
      </div>
    </div>
  );
};

// ── Status badge helper ───────────────────────────────────────────────────────
const StatusBadge: React.FC<{ status: WorkoutStatus }> = ({ status }) => {
  const { t } = useTranslation(['athlete']);
  const config: Record<WorkoutStatus, { color: string; label: string; icon: string }> = {
    [WorkoutStatus.InProgress]: { color: '#fabd00', label: t('athlete:workoutLogger.status.inProgress'), icon: 'fitness_center' },
    [WorkoutStatus.Completed]: { color: '#12b76a', label: t('athlete:workoutLogger.status.completed'), icon: 'check_circle' },
    [WorkoutStatus.Missed]: { color: '#ba1a1a', label: t('athlete:workoutLogger.status.missed'), icon: 'cancel' },
    [WorkoutStatus.NoProgram]: { color: '#76767e', label: t('athlete:workoutLogger.status.noProgram'), icon: 'calendar_today' },
  };
  const c = config[status];
  return (
    <Tag
      icon={<span className="material-symbols-outlined" style={{ fontSize: 13, marginRight: 4 }}>{c.icon}</span>}
      style={{
        background: `${c.color}20`,
        borderColor: `${c.color}60`,
        color: c.color,
        fontFamily: 'var(--font-data)',
        fontSize: 11,
        fontWeight: 600,
        letterSpacing: '0.05em',
        padding: '2px 10px',
        borderRadius: 999,
      }}
    >
      {c.label}
    </Tag>
  );
};

// ── Main Page ────────────────────────────────────────────────────────────────
const WorkoutLogger: React.FC = () => {
  const navigate = useNavigate();
  const { t, i18n } = useTranslation(['common', 'athlete']);
  const { data: workout, isLoading, isError } = useGetTodaysWorkout();
  const { mutate: completeWorkout, isPending: isCompleting } = useCompleteWorkout();

  const [videoModal, setVideoModal] = useState<{ open: boolean; videoId: string; name: string }>({
    open: false,
    videoId: '',
    name: '',
  });

  const handleVideoPlay = (videoId: string, exerciseName: string) => {
    setVideoModal({ open: true, videoId, name: exerciseName });
  };

  const handleComplete = () => {
    if (!workout?.workoutLogId) return;
    completeWorkout(
      { workoutLogId: workout.workoutLogId },
      {
        onSuccess: () => {
          navigate('/athlete/dashboard');
        },
      },
    );
  };

  // ── Loading state ──────────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div id="workout-logger-page" className="workout-logger animate-fade-in">
        <div className="workout-logger__header">
          <Skeleton.Input style={{ width: 220, height: 32 }} active />
          <Skeleton.Input style={{ width: 100, height: 24 }} active />
        </div>
        <div className="workout-logger__skeleton-cards">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} active paragraph={{ rows: 4 }} />
          ))}
        </div>
      </div>
    );
  }

  // ── Error state ────────────────────────────────────────────────────────────
  if (isError) {
    return (
      <div id="workout-logger-page" className="workout-logger animate-fade-in">
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={t('athlete:workoutLogger.failedLoad')}
          style={{ marginTop: 80 }}
        >
          <Button onClick={() => window.location.reload()} type="primary">
            {t('athlete:workoutLogger.retry')}
          </Button>
        </Empty>
      </div>
    );
  }

  // ── No Program state ───────────────────────────────────────────────────────
  if (!workout || workout.status === WorkoutStatus.NoProgram) {
    return (
      <div id="workout-logger-page" className="workout-logger animate-fade-in">
        <div className="workout-logger__header">
          <div>
            <h1 className="workout-logger__title">{t('athlete:workoutLogger.title')}</h1>
            <p className="workout-logger__sub">{t('athlete:workoutLogger.sub')}</p>
          </div>
        </div>
        <div className="workout-logger__no-program">
          <span className="material-symbols-outlined workout-logger__no-program-icon">
            sports_gymnastics
          </span>
          <h2>{t('athlete:workoutLogger.noProgram')}</h2>
          <p>{t('athlete:workoutLogger.noProgramDesc')}</p>
        </div>
      </div>
    );
  }

  const { day, loggedSets, workoutLogId, status } = workout;
  const isCompleted = status === WorkoutStatus.Completed;

  // ── Rest Day state ─────────────────────────────────────────────────────────
  if (day?.isRestDay) {
    return (
      <div id="workout-logger-page" className="workout-logger animate-fade-in">
        <div className="workout-logger__header">
          <div>
            <h1 className="workout-logger__title">{t('athlete:workoutLogger.title')}</h1>
            <p className="workout-logger__sub">{t('athlete:workoutLogger.sub')}</p>
          </div>
        </div>
        <div className="workout-logger__rest-day">
          <span className="material-symbols-outlined workout-logger__rest-icon">self_improvement</span>
          <h2>{t('athlete:workoutLogger.restDayTitle')}</h2>
          <p>{t('athlete:workoutLogger.restDayDesc')}</p>
        </div>
      </div>
    );
  }

  const totalExercises =
    (day?.warmUp.length ?? 0) + (day?.main.length ?? 0) + (day?.coolDown.length ?? 0);
  const loggedCount = new Set(loggedSets?.map((s) => s.exerciseId) ?? []).size;

  return (
    <div id="workout-logger-page" className="workout-logger animate-fade-in">
      {/* ── Header ── */}
      <div className="workout-logger__header">
        <div>
          <h1 className="workout-logger__title">{day?.dayLabel ?? t('athlete:workoutLogger.title')}</h1>
          <p className="workout-logger__sub">
            {t('athlete:workoutLogger.exercisesCount', { count: totalExercises })} · {t('athlete:workoutLogger.exercisesCountStarted', { started: loggedCount, total: totalExercises })}
          </p>
        </div>
        {status !== WorkoutStatus.Completed && <StatusBadge status={status} />}
      </div>

      {/* ── Session Info Card ── */}
      <div className="workout-logger__info-card">
        <div className="workout-logger__info-row">
          <span className="material-symbols-outlined">calendar_today</span>
          <span className="mono">{new Date().toLocaleDateString(i18n.language, { weekday: 'long', day: 'numeric', month: 'long' })}</span>
        </div>
        {isCompleted && (
          <div className="workout-logger__completed-banner">
            <span className="material-symbols-outlined">check_circle</span>
            {t('athlete:workoutLogger.completedBanner')}
          </div>
        )}
      </div>

      {/* ── Exercise Sections ── */}
      <div className="workout-logger__content">
        <SectionGroup
          title={t('athlete:workoutLogger.sections.warmup')}
          icon="directions_run"
          exercises={day?.warmUp ?? []}
          loggedSets={loggedSets}
          workoutLogId={workoutLogId}
          onVideoPlay={handleVideoPlay}
          accentClass="workout-section__header--warmup"
        />
        <SectionGroup
          title={t('athlete:workoutLogger.sections.main')}
          icon="fitness_center"
          exercises={day?.main ?? []}
          loggedSets={loggedSets}
          workoutLogId={workoutLogId}
          onVideoPlay={handleVideoPlay}
          accentClass="workout-section__header--main"
        />
        <SectionGroup
          title={t('athlete:workoutLogger.sections.cooldown')}
          icon="self_improvement"
          exercises={day?.coolDown ?? []}
          loggedSets={loggedSets}
          workoutLogId={workoutLogId}
          onVideoPlay={handleVideoPlay}
          accentClass="workout-section__header--cooldown"
        />
      </div>

      {/* ── Complete Workout Button ── */}
      {!isCompleted && (
        <div className="workout-logger__complete-zone">
          <Popconfirm
            title={t('athlete:workoutLogger.confirmComplete')}
            description={t('athlete:workoutLogger.confirmCompleteDesc')}
            onConfirm={handleComplete}
            okText={t('athlete:workoutLogger.completeBtn')}
            cancelText={t('athlete:workoutLogger.cancelBtn')}
            okButtonProps={{ style: { background: 'var(--color-navy)', borderColor: 'var(--color-navy)' } }}
          >
            <button
              className="workout-logger__complete-btn"
              disabled={isCompleting}
              id="complete-workout-btn"
            >
              <span className="material-symbols-outlined">emoji_events</span>
              {isCompleting ? t('athlete:workoutLogger.saving') : t('athlete:workoutLogger.completeWorkout')}
            </button>
          </Popconfirm>
        </div>
      )}

      {/* ── Video Demo Modal ── */}
      <VideoDemoModal
        open={videoModal.open}
        videoId={videoModal.videoId}
        exerciseName={videoModal.name}
        onClose={() => setVideoModal((s) => ({ ...s, open: false }))}
      />
    </div>
  );
};

export default WorkoutLogger;
