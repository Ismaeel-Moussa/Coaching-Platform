import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import i18n from '../../i18n/i18n';
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
      antMessage.success(i18n.t('common:alerts.templateCreated', { name: data.name }));
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

export const useUpdateTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation<WorkoutTemplateDto, AxiosError, { id: number; form: CreateWorkoutTemplateForm }>({
    mutationFn: ({ id, form }) => updateWorkoutTemplate(id, form),
    onSuccess: (data) => {
      antMessage.success(i18n.t('common:alerts.templateUpdated', { name: data.name }));
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
      queryClient.invalidateQueries({ queryKey: ['workout-template', data.id] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

export const useAssignTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation<{ assignedCount: number; message: string }, AxiosError, { id: number; form: AssignTemplateForm }>({
    mutationFn: ({ id, form }) => assignWorkoutTemplate(id, form),
    onSuccess: (_data, variables) => {
      antMessage.success(i18n.t('common:alerts.templateAssigned'));
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
      queryClient.invalidateQueries({ queryKey: ['coach-roster'] });
      queryClient.invalidateQueries({ queryKey: ['coach-dashboard'] });
      variables.form.athleteIds.forEach((athleteId) => {
        queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile', athleteId] });
      });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

export const useDeleteTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation<void, AxiosError, number, { previousQueries: { queryKey: any; data: any }[] }>({
    mutationFn: (id) => deleteWorkoutTemplate(id),
    onMutate: async (deletedId) => {
      await queryClient.cancelQueries({ queryKey: ['workout-templates'] });
      const queries = queryClient.getQueriesData<any>({ queryKey: ['workout-templates'] });
      const previousQueries = queries.map(([queryKey, data]) => ({ queryKey, data }));

      queries.forEach(([queryKey, data]) => {
        if (data && Array.isArray(data.items)) {
          queryClient.setQueryData(queryKey, {
            ...data,
            items: data.items.filter((item: any) => item.id !== deletedId),
            totalCount: Math.max(0, data.totalCount - 1),
          });
        }
      });

      return { previousQueries };
    },
    onError: (error, deletedId, context) => {
      context?.previousQueries?.forEach(({ queryKey, data }) => {
        queryClient.setQueryData(queryKey, data);
      });
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.templateDeleted'));
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
    },
  });
};
