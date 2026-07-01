import axiosInstance from './axiosInstance';
import type {
  WorkoutTemplateSummaryDto,
  WorkoutTemplateDto,
  CreateWorkoutTemplateForm,
  AssignTemplateForm,
} from '../types/Workout';

export interface GetWorkoutTemplatesParams {
  page?: number;
  pageSize?: number;
}

export interface PagedWorkoutTemplateSummaryResult {
  items: WorkoutTemplateSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const getWorkoutTemplates = async (
  params?: GetWorkoutTemplatesParams,
): Promise<PagedWorkoutTemplateSummaryResult> => {
  const response = await axiosInstance.get<PagedWorkoutTemplateSummaryResult>('/workout-templates', { params });
  return response.data;
};

export const getWorkoutTemplateById = async (id: number): Promise<WorkoutTemplateDto> => {
  const response = await axiosInstance.get<WorkoutTemplateDto>(`/workout-templates/${id}`);
  return response.data;
};

export const createWorkoutTemplate = async (form: CreateWorkoutTemplateForm): Promise<WorkoutTemplateDto> => {
  const response = await axiosInstance.post<WorkoutTemplateDto>('/workout-templates', form);
  return response.data;
};

export const updateWorkoutTemplate = async (id: number, form: CreateWorkoutTemplateForm): Promise<WorkoutTemplateDto> => {
  const response = await axiosInstance.put<WorkoutTemplateDto>(`/workout-templates/${id}`, form);
  return response.data;
};

export const assignWorkoutTemplate = async (
  id: number,
  form: AssignTemplateForm,
): Promise<{ assignedCount: number; message: string }> => {
  const response = await axiosInstance.post<{ assignedCount: number; message: string }>(
    `/workout-templates/${id}/assign`,
    form,
  );
  return response.data;
};
