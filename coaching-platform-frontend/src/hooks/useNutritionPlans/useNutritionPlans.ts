import { message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { AxiosError } from 'axios';
import {
  assignNutritionPlan,
  changeNutritionPlanStatus,
  createNutritionPlan,
  getAthleteNutritionPlan,
  getMyNutritionPlan,
  getNutritionPlan,
  getNutritionPlans,
  updateNutritionPlan,
  validateNutritionPlan,
  type NutritionPlanFilters,
} from '../../api/nutritionPlan';
import type { ContentStatus, NutritionPlanForm } from '../../types/NutritionPlan';

const errorMessage = (error: AxiosError) =>
  (error.response?.data as { message?: string })?.message ?? 'Something went wrong.';

export const useNutritionPlans = (filters?: NutritionPlanFilters) =>
  useQuery({ queryKey: ['nutrition-plans', filters], queryFn: () => getNutritionPlans(filters) });

export const useNutritionPlan = (id?: number) =>
  useQuery({ queryKey: ['nutrition-plan', id], queryFn: () => getNutritionPlan(id!), enabled: !!id });

export const useNutritionPlanValidation = (id?: number) =>
  useQuery({ queryKey: ['nutrition-plan-validation', id], queryFn: () => validateNutritionPlan(id!), enabled: !!id });

export const useSaveNutritionPlan = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id?: number; form: NutritionPlanForm }) =>
      id ? updateNutritionPlan(id, form) : createNutritionPlan(form),
    onSuccess: (plan) => {
      message.success('Nutrition plan saved.');
      queryClient.invalidateQueries({ queryKey: ['nutrition-plans'] });
      queryClient.invalidateQueries({ queryKey: ['nutrition-plan', plan.id] });
      queryClient.invalidateQueries({ queryKey: ['nutrition-plan-validation', plan.id] });
    },
    onError: (error: AxiosError) => message.error(errorMessage(error)),
  });
};

export const useChangeNutritionPlanStatus = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status, expectedContentVersion }: { id: number; status: ContentStatus; expectedContentVersion: number }) =>
      changeNutritionPlanStatus(id, status, expectedContentVersion),
    onSuccess: (plan) => {
      message.success(`Plan moved to ${plan.contentStatus}.`);
      queryClient.invalidateQueries({ queryKey: ['nutrition-plans'] });
      queryClient.invalidateQueries({ queryKey: ['nutrition-plan', plan.id] });
    },
    onError: (error: AxiosError) => message.error(errorMessage(error)),
  });
};

export const useAssignNutritionPlan = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, athleteIds, notes }: { id: number; athleteIds: number[]; notes?: string }) =>
      assignNutritionPlan(id, athleteIds, notes),
    onSuccess: ({ assignedCount }, variables) => {
      message.success(`Plan assigned to ${assignedCount} athlete${assignedCount === 1 ? '' : 's'}.`);
      queryClient.invalidateQueries({ queryKey: ['nutrition-plans'] });
      queryClient.invalidateQueries({ queryKey: ['athlete-nutrition-plan'] });
      queryClient.invalidateQueries({ queryKey: ['coach-roster'] });
      queryClient.invalidateQueries({ queryKey: ['coach-dashboard'] });
      variables.athleteIds.forEach((athleteId) => {
        queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile', athleteId] });
      });
    },
    onError: (error: AxiosError) => message.error(errorMessage(error)),
  });
};

export const useAthleteNutritionPlan = (athleteId?: number) =>
  useQuery({
    queryKey: ['athlete-nutrition-plan', athleteId],
    queryFn: () => getAthleteNutritionPlan(athleteId!),
    enabled: !!athleteId,
  });

export const useMyNutritionPlan = () =>
  useQuery({ queryKey: ['my-nutrition-plan'], queryFn: getMyNutritionPlan });
