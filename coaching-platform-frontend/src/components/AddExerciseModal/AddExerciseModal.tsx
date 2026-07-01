import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Button } from 'antd';
import { useCreateExercise, useUpdateExercise } from '../../hooks/useExercises/useExercises';
import type { ExerciseAdminDto, MuscleGroup } from '../../types/Exercise';
import './AddExerciseModal.scss';

interface AddExerciseModalProps {
  visible: boolean;
  onCancel: () => void;
  exercise?: ExerciseAdminDto | null;
}

const MUSCLE_GROUPS: MuscleGroup[] = ['Chest', 'Back', 'Shoulders', 'Arms', 'Legs', 'Cardio', 'Core'];

const AddExerciseModal: React.FC<AddExerciseModalProps> = ({ visible, onCancel, exercise }) => {
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
      title={isEdit ? 'Edit Exercise' : 'Add New Exercise'}
      open={visible}
      onCancel={onCancel}
      footer={[
        <Button key="cancel" onClick={onCancel}>
          Cancel
        </Button>,
        <Button
          key="submit"
          type="primary"
          onClick={handleSubmit}
          loading={createExerciseMutation.isPending || updateExerciseMutation.isPending}
          style={{ backgroundColor: 'var(--color-gold)', color: 'var(--color-navy)', border: 'none', fontWeight: 600 }}
        >
          {isEdit ? 'Save Changes' : 'Add Exercise'}
        </Button>,
      ]}
      className="add-exercise-modal"
      width={600}
    >
      <Form form={form} layout="vertical" className="add-exercise-modal__form">
        <Form.Item
          name="name"
          label="Exercise Name"
          rules={[{ required: true, message: 'Please enter exercise name' }]}
        >
          <Input placeholder="e.g. Incline Bench Press" />
        </Form.Item>

        <Form.Item
          name="primaryMuscle"
          label="Primary Muscle Group"
          rules={[{ required: true, message: 'Please select primary muscle group' }]}
        >
          <Select placeholder="Select primary muscle group">
            {MUSCLE_GROUPS.map((m) => (
              <Select.Option key={m} value={m}>
                {m}
              </Select.Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item name="equipmentRequired" label="Equipment Needed">
          <Input placeholder="e.g. Barbell, Incline Bench" />
        </Form.Item>

        <Form.Item name="instructions" label="Instructions">
          <Input.TextArea rows={4} placeholder="Enter step-by-step instructions..." />
        </Form.Item>

        <Form.Item
          name="youTubeVideoId"
          label="YouTube Video ID"
          help="Provide ONLY the 11-character video ID (e.g. rT7DgCr-3pg), not the full URL."
        >
          <Input placeholder="e.g. rT7DgCr-3pg" />
        </Form.Item>

        {watchYouTubeVideoId && watchYouTubeVideoId.trim().length === 11 && (
          <div className="add-exercise-modal__video-preview">
            <span className="add-exercise-modal__video-label">Video Preview:</span>
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
