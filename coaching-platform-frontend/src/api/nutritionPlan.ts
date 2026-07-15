import axiosInstance from './axiosInstance';
import type {
  ContentStatus,
  NutritionPlan,
  NutritionPlanAssignment,
  NutritionPlanForm,
  NutritionPlanValidation,
  PagedNutritionPlans,
} from '../types/NutritionPlan';

export interface NutritionPlanFilters {
  status?: ContentStatus;
  search?: string;
  page?: number;
  pageSize?: number;
}

export const getNutritionPlans = async (params?: NutritionPlanFilters) =>
  (await axiosInstance.get<PagedNutritionPlans>('/nutrition-plans', { params })).data;

export const getNutritionPlan = async (id: number) =>
  (await axiosInstance.get<NutritionPlan>(`/nutrition-plans/${id}`)).data;

export const createNutritionPlan = async (form: NutritionPlanForm) =>
  (await axiosInstance.post<NutritionPlan>('/nutrition-plans', form)).data;

export const updateNutritionPlan = async (id: number, form: NutritionPlanForm) =>
  (await axiosInstance.put<NutritionPlan>(`/nutrition-plans/${id}`, form)).data;

export const validateNutritionPlan = async (id: number) =>
  (await axiosInstance.get<NutritionPlanValidation>(`/nutrition-plans/${id}/validation`)).data;

export const changeNutritionPlanStatus = async (id: number, status: ContentStatus, expectedContentVersion: number) =>
  (await axiosInstance.post<NutritionPlan>(`/nutrition-plans/${id}/status`, { status, expectedContentVersion })).data;

export const assignNutritionPlan = async (id: number, athleteIds: number[], notes?: string) =>
  (await axiosInstance.post<{ assignedCount: number }>(`/nutrition-plans/${id}/assign`, { athleteIds, notes })).data;

export const getAthleteNutritionPlan = async (athleteId: number) =>
  (await axiosInstance.get<NutritionPlanAssignment | null>(`/nutrition-plans/athletes/${athleteId}/current`)).data;

export const getMyNutritionPlan = async () =>
  (await axiosInstance.get<NutritionPlanAssignment | null>('/nutrition-plans/me/current')).data;
