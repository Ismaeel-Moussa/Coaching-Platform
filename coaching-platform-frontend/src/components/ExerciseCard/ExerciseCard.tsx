import React, { useState } from 'react';
import { Tooltip, Popover } from 'antd';
import { useLogSet } from '../../hooks/useWorkout/useWorkout';
import type { TemplateExerciseDto, SetLogDto } from '../../types/Workout';
import type { LogSetForm } from '../../types/Workout';
import { ExerciseSection } from '../../types/Workout';
import './ExerciseCard.scss';

interface ExerciseCardProps {
  exercise: TemplateExerciseDto;
  loggedSets: SetLogDto[];
  workoutLogId: number;
  onVideoPlay: (videoId: string, exerciseName: string) => void;
}

const SECTION_LABELS: Record<ExerciseSection, { label: string; className: string }> = {
  [ExerciseSection.WarmUp]: { label: 'Warm-Up', className: 'exercise-card__badge--warmup' },
  [ExerciseSection.Main]: { label: 'Main', className: 'exercise-card__badge--main' },
  [ExerciseSection.CoolDown]: { label: 'Cool-Down', className: 'exercise-card__badge--cooldown' },
};

const ExerciseCard: React.FC<ExerciseCardProps> = ({
  exercise,
  loggedSets,
  workoutLogId,
  onVideoPlay,
}) => {
  const { mutate: logSet, isPending } = useLogSet();

  // Local input state: one row per target set — [{ weight, reps }, ...]
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

  const isSetLogged = (setIndex: number) => {
    return loggedSets.some(
      (s) => s.exerciseId === exercise.exercise.id && s.setNumber === setIndex + 1,
    );
  };

  const allSetsLogged = Array.from({ length: exercise.targetSets }, (_, i) =>
    isSetLogged(i),
  ).every(Boolean);

  const handleInputChange = (
    setIndex: number,
    field: 'weight' | 'reps',
    value: string,
  ) => {
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

  const sectionMeta = SECTION_LABELS[exercise.section];

  return (
    <div className={`exercise-card ${allSetsLogged ? 'exercise-card--done' : ''}`}>
      {/* ── Header ── */}
      <div className="exercise-card__header">
        <div className="exercise-card__title-row">
          <span className={`exercise-card__badge ${sectionMeta.className}`}>
            {sectionMeta.label}
          </span>
          <h3 className="exercise-card__name">{exercise.exercise.name}</h3>
          {allSetsLogged && (
            <span className="exercise-card__done-check material-symbols-outlined">
              check_circle
            </span>
          )}
        </div>

        <div className="exercise-card__meta">
          <span className="mono exercise-card__target">
            {exercise.targetSets} × {exercise.targetReps}
          </span>
          {exercise.restSeconds && (
            <span className="exercise-card__rest mono">
              <span className="material-symbols-outlined">timer</span>
              {exercise.restSeconds}s rest
            </span>
          )}
          {exercise.progressiveOverloadTargetKg && (
            <span className="exercise-card__overload mono">
              <span className="material-symbols-outlined">trending_up</span>
              Target: {exercise.progressiveOverloadTargetKg}kg
            </span>
          )}
          {exercise.exercise.equipmentRequired && (
            <span className="exercise-card__equipment">
              <span className="material-symbols-outlined">fitness_center</span>
              {exercise.exercise.equipmentRequired}
            </span>
          )}
        </div>
      </div>

      {/* ── Set Rows ── */}
      <div className="exercise-card__sets">
        <div className="exercise-card__sets-header">
          <span>Set</span>
          <span>Weight (kg)</span>
          <span>Reps</span>
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
                  Log
                </button>
              )}
            </div>
          );
        })}
      </div>

      {/* ── Footer Actions ── */}
      <div className="exercise-card__footer">
        {exercise.exercise.instructions && (
          <Popover
            content={
              <p style={{ maxWidth: 280, fontSize: 13 }}>{exercise.exercise.instructions}</p>
            }
            title="Instructions"
            trigger="click"
          >
            <button className="exercise-card__action-btn" id={`instructions-btn-${exercise.exercise.id}`}>
              <span className="material-symbols-outlined">info</span>
              Instructions
            </button>
          </Popover>
        )}
        {exercise.exercise.youTubeVideoId && (
          <Tooltip title="Watch demo video">
            <button
              className="exercise-card__action-btn exercise-card__action-btn--video"
              onClick={() => onVideoPlay(exercise.exercise.youTubeVideoId!, exercise.exercise.name)}
              id={`video-btn-${exercise.exercise.id}`}
            >
              <span className="material-symbols-outlined">play_circle</span>
              Demo Video
            </button>
          </Tooltip>
        )}
      </div>
    </div>
  );
};

export default ExerciseCard;
