import React, { useState, useEffect } from 'react';
import { useDroppable } from '@dnd-kit/core';
import { Card, Switch, Select, Input, Button, InputNumber } from 'antd';
import { useTranslation } from 'react-i18next';
import type { WorkoutTemplateExerciseDto } from '../../types/Workout';
import './WorkoutDayColumn.scss';

interface WorkoutDayColumnProps {
  dayNumber: number;
  dayLabel: string;
  isRestDay: boolean;
  exercises: WorkoutTemplateExerciseDto[];
  onToggleRestDay: (dayNumber: number, isRestDay: boolean) => void;
  onUpdateExercise: (dayNumber: number, exerciseIndex: number, updatedFields: Partial<WorkoutTemplateExerciseDto>) => void;
  onRemoveExercise: (dayNumber: number, exerciseIndex: number) => void;
  onUpdateDayLabel: (dayNumber: number, newLabel: string) => void;
  onRemoveDay: (dayNumber: number) => void;
}

const WorkoutDayColumn: React.FC<WorkoutDayColumnProps> = ({
  dayNumber,
  dayLabel,
  isRestDay,
  exercises,
  onToggleRestDay,
  onUpdateExercise,
  onRemoveExercise,
  onUpdateDayLabel,
  onRemoveDay,
}) => {
  const { t } = useTranslation(['common', 'athlete', 'coach']);
  const { setNodeRef, isOver } = useDroppable({
    id: `day-${dayNumber}`,
    data: {
      dayNumber,
    },
  });

  const [isEditing, setIsEditing] = useState(false);
  const [editedValue, setEditedValue] = useState(dayLabel);

  useEffect(() => {
    setEditedValue(dayLabel);
  }, [dayLabel]);

  const handleSave = () => {
    if (editedValue.trim() && editedValue.trim() !== dayLabel) {
      onUpdateDayLabel(dayNumber, editedValue.trim());
    } else {
      setEditedValue(dayLabel);
    }
    setIsEditing(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSave();
    } else if (e.key === 'Escape') {
      setEditedValue(dayLabel);
      setIsEditing(false);
    }
  };

  const SECTIONS = [
    { label: t('athlete:workoutLogger.sections.warmup'), value: 'WarmUp' },
    { label: t('athlete:workoutLogger.sections.main'), value: 'Main' },
    { label: t('athlete:workoutLogger.sections.cooldown'), value: 'CoolDown' },
  ];

  return (
    <div
      ref={setNodeRef}
      className={`workout-day-column ${isOver ? 'workout-day-column--over' : ''} ${
        isRestDay ? 'workout-day-column--rest-day' : ''
      }`}
    >
      <Card
        title={
          <div className="workout-day-column__header">
            <div className="workout-day-column__title-section">
              <span className="workout-day-column__day-num mono">{t('coach:templateBuilder.dayNum', { defaultValue: 'Day {{num}}', num: dayNumber })}</span>
              {isEditing ? (
                <Input
                  size="small"
                  value={editedValue}
                  onChange={(e) => setEditedValue(e.target.value)}
                  onBlur={handleSave}
                  onKeyDown={handleKeyDown}
                  autoFocus
                  className="workout-day-column__label-input"
                />
              ) : (
                <h3
                  className="workout-day-column__day-label editable"
                  onClick={() => setIsEditing(true)}
                  title={t('coach:templateBuilder.clickToEditLabel', { defaultValue: 'Click to edit day label' })}
                >
                  {dayLabel}
                  <span className="material-symbols-outlined edit-icon" style={{ fontSize: 14, marginLeft: 4 }}>edit</span>
                </h3>
              )}
            </div>
            <div className="workout-day-column__header-actions">
              <div className="workout-day-column__rest-toggle">
                <span className="workout-day-column__toggle-text">{t('coach:templateBuilder.rest', { defaultValue: 'Rest' })}</span>
                <Switch
                  size="small"
                  checked={isRestDay}
                  onChange={(checked) => onToggleRestDay(dayNumber, checked)}
                  checkedChildren="ON"
                  unCheckedChildren="OFF"
                />
              </div>
              <Button
                type="text"
                danger
                size="small"
                onClick={() => onRemoveDay(dayNumber)}
                className="workout-day-column__delete-day-btn"
                icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>close</span>}
                title={t('coach:templateBuilder.delete', { defaultValue: 'Remove this day' })}
              />
            </div>
          </div>
        }
        className="workout-day-column__card"
        variant="borderless"
      >
        {isRestDay ? (
          <div className="workout-day-column__rest-body">
            <span className="material-symbols-outlined">hotel</span>
            <p>{t('coach:templateBuilder.restRecovery', { defaultValue: 'Rest & Recovery Day' })}</p>
          </div>
        ) : (
          <div className="workout-day-column__body">
            {exercises.length === 0 ? (
              <div className="workout-day-column__empty-zone">
                <span className="material-symbols-outlined">add_task</span>
                <p>{t('coach:templateBuilder.dragHelp', { defaultValue: 'Drag exercises here' })}</p>
              </div>
            ) : (
              <div className="workout-day-column__exercise-list">
                {exercises.map((item, idx) => (
                  <div key={idx} className="workout-day-column__exercise-item">
                    <div className="workout-day-column__exercise-item-header">
                      <span className="workout-day-column__exercise-name">{item.exerciseName}</span>
                      <Button
                        type="text"
                        danger
                        size="small"
                        onClick={() => onRemoveExercise(dayNumber, idx)}
                        className="workout-day-column__remove-btn"
                        icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>delete</span>}
                      />
                    </div>

                    <div className="workout-day-column__exercise-fields">
                      <div className="workout-day-column__field-group">
                        <span className="workout-day-column__field-label">{t('coach:templateBuilder.sectionLabel', { defaultValue: 'Section' })}</span>
                        <Select
                          size="small"
                          value={item.section}
                          options={SECTIONS}
                          onChange={(val) => onUpdateExercise(dayNumber, idx, { section: val as any })}
                          className="workout-day-column__select"
                          classNames={{ popup: { root: 'workout-day-column__dropdown' } }}
                        />
                      </div>

                      <div className="workout-day-column__fields-grid">
                        <div className="workout-day-column__field-group">
                          <span className="workout-day-column__field-label">{t('coach:templateBuilder.setsLabel', { defaultValue: 'Sets' })}</span>
                          <InputNumber
                            size="small"
                            min={1}
                            max={20}
                            value={item.targetSets}
                            onChange={(val) => onUpdateExercise(dayNumber, idx, { targetSets: val || 1 })}
                            className="workout-day-column__input-number"
                          />
                        </div>

                        <div className="workout-day-column__field-group">
                          <span className="workout-day-column__field-label">{t('athlete:components.exerciseCard.repsHeader', { defaultValue: 'Reps' })}</span>
                          <Input
                            size="small"
                            value={item.targetReps}
                            onChange={(e) => onUpdateExercise(dayNumber, idx, { targetReps: e.target.value })}
                            placeholder="e.g. 8-12"
                            className="workout-day-column__input"
                          />
                        </div>

                        <div className="workout-day-column__field-group">
                          <span className="workout-day-column__field-label">{t('athlete:components.exerciseCard.restTimeLabel', { defaultValue: 'Rest (s)' })}</span>
                          <InputNumber
                            size="small"
                            min={0}
                            value={item.restSeconds ?? undefined}
                            onChange={(val) => onUpdateExercise(dayNumber, idx, { restSeconds: val })}
                            placeholder="90"
                            className="workout-day-column__input-number"
                          />
                        </div>

                        <div className="workout-day-column__field-group">
                          <span className="workout-day-column__field-label">{t('athlete:components.exerciseCard.targetWeightLabel', { defaultValue: 'Target (kg)' })}</span>
                          <InputNumber
                            size="small"
                            min={0}
                            precision={1}
                            value={item.progressiveOverloadTargetKg ?? undefined}
                            onChange={(val) => onUpdateExercise(dayNumber, idx, { progressiveOverloadTargetKg: val })}
                            placeholder="Overload"
                            className="workout-day-column__input-number"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </Card>
    </div>
  );
};

export default WorkoutDayColumn;
