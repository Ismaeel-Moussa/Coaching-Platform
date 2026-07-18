import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Button } from 'antd';
import { useTranslation } from 'react-i18next';
import { useCreateExercise, useUpdateExercise } from '../../hooks/useExercises/useExercises';
import type { ExerciseAdminDto, MuscleGroup } from '../../types/Exercise';
import './AddExerciseModal.scss';

interface AddExerciseModalProps {
  visible: boolean;
  onCancel: () => void;
  exercise?: ExerciseAdminDto | null;
}dd

const MUSCLE_GROUPS: MuscleGroup[] = ['Chest', 'Back', 'Shoulders', 'Arms', 'Legs', 'Cardio', 'Core'];

const getMuscleCategoryLabel = (category: string, t: any) => {
  switch (category) {
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

const AddExerciseModal: React.FC<AddExerciseModalProps> = ({ visible, onCancel, exercise }) => {
  const { t } = useTranslation(['common', 'athlete', 'coach']);
  const [form] = Form.useForm();
  const createExerciseMutation = useCreateExercise();
  const updateExerciseMutation = useUpdateExercise();

  const isEdit = !!exercise;

  useEffect(() => {
    if (visible) {
      if (exercise) {
        form.setFieldsValue({
          name: exercise.name,
          primaryMuscle: exercise.primaryMuscle,
          equipmentRequired: exercise.equipmentRequired ?? '',
          instructions: exercise.instructions ?? '',
          youTubeVideoId: exercise.youTubeVideoId ?? '',
        });
      } else {
        form.resetFields();
      }
    }
  }, [visible, exercise, form]);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      const formPayload = {
        name: values.name,
        primaryMuscle: values.primaryMuscle,
        equipmentRequired: values.equipmentRequired || undefined,
        instructions: values.instructions || undefined,
        youTubeVideoId: values.youTubeVideoId || undefined,
      };

      if (isEdit && exercise) {
        await updateExerciseMutation.mutateAsync({ id: exercise.id, form: formPayload });
      } else {
        await createExerciseMutation.mutateAsync(formPayload);
      }
      onCancel();
    } catch (err) {
      // Validation failed or API error handled by hook
    }
  };

  const watchYouTubeVideoId = Form.useWatch('youTubeVideoId', form);

  return (
    <Modal
      title={isEdit ? t('athlete:components.addExerciseModal.titleEdit') : t('athlete:components.addExerciseModal.titleAdd')}
      open={visible}
      onCancel={onCancel}
      footer={[
        <Button key="cancel" onClick={onCancel}>
          {t('common:actions.cancel')}
        </Button>,
        <Button
          key="submit"
          type="primary"
          onClick={handleSubmit}
          loading={createExerciseMutation.isPending || updateExerciseMutation.isPending}
          style={{ backgroundColor: 'var(--color-gold)', color: 'var(--color-navy)', border: 'none', fontWeight: 600 }}
        >
          {isEdit ? t('common:actions.save') : t('coach:exerciseLibrary.addExercise')}
        </Button>,
      ]}
      className="add-exercise-modal"
      width={600}
    >
      <Form form={form} layout="vertical" className="add-exercise-modal__form">
        <Form.Item
          name="name"
          label={t('athlete:components.addExerciseModal.name')}
          rules={[{ required: true, message: t('coach:exerciseLibrary.enterName', { defaultValue: 'Please enter exercise name' }) }]}
        >
          <Input placeholder={t('coach:exerciseLibrary.namePlaceholder', { defaultValue: 'e.g. Incline Bench Press' })} />
        </Form.Item>

        <Form.Item
          name="primaryMuscle"
          label={t('athlete:components.addExerciseModal.muscle')}
          rules={[{ required: true, message: t('coach:exerciseLibrary.selectMuscle', { defaultValue: 'Please select primary muscle group' }) }]}
        >
          <Select placeholder={t('coach:exerciseLibrary.selectMuscle', { defaultValue: 'Select primary muscle group' })}>
            {MUSCLE_GROUPS.map((m) => (
              <Select.Option key={m} value={m}>
                {getMuscleCategoryLabel(m, t)}
              </Select.Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item name="equipmentRequired" label={t('athlete:components.addExerciseModal.equipment')}>
          <Input placeholder={t('coach:exerciseLibrary.equipmentPlaceholder', { defaultValue: 'e.g. Barbell, Incline Bench' })} />
        </Form.Item>

        <Form.Item name="instructions" label={t('athlete:components.addExerciseModal.instructions')}>
          <Input.TextArea rows={4} placeholder={t('coach:exerciseLibrary.instructionsPlaceholder', { defaultValue: 'Enter step-by-step instructions...' })} />
        </Form.Item>

        <Form.Item
          name="youTubeVideoId"
          label={t('athlete:components.addExerciseModal.video')}
          help={t('coach:exerciseLibrary.videoHelp', { defaultValue: 'Provide ONLY the 11-character video ID (e.g. rT7DgCr-3pg), not the full URL.' })}
        >
          <Input placeholder={t('coach:exerciseLibrary.videoPlaceholder', { defaultValue: 'e.g. rT7DgCr-3pg' })} />
        </Form.Item>

        {watchYouTubeVideoId && watchYouTubeVideoId.trim().length === 11 && (
          <div className="add-exercise-modal__video-preview">
            <span className="add-exercise-modal__video-label">{t('coach:exerciseLibrary.videoPreviewLabel', { defaultValue: 'Video Preview:' })}</span>
            <iframe
              src={`https://www.youtube.com/embed/${watchYouTubeVideoId.trim()}`}
              title="YouTube video player"
              frameBorder="0"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
            ></iframe>
          </div>
        )}
      </Form>
    </Modal>
  );
};

export default AddExerciseModal;
