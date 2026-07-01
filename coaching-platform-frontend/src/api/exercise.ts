import axiosInstance from './axiosInstance';
import type {
  ExerciseAdminDto,
  CreateExerciseForm,
  UpdateExerciseForm,
  PagedExerciseResult,
  MuscleGroup,
} from '../types/Exercise';

export interface GetExercisesParams {
  page?: number;
  pageSize?: number;
  muscleGroup?: MuscleGroup;
  search?: string;
}

export const getExercises = async (params?: GetExercisesParams): Promise<PagedExerciseResult> => {
  const response = await axiosInstance.get<PagedExerciseResult>('/exercises', { params });
  return response.data;
};

export const getExerciseById = async (id: number): Promise<ExerciseAdminDto> => {
  const response = await axiosInstance.get<ExerciseAdminDto>(`/exercises/${id}`);
  return response.data;
};

export const createExercise = async (form: CreateExerciseForm): Promise<ExerciseAdminDto> => {
  const response = await axiosInstance.post<ExerciseAdminDto>('/exercises', form);
  return response.data;
};

export const updateExercise = async (id: number, form: UpdateExerciseForm): Promise<ExerciseAdminDto> => {
  const response = await axiosInstance.put<ExerciseAdminDto>(`/exercises/${id}`, form);
  return response.data;
};

export const deleteExercise = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/exercises/${id}`);
};
