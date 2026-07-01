import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { DndContext } from '@dnd-kit/core';
import { Button, Input, Tabs, List, Modal, Checkbox, Space, Skeleton, Card, Tag, Empty, Popconfirm } from 'antd';
import { useGetExercises } from '../../../hooks/useExercises/useExercises';
import {
  useGetWorkoutTemplates,
  useGetWorkoutTemplateById,
  useSaveTemplate,
  useUpdateTemplate,
  useAssignTemplate,
  useDeleteTemplate,
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
  { dayNumber: 1, dayLabel: 'Day 1', isRestDay: false, exercises: [] },
];

const WorkoutTemplateBuilder: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const templateId = id ? parseInt(id, 10) : null;

  // Local State
  const [activeTab, setActiveTab] = useState<string>('list');
  const [name, setName] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [days, setDays] = useState<WorkoutTemplateDayDto[]>(INITIAL_DAYS);
  const [selectedMuscle, setSelectedMuscle] = useState<string>('All');
  const [exerciseSearch, setExerciseSearch] = useState<string>('');
  const [isAssignModalVisible, setIsAssignModalVisible] = useState<boolean>(false);
  const [selectedAthleteIds, setSelectedAthleteIds] = useState<number[]>([]);
  const [listPage, setListPage] = useState<number>(1);
  const [assigningTemplateId, setAssigningTemplateId] = useState<number | null>(null);

  // TanStack Queries
  const { data: exercisesData, isLoading: isExercisesLoading } = useGetExercises({
    muscleGroup: selectedMuscle !== 'All' ? (selectedMuscle as MuscleGroup) : undefined,
    search: exerciseSearch || undefined,
    page: 1,
    pageSize: 100,
  });

  const { data: rosterData } = useGetRoster(1, 100);

  // Fetch all templates for list tab
  const { data: templatesData, isLoading: isTemplatesLoading } = useGetWorkoutTemplates({
    page: listPage,
    pageSize: 8,
  });

  const { data: existingTemplate, isLoading: isTemplateLoading } = useGetWorkoutTemplateById(
    templateId!,
    !!templateId,
  );

  // Mutations
  const saveTemplateMutation = useSaveTemplate();
  const updateTemplateMutation = useUpdateTemplate();
  const assignTemplateMutation = useAssignTemplate();
  const deleteTemplateMutation = useDeleteTemplate();

  // If in edit mode (URL contains id), switch to builder tab and load data
  useEffect(() => {
    if (templateId) {
      setActiveTab('builder');
    } else {
      setActiveTab('list');
      // Reset builder form when returning to list/create
      setName('');
      setDescription('');
      setDays(INITIAL_DAYS);
    }
  }, [templateId]);

  // Load existing template data in edit mode
  useEffect(() => {
    if (existingTemplate && templateId) {
      setName(existingTemplate.name);
      setDescription(existingTemplate.description || '');
      setDays(
        existingTemplate.days.map((day) => ({
          ...day,
          exercises: day.exercises.map((ex) => ({
            ...ex,
            exerciseId: ex.exercise?.id || ex.exerciseId,
            exerciseName: ex.exercise?.name || ex.exerciseName,
          })),
        })) || []
      );
    }
  }, [existingTemplate, templateId]);

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

  // Update Day Label
  const handleUpdateDayLabel = (dayNumber: number, newLabel: string) => {
    setDays((prevDays) =>
      prevDays.map((day) =>
        day.dayNumber === dayNumber ? { ...day, dayLabel: newLabel } : day
      )
    );
  };

  // Add Day Dynamically
  const handleAddDay = () => {
    setDays((prevDays) => [
      ...prevDays,
      {
        dayNumber: prevDays.length + 1,
        dayLabel: `Day ${prevDays.length + 1}`,
        isRestDay: false,
        exercises: [],
      },
    ]);
  };

  // Remove Day Dynamically
  const handleRemoveDay = (dayNumber: number) => {
    setDays((prevDays) => {
      const filtered = prevDays.filter((d) => d.dayNumber !== dayNumber);
      // Re-number remaining days sequentially
      return filtered.map((d, idx) => ({
        ...d,
        dayNumber: idx + 1,
      }));
    });
  };

  // Save template to DB
  const handleSave = async () => {
    if (!name.trim()) {
      Modal.error({ title: 'Validation Error', content: 'Please enter a name for the template.' });
      return;
    }

    if (days.length === 0) {
      Modal.error({ title: 'Validation Error', content: 'Please include at least one workout day.' });
      return;
    }

    const payload = {
      name,
      description: description || undefined,
      days: days.map((day) => ({
        dayNumber: day.dayNumber,
        dayLabel: day.dayLabel,
        isRestDay: day.isRestDay,
        exercises: day.exercises.map((ex: any) => ({
          exerciseId: ex.exercise?.id || ex.exerciseId,
          exerciseName: ex.exercise?.name || ex.exerciseName,
          section: ex.section,
          orderIndex: ex.orderIndex,
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
      if (document.activeElement instanceof HTMLElement) {
        document.activeElement.blur();
      }
      navigate('/coach/template-builder'); // redirect to roster / main builder page
    } catch (err) {
      // API error handled by hook
    }
  };

  // Open assign modal for a specific template
  const openAssignModal = (id: number) => {
    setAssigningTemplateId(id);
    setSelectedAthleteIds([]);
    setIsAssignModalVisible(true);
  };

  // Assign template flow
  const handleAssign = async () => {
    if (selectedAthleteIds.length === 0) {
      Modal.error({ title: 'Validation Error', content: 'Please select at least one athlete.' });
      return;
    }

    const targetId = assigningTemplateId || templateId;
    if (!targetId) return;

    try {
      await assignTemplateMutation.mutateAsync({
        id: targetId,
        form: { athleteIds: selectedAthleteIds },
      });
      setIsAssignModalVisible(false);
      setSelectedAthleteIds([]);
      setAssigningTemplateId(null);
    } catch (err) {
      // Handled by hook
    }
  };

  const handleTabChange = (key: string) => {
    if (document.activeElement instanceof HTMLElement) {
      document.activeElement.blur();
    }
    if (key === 'list' && templateId) {
      navigate('/coach/template-builder');
    } else {
      setActiveTab(key);
    }
  };

  if (templateId && isTemplateLoading) {
    return (
      <div className="workout-template-builder workout-template-builder--loading">
        <Skeleton active paragraph={{ rows: 10 }} />
      </div>
    );
  }

  const renderTemplatesList = () => {
    if (isTemplatesLoading) {
      return (
        <div style={{ padding: '40px 0' }}>
          <Skeleton active paragraph={{ rows: 6 }} />
        </div>
      );
    }

    const items = templatesData?.items || [];

    if (items.length === 0) {
      return (
        <div className="workout-template-builder__list-empty">
          <Empty
            description={
              <span>No workout program templates created yet. Get started by building one!</span>
            }
          >
            <Button type="primary" onClick={() => handleTabChange('builder')}>
              Create First Template
            </Button>
          </Empty>
        </div>
      );
    }

    return (
      <div className="workout-template-builder__list-container animate-fade-in">
        <div className="workout-template-builder__list-grid">
          {items.map((tmpl: any) => (
            <Card
              key={tmpl.id}
              className="workout-template-builder__template-card"
              title={
                <div className="workout-template-builder__card-title">
                  <span>{tmpl.name}</span>
                  <Tag color={tmpl.isActive ? 'success' : 'default'}>
                    {tmpl.isActive ? 'Active' : 'Archived'}
                  </Tag>
                </div>
              }
              actions={[
                <Button
                  type="link"
                  key="edit"
                  onClick={(e) => {
                    e.currentTarget.blur();
                    navigate(`/coach/template-builder/${tmpl.id}`);
                  }}
                >
                  View / Edit
                </Button>,
                <Button
                  type="link"
                  key="assign"
                  onClick={(e) => {
                    e.currentTarget.blur();
                    openAssignModal(tmpl.id);
                  }}
                >
                  Assign
                </Button>,
                <Popconfirm
                  title="Delete Template"
                  description="Are you sure you want to delete this template?"
                  onConfirm={() => {
                    if (document.activeElement instanceof HTMLElement) {
                      document.activeElement.blur();
                    }
                    deleteTemplateMutation.mutate(tmpl.id);
                  }}
                  okText="Yes, Delete"
                  cancelText="No"
                  okButtonProps={{ danger: true, loading: deleteTemplateMutation.isPending }}
                  key="delete"
                >
                  <Button
                    type="link"
                    danger
                    onClick={(e) => e.currentTarget.blur()}
                  >
                    Delete
                  </Button>
                </Popconfirm>,
              ]}
            >
              <div className="workout-template-builder__card-content">
                <p className="workout-template-builder__card-desc">
                  {tmpl.description || 'No description provided.'}
                </p>
                <div className="workout-template-builder__card-stats">
                  <span className="stat-item">
                    <span className="material-symbols-outlined">calendar_today</span>
                    <strong>{tmpl.dayCount} Days</strong>
                  </span>
                  <span className="stat-item">
                    <span className="material-symbols-outlined">person</span>
                    <span>By Coach {tmpl.coachName}</span>
                  </span>
                </div>
              </div>
            </Card>
          ))}
        </div>
        {templatesData && templatesData.totalCount > 8 && (
          <div className="workout-template-builder__pagination">
            <Button
              disabled={listPage === 1}
              onClick={() => setListPage((p) => p - 1)}
            >
              Previous
            </Button>
            <span className="pagination-text">
              Page {listPage} of {Math.ceil(templatesData.totalCount / 8)}
            </span>
            <Button
              disabled={listPage * 8 >= templatesData.totalCount}
              onClick={() => setListPage((p) => p + 1)}
            >
              Next
            </Button>
          </div>
        )}
      </div>
    );
  };

  return (
    <div id="template-builder-page" className="workout-template-builder animate-fade-in">
      {/* Top Tabs */}
      <Tabs
        activeKey={activeTab}
        onChange={handleTabChange}
        items={[
          {
            key: 'list',
            label: (
              <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                <span className="material-symbols-outlined" style={{ fontSize: 18 }}>view_list</span>
                My Templates
              </span>
            ),
            children: renderTemplatesList(),
          },
          {
            key: 'builder',
            label: (
              <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                <span className="material-symbols-outlined" style={{ fontSize: 18 }}>design_services</span>
                {templateId ? 'Edit Program Template' : 'Create New Template'}
              </span>
            ),
            children: (
              <DndContext onDragEnd={handleDragEnd}>
                <div className="workout-template-builder__builder-tab animate-fade-in">
                  
                  {/* Header Block */}
                  <div className="workout-template-builder__header">
                    <div>
                      <h1 className="workout-template-builder__title">
                        {templateId ? 'Edit Workout Program Template' : 'Workout Program Template Builder'}
                      </h1>
                      <p className="workout-template-builder__subtitle">
                        Drag-and-drop movements to curate personalized hypertrophic workout programs
                      </p>
                    </div>
                    <Space>
                      {templateId && (
                        <Button
                          type="default"
                          onClick={() => openAssignModal(templateId)}
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
                            onUpdateDayLabel={handleUpdateDayLabel}
                            onRemoveDay={handleRemoveDay}
                          />
                        ))}
                        <div 
                          className="workout-template-builder__add-day-card"
                          onClick={handleAddDay}
                          title="Add new workout day"
                        >
                          <span className="material-symbols-outlined">add_circle</span>
                          <span className="add-day-text">Add Day</span>
                        </div>
                      </div>
                    </div>

                  </div>

                </div>
              </DndContext>
            ),
          },
        ]}
      />

      {/* Assign to Athletes Modal */}
      <Modal
        title="Assign Workout Template"
        open={isAssignModalVisible}
        onCancel={() => {
          setIsAssignModalVisible(false);
          setAssigningTemplateId(null);
        }}
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
  );
};

export default WorkoutTemplateBuilder;
