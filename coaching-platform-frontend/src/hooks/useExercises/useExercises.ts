import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
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
  });

export const useCreateExercise = () => {
  const queryClient = useQueryClient();
  return useMutation<ExerciseAdminDto, AxiosError, CreateExerciseForm>({
    mutationFn: createExercise,
    onSuccess: (data) => {
      antMessage.success(`Exercise "${data.name}" created successfully!`);
      queryClient.invalidateQueries({ queryKey: ['exercises'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to create exercise.';
      antMessage.error(msg);
    },
  });
};

export const useUpdateExercise = () => {
  const queryClient = useQueryClient();
  return useMutation<ExerciseAdminDto, AxiosError, { id: number; form: UpdateExerciseForm }>({
    mutationFn: ({ id, form }) => updateExercise(id, form),
    onSuccess: (data) => {
      antMessage.success(`Exercise "${data.name}" updated successfully!`);
      queryClient.invalidateQueries({ queryKey: ['exercises'] });
      queryClient.invalidateQueries({ queryKey: ['exercise', data.id] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to update exercise.';
      antMessage.error(msg);
    },
  });
};

export const useDeleteExercise = () => {
  const queryClient = useQueryClient();
  return useMutation<void, AxiosError, number>({
    mutationFn: deleteExercise,
    onSuccess: () => {
      antMessage.success('Exercise deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['exercises'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to delete exercise.';
      antMessage.error(msg);
    },
  });
};
