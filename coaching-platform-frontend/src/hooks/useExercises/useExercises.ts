import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import i18n from '../../i18n/i18n';
import {
  getExercises,
  getExerciseById,
  createExercise,
  updateExercise,
  deleteExercise,
  type GetExercisesParams,
} from '../../api/exercise';
import type {
  ExerciseAdminDto,
  CreateExerciseForm,
  UpdateExerciseForm,
  PagedExerciseResult,
} from '../../types/Exercise';

export const useGetExercises = (params?: GetExercisesParams, enabled = true) =>
  useQuery<PagedExerciseResult, AxiosError>({
    queryKey: ['exercises', params],
    queryFn: () => getExercises(params),
    enabled,
    staleTime: 30_000,
  });

export const useGetExerciseById = (id: number, enabled = true) =>
  useQuery<ExerciseAdminDto, AxiosError>({
    queryKey: ['exercise', id],
    queryFn: () => getExerciseById(id),
    enabled,
    staleTime: 60_000,
  });

export const useCreateExercise = () => {
  const queryClient = useQueryClient();
  return useMutation<ExerciseAdminDto, AxiosError, CreateExerciseForm>({
    mutationFn: createExercise,
    onSuccess: (data) => {
      antMessage.success(i18n.t('common:alerts.exerciseCreated', { name: data.name }));
      queryClient.invalidateQueries({ queryKey: ['exercises'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

export const useUpdateExercise = () => {
  const queryClient = useQueryClient();
  return useMutation<ExerciseAdminDto, AxiosError, { id: number; form: UpdateExerciseForm }>({
    mutationFn: ({ id, form }) => updateExercise(id, form),
    onSuccess: (data) => {
      antMessage.success(i18n.t('common:alerts.exerciseUpdated', { name: data.name }));
      queryClient.invalidateQueries({ queryKey: ['exercises'] });
      queryClient.invalidateQueries({ queryKey: ['exercise', data.id] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

export const useDeleteExercise = () => {
  const queryClient = useQueryClient();
  return useMutation<void, AxiosError, number, { previousQueries: { queryKey: any; data: any }[] }>({
    mutationFn: deleteExercise,
    onMutate: async (deletedId) => {
      await queryClient.cancelQueries({ queryKey: ['exercises'] });
      const queries = queryClient.getQueriesData<any>({ queryKey: ['exercises'] });
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
      antMessage.success(i18n.t('common:alerts.exerciseDeleted'));
      queryClient.invalidateQueries({ queryKey: ['exercises'] });
    },
  });
};
