import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import {
  getWorkoutTemplates,
  getWorkoutTemplateById,
  createWorkoutTemplate,
  updateWorkoutTemplate,
  assignWorkoutTemplate,
  deleteWorkoutTemplate,
  type PagedWorkoutTemplateSummaryResult,
  type GetWorkoutTemplatesParams,
} from '../../api/workoutTemplate';
import type {
  WorkoutTemplateDto,
  CreateWorkoutTemplateForm,
  AssignTemplateForm,
} from '../../types/Workout';

export const useGetWorkoutTemplates = (params?: GetWorkoutTemplatesParams, enabled = true) =>
  useQuery<PagedWorkoutTemplateSummaryResult, AxiosError>({
    queryKey: ['workout-templates', params],
    queryFn: () => getWorkoutTemplates(params),
    enabled,
    staleTime: 30_000,
  });

export const useGetWorkoutTemplateById = (id: number, enabled = true) =>
  useQuery<WorkoutTemplateDto, AxiosError>({
    queryKey: ['workout-template', id],
    queryFn: () => getWorkoutTemplateById(id),
    enabled,
  });

export const useSaveTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation<WorkoutTemplateDto, AxiosError, CreateWorkoutTemplateForm>({
    mutationFn: createWorkoutTemplate,
    onSuccess: (data) => {
      antMessage.success(`Template "${data.name}" created successfully!`);
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to create workout template.';
      antMessage.error(msg);
    },
  });
};

export const useUpdateTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation<WorkoutTemplateDto, AxiosError, { id: number; form: CreateWorkoutTemplateForm }>({
    mutationFn: ({ id, form }) => updateWorkoutTemplate(id, form),
    onSuccess: (data) => {
      antMessage.success(`Template "${data.name}" updated successfully!`);
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
      queryClient.invalidateQueries({ queryKey: ['workout-template', data.id] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to update workout template.';
      antMessage.error(msg);
    },
  });
};

export const useAssignTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation<{ assignedCount: number; message: string }, AxiosError, { id: number; form: AssignTemplateForm }>({
    mutationFn: ({ id, form }) => assignWorkoutTemplate(id, form),
    onSuccess: (data) => {
      antMessage.success(data.message || 'Template assigned successfully!');
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
      queryClient.invalidateQueries({ queryKey: ['coach-roster'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to assign template.';
      antMessage.error(msg);
    },
  });
};

export const useDeleteTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation<void, AxiosError, number>({
    mutationFn: (id) => deleteWorkoutTemplate(id),
    onSuccess: () => {
      antMessage.success('Template deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to delete template.';
      antMessage.error(msg);
    },
  });
};
