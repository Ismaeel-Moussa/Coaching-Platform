import axiosInstance from './axiosInstance';
import type {
  TodaysWorkoutDto,
  WorkoutProgramDto,
  SetLogDto,
  WorkoutHistoryDto,
  LogSetForm,
  CompleteWorkoutForm,
} from '../types/Workout';

export const getTodaysWorkout = async (): Promise<TodaysWorkoutDto> => {
  const response = await axiosInstance.get<TodaysWorkoutDto>('/workouts/today');
  return response.data;
};

export const getWorkoutProgram = async (): Promise<WorkoutProgramDto> => {
  const response = await axiosInstance.get<WorkoutProgramDto>('/workouts/program');
  return response.data;
};

export const logSet = async (form: LogSetForm): Promise<SetLogDto> => {
  const response = await axiosInstance.post<SetLogDto>('/workouts/log-set', form);
  return response.data;
};

export const completeWorkout = async (form: CompleteWorkoutForm): Promise<{ message: string }> => {
  const response = await axiosInstance.post<{ message: string }>('/workouts/complete', form);
  return response.data;
};

export const getWorkoutHistory = async (): Promise<WorkoutHistoryDto[]> => {
  const response = await axiosInstance.get<WorkoutHistoryDto[]>('/workouts/history');
  return response.data;
};
