import React, { useState } from 'react';
import { Button, Input, Tabs, Dropdown, Skeleton, Empty, Popconfirm } from 'antd';
import { useGetExercises, useDeleteExercise } from '../../../hooks/useExercises/useExercises';
import AddExerciseModal from '../../../components/AddExerciseModal/AddExerciseModal';
import type { ExerciseAdminDto, MuscleGroup } from '../../../types/Exercise';
import './ExerciseLibraryAdmin.scss';

const CATEGORIES: { label: string; value: string }[] = [
  { label: 'All', value: 'All' },
  { label: 'Chest', value: 'Chest' },
  { label: 'Back', value: 'Back' },
  { label: 'Shoulders', value: 'Shoulders' },
  { label: 'Arms', value: 'Arms' },
  { label: 'Legs', value: 'Legs' },
  { label: 'Cardio', value: 'Cardio' },
  { label: 'Core', value: 'Core' },
];

const TAB_ITEMS = CATEGORIES.map((cat) => ({
  key: cat.value,
  label: cat.label,
}));

const ExerciseLibraryAdmin: React.FC = () => {
  const [activeTab, setActiveTab] = useState<string>('All');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [isModalVisible, setIsModalVisible] = useState<boolean>(false);
  const [selectedExercise, setSelectedExercise] = useState<ExerciseAdminDto | null>(null);

  // Debounced search parameter handling can be simplified or direct since it's local
  // Pass to TanStack query params
  const { data, isLoading, error } = useGetExercises({
    muscleGroup: activeTab !== 'All' ? (activeTab as MuscleGroup) : undefined,
    search: searchQuery || undefined,
    page: 1,
    pageSize: 100, // Load all for catalog admin management view
  });

  const deleteMutation = useDeleteExercise();

  const handleEdit = (exercise: ExerciseAdminDto) => {
    setSelectedExercise(exercise);
    setIsModalVisible(true);
  };

  const handleAdd = () => {
    setSelectedExercise(null);
    setIsModalVisible(true);
  };

  const handleDelete = (id: number) => {
    deleteMutation.mutate(id);
  };

  return (
    <div id="exercise-library-page" className="exercise-library-admin animate-fade-in">
      <div className="exercise-library-admin__header">
        <div>
          <h1 className="exercise-library-admin__title">Exercise Library</h1>
          <p className="exercise-library-admin__subtitle">Manage the master exercise database for workout templates</p>
        </div>
        <Button
          type="primary"
          onClick={handleAdd}
          className="exercise-library-admin__add-btn"
          icon={<span className="material-symbols-outlined">add</span>}
        >
          Add Exercise
        </Button>
      </div>

      <div className="exercise-library-admin__controls">
        <div className="exercise-library-admin__tabs-wrapper">
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={TAB_ITEMS}
            className="exercise-library-admin__tabs"
          />
        </div>
        <Input
          placeholder="Search by exercise name..."
          prefix={<span className="material-symbols-outlined" style={{ fontSize: 18, color: 'var(--color-border-strong)' }}>search</span>}
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          allowClear
          className="exercise-library-admin__search"
        />
      </div>

      {isLoading ? (
        <div className="exercise-library-admin__loading">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} active paragraph={{ rows: 3 }} />
          ))}
        </div>
      ) : error ? (
        <div className="exercise-library-admin__error">
          <span className="material-symbols-outlined">error</span>
          <h3>Error Loading Exercises</h3>
          <p>Please check your connection and try again.</p>
        </div>
      ) : data?.items.length === 0 ? (
        <Empty description="No exercises found matching filters." style={{ padding: '60px 0' }} />
      ) : (
        <div className="exercise-library-admin__grid">
          {data?.items.map((item) => (
            <div key={item.id} className="exercise-library-admin__card">
              <div className="exercise-library-admin__card-media">
                {item.youTubeVideoId ? (
                  <img
                    src={`https://img.youtube.com/vi/${item.youTubeVideoId}/hqdefault.jpg`}
                    alt={item.name}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = 'https://placehold.co/400x225/0b132b/fdc003?text=Video+Demo';
                    }}
                  />
                ) : (
                  <div className="exercise-library-admin__card-placeholder">
                    <span className="material-symbols-outlined">fitness_center</span>
                  </div>
                )}
                <span className="exercise-library-admin__card-badge font-data">{item.primaryMuscle}</span>
              </div>
              <div className="exercise-library-admin__card-content">
                <div className="exercise-library-admin__card-header">
                  <h3 className="exercise-library-admin__card-name">{item.name}</h3>
                  <Dropdown
                    menu={{
                      items: [
                        {
                          key: 'edit',
                          label: (
                            <div className="table-menu-item" onClick={() => handleEdit(item)}>
                              <span className="material-symbols-outlined">edit</span>
                              Edit
                            </div>
                          )
                        },
                        {
                          key: 'delete',
                          label: (
                            <Popconfirm
                              title="Delete Exercise"
                              description="Are you sure you want to delete this exercise?"
                              onConfirm={() => handleDelete(item.id)}
                              okText="Yes"
                              cancelText="No"
                              okButtonProps={{ danger: true }}
                            >
                              <div className="table-menu-item delete-item">
                                <span className="material-symbols-outlined">delete</span>
                                Delete
                              </div>
                            </Popconfirm>
                          )
                        }
                      ],
                      className: "table-menu-dropdown"
                    }}
                    trigger={['click']}
                    placement="bottomRight"
                  >
                    <Button type="text" className="exercise-library-admin__more-btn">
                      <span className="material-symbols-outlined">more_vert</span>
                    </Button>
                  </Dropdown>
                </div>
                <p className="exercise-library-admin__card-equip">
                  <strong>Equipment:</strong> {item.equipmentRequired || 'None'}
                </p>
                {item.instructions && (
                  <p className="exercise-library-admin__card-desc">
                    {item.instructions.length > 90 ? `${item.instructions.substring(0, 90)}...` : item.instructions}
                  </p>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <AddExerciseModal
        visible={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        exercise={selectedExercise}
      />
    </div>
  );
};

export default ExerciseLibraryAdmin;
