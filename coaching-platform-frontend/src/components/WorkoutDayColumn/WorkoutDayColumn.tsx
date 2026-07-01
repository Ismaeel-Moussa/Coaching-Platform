import React from 'react';
import { useDroppable } from '@dnd-kit/core';
import { Card, Switch, Select, Input, Button, InputNumber } from 'antd';
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
}

const SECTIONS = [
  { label: 'Warm Up', value: 'WarmUp' },
  { label: 'Main Lift', value: 'Main' },
  { label: 'Cool Down', value: 'CoolDown' },
];

const WorkoutDayColumn: React.FC<WorkoutDayColumnProps> = ({
  dayNumber,
  dayLabel,
  isRestDay,
  exercises,
  onToggleRestDay,
  onUpdateExercise,
  onRemoveExercise,
}) => {
  const { setNodeRef, isOver } = useDroppable({
    id: `day-${dayNumber}`,
    data: {
      dayNumber,
    },
  });

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
            <div>
              <span className="workout-day-column__day-num mono">Day {dayNumber}</span>
              <h3 className="workout-day-column__day-label">{dayLabel}</h3>
            </div>
            <div className="workout-day-column__rest-toggle">
              <span className="workout-day-column__toggle-text">Rest</span>
              <Switch
                size="small"
                checked={isRestDay}
                onChange={(checked) => onToggleRestDay(dayNumber, checked)}
                checkedChildren="ON"
                unCheckedChildren="OFF"
              />
            </div>
          </div>
        }
        className="workout-day-column__card"
        bordered={false}
      >
        {isRestDay ? (
          <div className="workout-day-column__rest-body">
            <span className="material-symbols-outlined">hotel</span>
            <p>Rest & Recovery Day</p>
          </div>
        ) : (
          <div className="workout-day-column__body">
            {exercises.length === 0 ? (
              <div className="workout-day-column__empty-zone">
                <span className="material-symbols-outlined">add_task</span>
                <p>Drag exercises here</p>
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
                        <span className="workout-day-column__field-label">Section</span>
                        <Select
                          size="small"
                          value={item.section}
                          options={SECTIONS}
                          onChange={(val) => onUpdateExercise(dayNumber, idx, { section: val as any })}
                          className="workout-day-column__select"
                          popupClassName="workout-day-column__dropdown"
                        />
                      </div>

                      <div className="workout-day-column__fields-grid">
                        <div className="workout-day-column__field-group">
                          <span className="workout-day-column__field-label">Sets</span>
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
                          <span className="workout-day-column__field-label">Reps</span>
                          <Input
                            size="small"
                            value={item.targetReps}
                            onChange={(e) => onUpdateExercise(dayNumber, idx, { targetReps: e.target.value })}
                            placeholder="e.g. 8-12"
                            className="workout-day-column__input"
                          />
                        </div>

                        <div className="workout-day-column__field-group">
                          <span className="workout-day-column__field-label">Rest (s)</span>
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
                          <span className="workout-day-column__field-label">Target (kg)</span>
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
