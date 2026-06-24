import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import {
  getTodaysWorkout,
  getWorkoutProgram,
  logSet,
  completeWorkout,
  getWorkoutHistory,
} from '../../api/workout';
import type { LogSetForm, CompleteWorkoutForm } from '../../types/Workout';

// ── Queries ───────────────────────────────────────────────────────────────────

export const useGetTodaysWorkout = () =>
  useQuery({
    queryKey: ['workout-today'],
    queryFn: getTodaysWorkout,
  });

export const useGetWorkoutProgram = () =>
  useQuery({
    queryKey: ['workout-program'],
    queryFn: getWorkoutProgram,
  });

export const useGetWorkoutHistory = () =>
  useQuery({
    queryKey: ['workout-history'],
    queryFn: getWorkoutHistory,
  });

// ── Mutations ─────────────────────────────────────────────────────────────────

export const useLogSet = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: LogSetForm) => logSet(form),
    onSuccess: () => {
      antMessage.success('Set logged!');
      queryClient.invalidateQueries({ queryKey: ['workout-today'] });
    },
    onError: () => antMessage.error('Failed to log set. Please try again.'),
  });
};

export const useCompleteWorkout = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: CompleteWorkoutForm) => completeWorkout(form),
    onSuccess: () => {
      antMessage.success('Workout completed! 🔥 Streak updated.');
      queryClient.invalidateQueries({ queryKey: ['workout-today'] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error('Failed to complete workout. Please try again.'),
  });
};
