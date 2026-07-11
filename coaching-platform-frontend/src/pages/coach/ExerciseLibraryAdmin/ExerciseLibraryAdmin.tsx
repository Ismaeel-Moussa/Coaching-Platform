import React, { useState } from 'react';
import { Button, Input, Tabs, Dropdown, Skeleton, Empty, Popconfirm } from 'antd';
import { useTranslation } from 'react-i18next';
import { useGetExercises, useDeleteExercise } from '../../../hooks/useExercises/useExercises';
import AddExerciseModal from '../../../components/AddExerciseModal/AddExerciseModal';
import type { ExerciseAdminDto, MuscleGroup } from '../../../types/Exercise';
import './ExerciseLibraryAdmin.scss';

const getMuscleCategoryLabel = (category: string, t: any) => {
  switch (category) {
    case 'All': return t('common:status.all', { defaultValue: 'All' });
    case 'Chest': return t('common:muscleGroups.chest', { defaultValue: 'Chest' });
    case 'Back': return t('common:muscleGroups.back', { defaultValue: 'Back' });
    case 'Shoulders': return t('common:muscleGroups.shoulders', { defaultValue: 'Shoulders' });
    case 'Arms': return t('common:muscleGroups.arms', { defaultValue: 'Arms' });
    case 'Legs': return t('common:muscleGroups.legs', { defaultValue: 'Legs' });
    case 'Cardio': return t('common:muscleGroups.cardio', { defaultValue: 'Cardio' });
    case 'Core': return t('common:muscleGroups.core', { defaultValue: 'Core' });
    default: return category;
  }
};

const ExerciseLibraryAdmin: React.FC = () => {
  const { t } = useTranslation(['common', 'athlete', 'coach']);
  const [activeTab, setActiveTab] = useState<string>('All');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [isModalVisible, setIsModalVisible] = useState<boolean>(false);
  const [selectedExercise, setSelectedExercise] = useState<ExerciseAdminDto | null>(null);

  const CATEGORIES = [
    { label: t('common:status.all', { defaultValue: 'All' }), value: 'All' },
    { label: t('common:muscleGroups.chest', { defaultValue: 'Chest' }), value: 'Chest' },
    { label: t('common:muscleGroups.back', { defaultValue: 'Back' }), value: 'Back' },
    { label: t('common:muscleGroups.shoulders', { defaultValue: 'Shoulders' }), value: 'Shoulders' },
    { label: t('common:muscleGroups.arms', { defaultValue: 'Arms' }), value: 'Arms' },
    { label: t('common:muscleGroups.legs', { defaultValue: 'Legs' }), value: 'Legs' },
    { label: t('common:muscleGroups.cardio', { defaultValue: 'Cardio' }), value: 'Cardio' },
    { label: t('common:muscleGroups.core', { defaultValue: 'Core' }), value: 'Core' },
  ];

  const TAB_ITEMS = CATEGORIES.map((cat) => ({
    key: cat.value,
    label: cat.label,
  }));

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
          <h1 className="exercise-library-admin__title">{t('coach:exerciseLibrary.title')}</h1>
          <p className="exercise-library-admin__subtitle">{t('coach:exerciseLibrary.subtitle')}</p>
        </div>
        <Button
          type="primary"
          onClick={handleAdd}
          className="exercise-library-admin__add-btn"
          icon={<span className="material-symbols-outlined">add</span>}
        >
          {t('coach:exerciseLibrary.addExercise')}
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
          placeholder={t('coach:exerciseLibrary.searchPlaceholder')}
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
          <h3>{t('coach:exerciseLibrary.errorTitle')}</h3>
          <p>{t('coach:exerciseLibrary.errorDesc')}</p>
        </div>
      ) : data?.items.length === 0 ? (
        <Empty description={t('coach:exerciseLibrary.empty')} style={{ padding: '60px 0' }} />
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
                <span className="exercise-library-admin__card-badge font-data">{getMuscleCategoryLabel(item.primaryMuscle, t)}</span>
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
                              {t('coach:exerciseLibrary.edit')}
                            </div>
                          )
                        },
                        {
                          key: 'delete',
                          label: (
                            <Popconfirm
                              title={t('coach:exerciseLibrary.delete')}
                              description={t('coach:assignmentHub.deleteConfirm')}
                              onConfirm={() => handleDelete(item.id)}
                              okText={t('common:confirm.yes')}
                              cancelText={t('common:confirm.no')}
                              okButtonProps={{ danger: true }}
                            >
                              <div className="table-menu-item delete-item">
                                <span className="material-symbols-outlined">delete</span>
                                {t('coach:exerciseLibrary.delete')}
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
                  <strong>{t('coach:exerciseLibrary.equipment')}</strong> {item.equipmentRequired || 'None'}
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
