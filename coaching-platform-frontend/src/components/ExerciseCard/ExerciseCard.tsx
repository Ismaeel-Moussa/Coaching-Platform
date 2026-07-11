import React, { useState } from 'react';
import { Tooltip, Popover } from 'antd';
import { useTranslation } from 'react-i18next';
import { useLogSet } from '../../hooks/useWorkout/useWorkout';
import type { TemplateExerciseDto, SetLogDto, LogSetForm } from '../../types/Workout';
import { ExerciseSection } from '../../types/Workout';
import './ExerciseCard.scss';

interface ExerciseCardProps {
  exercise: TemplateExerciseDto;
  loggedSets: SetLogDto[];
  workoutLogId: number;
  onVideoPlay: (videoId: string, exerciseName: string) => void;
}

const SECTION_CLASSES: Record<ExerciseSection, string> = {
  [ExerciseSection.WarmUp]: 'exercise-card__badge--warmup',
  [ExerciseSection.Main]: 'exercise-card__badge--main',
  [ExerciseSection.CoolDown]: 'exercise-card__badge--cooldown',
};

// Sections that don't use weight/reps — show a simple "Mark as Done" toggle
const SIMPLE_SECTIONS: ExerciseSection[] = [ExerciseSection.WarmUp, ExerciseSection.CoolDown];

const getSectionLabel = (sec: ExerciseSection, t: any) => {
  switch (sec) {
    case ExerciseSection.WarmUp: return t('athlete:workoutLogger.sections.warmup');
    case ExerciseSection.Main: return t('athlete:workoutLogger.sections.main');
    case ExerciseSection.CoolDown: return t('athlete:workoutLogger.sections.cooldown');
    default: return String(sec);
  }
};

const ExerciseCard: React.FC<ExerciseCardProps> = ({
  exercise,
  loggedSets,
  workoutLogId,
  onVideoPlay,
}) => {
  const { t } = useTranslation(['common', 'athlete']);
  const { mutate: logSet, isPending } = useLogSet();

  const [setInputs, setSetInputs] = useState<Array<{ weight: string; reps: string }>>(() =>
    Array.from({ length: exercise.targetSets }, (_, i) => {
      const existing = loggedSets.find(
        (s) => s.exerciseId === exercise.exercise.id && s.setNumber === i + 1,
      );
      return {
        weight: existing ? String(existing.weightKg) : '',
        reps: existing ? String(existing.reps) : '',
      };
    }),
  );

  const isSetLogged = (setIndex: number) =>
    loggedSets.some(
      (s) => s.exerciseId === exercise.exercise.id && s.setNumber === setIndex + 1,
    );

  const isSimple = SIMPLE_SECTIONS.includes(exercise.section);

  // Simple sections: done = any log entry exists for this exercise
  const allSetsLogged = isSimple
    ? loggedSets.some((s) => s.exerciseId === exercise.exercise.id)
    : Array.from({ length: exercise.targetSets }, (_, i) => isSetLogged(i)).every(Boolean);

  const handleInputChange = (setIndex: number, field: 'weight' | 'reps', value: string) => {
    setSetInputs((prev) => {
      const next = [...prev];
      next[setIndex] = { ...next[setIndex], [field]: value };
      return next;
    });
  };

  const handleLogSet = (setIndex: number) => {
    const { weight, reps } = setInputs[setIndex];
    const weightNum = parseFloat(weight);
    const repsNum = parseInt(reps, 10);
    if (isNaN(weightNum) || isNaN(repsNum) || repsNum < 1) return;
    const form: LogSetForm = {
      workoutLogId,
      exerciseId: exercise.exercise.id,
      setNumber: setIndex + 1,
      weightKg: weightNum,
      reps: repsNum,
    };
    logSet(form);
  };

  // WarmUp / CoolDown: log weightKg=0, reps=1 as a completion signal
  const handleToggleDone = () => {
    if (allSetsLogged) return;
    logSet({
      workoutLogId,
      exerciseId: exercise.exercise.id,
      setNumber: 1,
      weightKg: 0,
      reps: 1,
    });
  };

  return (
    <div className={`exercise-card ${allSetsLogged ? 'exercise-card--done' : ''}`}>
      {/* ── Header ── */}
      <div className="exercise-card__header">
        <div className="exercise-card__title-row">
          <span className={`exercise-card__badge ${SECTION_CLASSES[exercise.section]}`}>
            {getSectionLabel(exercise.section, t)}
          </span>
          <h3 className="exercise-card__name">{exercise.exercise.name}</h3>
          {/* Done button inline for WarmUp/CoolDown */}
          {isSimple && (
            <button
              className={`exercise-card__done-btn ${allSetsLogged ? 'exercise-card__done-btn--checked' : ''}`}
              onClick={handleToggleDone}
              disabled={isPending || allSetsLogged}
              id={`done-btn-exercise-${exercise.exercise.id}`}
            >
              <span className="material-symbols-outlined">
                {allSetsLogged ? 'check_circle' : 'radio_button_unchecked'}
              </span>
              {allSetsLogged ? t('common:status.done') : t('athlete:components.exerciseCard.markDone')}
            </button>
          )}
          {!isSimple && allSetsLogged && (
            <span className="exercise-card__done-check material-symbols-outlined">
              check_circle
            </span>
          )}
        </div>

        <div className="exercise-card__meta">
          <span className="mono exercise-card__target">
            {exercise.targetSets} × {exercise.targetReps}
          </span>
          {exercise.restSeconds != null && (
            <span className="exercise-card__rest mono">
              <span className="material-symbols-outlined">timer</span>
              {t('athlete:components.exerciseCard.restTime', { seconds: exercise.restSeconds })}
            </span>
          )}
          {!isSimple && exercise.progressiveOverloadTargetKg != null && (
            <span className="exercise-card__overload mono">
              <span className="material-symbols-outlined">trending_up</span>
              {t('athlete:components.exerciseCard.targetWeight', { defaultValue: 'Target: {{weight}}kg', weight: exercise.progressiveOverloadTargetKg })}
            </span>
          )}
          {exercise.exercise.equipmentRequired &&
            exercise.exercise.equipmentRequired !== 'None' && (
              <span className="exercise-card__equipment">
                <span className="material-symbols-outlined">fitness_center</span>
                {exercise.exercise.equipmentRequired}
              </span>
            )}
        </div>
      </div>

      {/* ── Body: full set table for Main only ── */}
      {!isSimple && (
        /* Main — full set logging table */
        <div className="exercise-card__sets">
          <div className="exercise-card__sets-header">
            <span>{t('athlete:components.exerciseCard.setHeader', { defaultValue: 'Set' })}</span>
            <span>{t('athlete:components.exerciseCard.weightHeader', { defaultValue: 'Weight (kg)' })}</span>
            <span>{t('athlete:components.exerciseCard.repsHeader', { defaultValue: 'Reps' })}</span>
            <span></span>
          </div>

          {Array.from({ length: exercise.targetSets }, (_, i) => {
            const logged = isSetLogged(i);
            return (
              <div
                key={i}
                className={`exercise-card__set-row ${logged ? 'exercise-card__set-row--logged' : ''}`}
              >
                <span className="mono exercise-card__set-num">{i + 1}</span>
                <input
                  type="number"
                  min={0}
                  step={0.5}
                  placeholder="0"
                  value={setInputs[i]?.weight ?? ''}
                  onChange={(e) => handleInputChange(i, 'weight', e.target.value)}
                  disabled={logged}
                  className="exercise-card__input"
                  id={`weight-exercise-${exercise.exercise.id}-set-${i + 1}`}
                />
                <input
                  type="number"
                  min={1}
                  step={1}
                  placeholder="0"
                  value={setInputs[i]?.reps ?? ''}
                  onChange={(e) => handleInputChange(i, 'reps', e.target.value)}
                  disabled={logged}
                  className="exercise-card__input"
                  id={`reps-exercise-${exercise.exercise.id}-set-${i + 1}`}
                />
                {logged ? (
                  <span className="exercise-card__logged-icon material-symbols-outlined">
                    check_circle
                  </span>
                ) : (
                  <button
                    className="exercise-card__log-btn"
                    onClick={() => handleLogSet(i)}
                    disabled={isPending || !setInputs[i]?.weight || !setInputs[i]?.reps}
                    id={`log-set-btn-exercise-${exercise.exercise.id}-set-${i + 1}`}
                  >
                    {t('athlete:components.exerciseCard.logSet', { defaultValue: 'Log' })}
                  </button>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* ── Footer Actions ── */}
      <div className="exercise-card__footer">
        {exercise.exercise.instructions && (
          <Popover
            content={
              <p style={{ maxWidth: 280, fontSize: 13 }}>{exercise.exercise.instructions}</p>
            }
            title={t('athlete:components.exerciseCard.notes', { defaultValue: 'Instructions' })}
            trigger="click"
          >
            <button
              className="exercise-card__action-btn"
              id={`instructions-btn-${exercise.exercise.id}`}
            >
              <span className="material-symbols-outlined">info</span>
              {t('athlete:components.exerciseCard.notes', { defaultValue: 'Instructions' })}
            </button>
          </Popover>
        )}
        {exercise.exercise.youTubeVideoId && (
          <Tooltip title={t('athlete:components.exerciseCard.videoDemo', { defaultValue: 'Watch demo video' })}>
            <button
              className="exercise-card__action-btn exercise-card__action-btn--video"
              onClick={() =>
                onVideoPlay(exercise.exercise.youTubeVideoId!, exercise.exercise.name)
              }
              id={`video-btn-${exercise.exercise.id}`}
            >
              <span className="material-symbols-outlined">play_circle</span>
              {t('athlete:components.exerciseCard.videoDemo', { defaultValue: 'Demo Video' })}
            </button>
          </Tooltip>
        )}
      </div>
    </div>
  );
};

export default ExerciseCard;
