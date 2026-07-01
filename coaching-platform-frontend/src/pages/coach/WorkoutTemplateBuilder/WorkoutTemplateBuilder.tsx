import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { DndContext } from '@dnd-kit/core';
import { Button, Input, Tabs, List, Modal, Checkbox, Space, Skeleton } from 'antd';
import { useGetExercises } from '../../../hooks/useExercises/useExercises';
import {
  useGetWorkoutTemplateById,
  useSaveTemplate,
  useUpdateTemplate,
  useAssignTemplate,
} from '../../../hooks/useWorkoutTemplates/useWorkoutTemplates';
import { useGetRoster } from '../../../hooks/useCoachHub/useCoachHub';
import DraggableExercise from '../../../components/DraggableExercise/DraggableExercise';
import WorkoutDayColumn from '../../../components/WorkoutDayColumn/WorkoutDayColumn';
import type { WorkoutTemplateExerciseDto, WorkoutTemplateDayDto } from '../../../types/Workout';
import type { MuscleGroup } from '../../../types/Exercise';
import './WorkoutTemplateBuilder.scss';

const EXERCISE_CATEGORIES: { label: string; value: string }[] = [
  { label: 'All', value: 'All' },
  { label: 'Chest', value: 'Chest' },
  { label: 'Back', value: 'Back' },
  { label: 'Shoulders', value: 'Shoulders' },
  { label: 'Arms', value: 'Arms' },
  { label: 'Legs', value: 'Legs' },
  { label: 'Cardio', value: 'Cardio' },
  { label: 'Core', value: 'Core' },
];

const INITIAL_DAYS: WorkoutTemplateDayDto[] = [
  { dayNumber: 1, dayLabel: 'Push Day 1', isRestDay: false, exercises: [] },
  { dayNumber: 2, dayLabel: 'Pull Day 1', isRestDay: false, exercises: [] },
  { dayNumber: 3, dayLabel: 'Legs Day 1', isRestDay: false, exercises: [] },
  { dayNumber: 4, dayLabel: 'Push Day 2', isRestDay: false, exercises: [] },
  { dayNumber: 5, dayLabel: 'Pull Day 2', isRestDay: false, exercises: [] },
  { dayNumber: 6, dayLabel: 'Legs Day 2', isRestDay: false, exercises: [] },
  { dayNumber: 7, dayLabel: 'Rest Day', isRestDay: true, exercises: [] },
];

const WorkoutTemplateBuilder: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const templateId = id ? parseInt(id, 10) : null;

  // Local State
  const [name, setName] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [days, setDays] = useState<WorkoutTemplateDayDto[]>(INITIAL_DAYS);
  const [selectedMuscle, setSelectedMuscle] = useState<string>('All');
  const [exerciseSearch, setExerciseSearch] = useState<string>('');
  const [isAssignModalVisible, setIsAssignModalVisible] = useState<boolean>(false);
  const [selectedAthleteIds, setSelectedAthleteIds] = useState<number[]>([]);

  // TanStack Queries
  const { data: exercisesData, isLoading: isExercisesLoading } = useGetExercises({
    muscleGroup: selectedMuscle !== 'All' ? (selectedMuscle as MuscleGroup) : undefined,
    search: exerciseSearch || undefined,
    page: 1,
    pageSize: 100,
  });

  const { data: rosterData } = useGetRoster(1, 100);

  const { data: existingTemplate, isLoading: isTemplateLoading } = useGetWorkoutTemplateById(
    templateId!,
    !!templateId,
  );

  // Mutations
  const saveTemplateMutation = useSaveTemplate();
  const updateTemplateMutation = useUpdateTemplate();
  const assignTemplateMutation = useAssignTemplate();

  // Load existing template data in edit mode
  useEffect(() => {
    if (existingTemplate) {
      setName(existingTemplate.name);
      setDescription(existingTemplate.description || '');
      
      // Ensure all 7 days are represented, fallback to default if missing
      const loadedDays = INITIAL_DAYS.map((defDay) => {
        const matchingDay = existingTemplate.days.find((d) => d.dayNumber === defDay.dayNumber);
        return matchingDay
          ? {
              ...matchingDay,
              exercises: matchingDay.exercises || [],
            }
          : defDay;
      });
      setDays(loadedDays);
    }
  }, [existingTemplate]);

  // DnD Drag End Handler
  const handleDragEnd = (event: any) => {
    const { active, over } = event;
    if (!over) return;

    const targetDayId = over.id as string;
    const dayNumber = parseInt(targetDayId.replace('day-', ''), 10);
    const exercise = active.data.current?.exercise;

    if (exercise && dayNumber) {
      setDays((prevDays) =>
        prevDays.map((day) => {
          if (day.dayNumber === dayNumber) {
            // Ignore if day is set to rest
            if (day.isRestDay) return day;

            const newExercise: WorkoutTemplateExerciseDto = {
              exerciseId: exercise.id,
              exerciseName: exercise.name,
              section: 'Main',
              orderIndex: day.exercises.length,
              targetSets: 3,
              targetReps: '10',
              restSeconds: 90,
              progressiveOverloadTargetKg: null,
            };
            return {
              ...day,
              exercises: [...day.exercises, newExercise],
            };
          }
          return day;
        }),
      );
    }
  };

  // Inline exercise property updater
  const handleUpdateExercise = (
    dayNumber: number,
    exerciseIndex: number,
    updatedFields: Partial<WorkoutTemplateExerciseDto>,
  ) => {
    setDays((prevDays) =>
      prevDays.map((day) => {
        if (day.dayNumber === dayNumber) {
          const updatedExercises = day.exercises.map((ex, i) =>
            i === exerciseIndex ? { ...ex, ...updatedFields } : ex,
          );
          return { ...day, exercises: updatedExercises };
        }
        return day;
      }),
    );
  };

  // Remove exercise from day
  const handleRemoveExercise = (dayNumber: number, exerciseIndex: number) => {
    setDays((prevDays) =>
      prevDays.map((day) => {
        if (day.dayNumber === dayNumber) {
          const filteredExercises = day.exercises
            .filter((_, i) => i !== exerciseIndex)
            .map((ex, index) => ({ ...ex, orderIndex: index })); // fix order indexes
          return { ...day, exercises: filteredExercises };
        }
        return day;
      }),
    );
  };

  // Toggle Rest Day status
  const handleToggleRestDay = (dayNumber: number, isRestDay: boolean) => {
    setDays((prevDays) =>
      prevDays.map((day) => {
        if (day.dayNumber === dayNumber) {
          return {
            ...day,
            isRestDay,
            exercises: isRestDay ? [] : day.exercises,
          };
        }
        return day;
      }),
    );
  };

  // Save template to DB
  const handleSave = async () => {
    if (!name.trim()) {
      Modal.error({ title: 'Validation Error', content: 'Please enter a name for the template.' });
      return;
    }

    const payload = {
      name,
      description: description || undefined,
      days: days.map((day) => ({
        dayNumber: day.dayNumber,
        dayLabel: day.dayLabel,
        isRestDay: day.isRestDay,
        exercises: day.exercises.map((ex, idx) => ({
          exerciseId: ex.exerciseId,
          section: ex.section,
          orderIndex: idx,
          targetSets: ex.targetSets,
          targetReps: ex.targetReps,
          restSeconds: ex.restSeconds ?? null,
          progressiveOverloadTargetKg: ex.progressiveOverloadTargetKg ?? null,
        })),
      })),
    };

    try {
      if (templateId) {
        await updateTemplateMutation.mutateAsync({ id: templateId, form: payload });
      } else {
        await saveTemplateMutation.mutateAsync(payload);
      }
      navigate('/coach/dashboard'); // redirect to coach home on success
    } catch (err) {
      // API error handled by hook
    }
  };

  // Assign template flow
  const handleAssign = async () => {
    if (selectedAthleteIds.length === 0) {
      Modal.error({ title: 'Validation Error', content: 'Please select at least one athlete.' });
      return;
    }

    try {
      await assignTemplateMutation.mutateAsync({
        id: templateId!,
        form: { athleteIds: selectedAthleteIds },
      });
      setIsAssignModalVisible(false);
      setSelectedAthleteIds([]);
    } catch (err) {
      // Handled by hook
    }
  };

  if (templateId && isTemplateLoading) {
    return (
      <div className="workout-template-builder workout-template-builder--loading">
        <Skeleton active paragraph={{ rows: 10 }} />
      </div>
    );
  }

  return (
    <DndContext onDragEnd={handleDragEnd}>
      <div id="template-builder-page" className="workout-template-builder animate-fade-in">
        
        {/* Header Block */}
        <div className="workout-template-builder__header">
          <div>
            <h1 className="workout-template-builder__title">
              {templateId ? 'Edit Workout Program Template' : 'Workout Program Template Builder'}
            </h1>
            <p className="workout-template-builder__subtitle">
              Drag-and-drop movements to curate professional 6-day hypertrophy plans
            </p>
          </div>
          <Space>
            {templateId && (
              <Button
                type="default"
                onClick={() => setIsAssignModalVisible(true)}
                icon={<span className="material-symbols-outlined">person_add</span>}
                className="workout-template-builder__action-btn"
              >
                Assign to Athletes
              </Button>
            )}
            <Button
              type="primary"
              onClick={handleSave}
              loading={saveTemplateMutation.isPending || updateTemplateMutation.isPending}
              icon={<span className="material-symbols-outlined">save</span>}
              className="workout-template-builder__action-btn workout-template-builder__action-btn--navy"
            >
              Save Template
            </Button>
          </Space>
        </div>

        {/* Info Form */}
        <div className="workout-template-builder__meta-card">
          <div className="workout-template-builder__meta-fields">
            <div className="workout-template-builder__meta-field">
              <span className="workout-template-builder__meta-label">Template Name</span>
              <Input
                placeholder="e.g. 6-Day PPL Phase 1 (Hypertrophy Focus)"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </div>
            <div className="workout-template-builder__meta-field">
              <span className="workout-template-builder__meta-label">Description / Coach Notes</span>
              <Input.TextArea
                rows={1}
                placeholder="Brief summary or details about progressive overload instructions..."
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                autoSize={{ minRows: 1, maxRows: 3 }}
              />
            </div>
          </div>
        </div>

        {/* Builder Workspace */}
        <div className="workout-template-builder__workspace">
          
          {/* Left panel: Exercise Library */}
          <div className="workout-template-builder__sidebar">
            <div className="workout-template-builder__sidebar-header">
              <span className="material-symbols-outlined">fitness_center</span>
              <h3>Exercise Catalog</h3>
            </div>
            
            <div className="workout-template-builder__sidebar-search">
              <Input
                size="small"
                placeholder="Search exercises..."
                value={exerciseSearch}
                onChange={(e) => setExerciseSearch(e.target.value)}
                allowClear
              />
              <Tabs
                size="small"
                activeKey={selectedMuscle}
                onChange={(key) => setSelectedMuscle(key)}
                items={EXERCISE_CATEGORIES.map((cat) => ({
                  key: cat.value,
                  label: cat.label,
                }))}
                className="workout-template-builder__sidebar-tabs"
              />
            </div>

            <div className="workout-template-builder__sidebar-list">
              {isExercisesLoading ? (
                <div style={{ padding: 16 }}><Skeleton active /></div>
              ) : exercisesData?.items.length === 0 ? (
                <div className="workout-template-builder__sidebar-empty">No matching exercises.</div>
              ) : (
                exercisesData?.items.map((ex) => (
                  <DraggableExercise
                    key={ex.id}
                    id={`exercise-library-${ex.id}`}
                    exercise={{ id: ex.id, name: ex.name, primaryMuscle: ex.primaryMuscle }}
                  />
                ))
              )}
            </div>
          </div>

          {/* Right canvas: Day Columns */}
          <div className="workout-template-builder__canvas">
            <div className="workout-template-builder__canvas-grid">
              {days.map((day) => (
                <WorkoutDayColumn
                  key={day.dayNumber}
                  dayNumber={day.dayNumber}
                  dayLabel={day.dayLabel}
                  isRestDay={day.isRestDay}
                  exercises={day.exercises}
                  onToggleRestDay={handleToggleRestDay}
                  onUpdateExercise={handleUpdateExercise}
                  onRemoveExercise={handleRemoveExercise}
                />
              ))}
            </div>
          </div>

        </div>

        {/* Assign to Athletes Modal */}
        <Modal
          title="Assign Workout Template"
          open={isAssignModalVisible}
          onCancel={() => setIsAssignModalVisible(false)}
          onOk={handleAssign}
          okText="Assign"
          okButtonProps={{ loading: assignTemplateMutation.isPending }}
          width={500}
        >
          <div className="assign-athletes-modal">
            <p className="assign-athletes-modal__hint">
              Select one or more athletes to assign this workout routine. This deactivates their current routine.
            </p>
            <div className="assign-athletes-modal__list-container">
              {rosterData && rosterData.items && rosterData.items.length > 0 ? (
                <Checkbox.Group
                  value={selectedAthleteIds}
                  onChange={(checked) => setSelectedAthleteIds(checked as number[])}
                  className="assign-athletes-modal__checkbox-group"
                >
                  <List
                    size="small"
                    dataSource={rosterData.items}
                    renderItem={(item: any) => (
                      <List.Item key={item.athleteId} className="assign-athletes-modal__item">
                        <Checkbox value={item.athleteId}>
                          <div className="assign-athletes-modal__athlete-info">
                            <strong className="assign-athletes-modal__athlete-name">{item.athleteName}</strong>
                            <span className="assign-athletes-modal__athlete-program">
                              Current Program: {item.activeProgramName || 'None'}
                            </span>
                          </div>
                        </Checkbox>
                      </List.Item>
                    )}
                  />
                </Checkbox.Group>
              ) : (
                <div className="assign-athletes-modal__empty">No active athletes on your roster.</div>
              )}
            </div>
          </div>
        </Modal>

      </div>
    </DndContext>
  );
};

export default WorkoutTemplateBuilder;
