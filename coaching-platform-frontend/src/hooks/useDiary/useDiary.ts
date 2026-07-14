import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import i18n from '../../i18n/i18n';
import {
  getDiary,
  getMacroSummary,
  logFood,
  bulkLogFood,
  getFilteredItems,
  toggleFavoriteFood,
  toggleFavoriteRecipe,
  removeLogEntry,
  updateWater,
  updateSteps,
} from '../../api/diary';
import type { BulkLogFoodForm, DailyDiaryDto, LogFoodForm, UpdateWaterForm, UpdateStepsForm } from '../../types/Diary';
import type { FoodDto } from '../../types/Food';
import type { RecipeDto } from '../../types/Recipe';
import type { MacroSummaryDto } from '../../types/Athlete';

export const useGetDiary = (date: string) =>
  useQuery<DailyDiaryDto>({
    queryKey: ['diary', date],
    queryFn: () => getDiary(date),
    enabled: !!date,
  });

export const useGetMacroSummary = (date: string) =>
  useQuery<MacroSummaryDto>({
    queryKey: ['diary-summary', date],
    queryFn: () => getMacroSummary(date),
    enabled: !!date,
  });

export const useLogFood = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: LogFoodForm) => logFood(form),
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.foodLogged'));
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error(i18n.t('common:alerts.foodLogFailed')),
  });
};

export const useBulkLogFood = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: BulkLogFoodForm) => bulkLogFood(form),
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.foodLogged'));
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['filtered-nutrition-items'] });
    },
    onError: () => antMessage.error(i18n.t('common:alerts.foodLogFailed')),
  });
};

export const useGetFilteredNutritionItems = (
  type: 'food' | 'recipe',
  source: 'recent' | 'frequent' | 'favorites',
  enabled = true,
) => useQuery<(FoodDto | RecipeDto)[]>({
  queryKey: ['filtered-nutrition-items', type, source],
  queryFn: () => getFilteredItems(type, source),
  enabled,
});

export const useToggleFavoriteFood = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: toggleFavoriteFood,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['filtered-nutrition-items'] }),
  });
};

export const useToggleFavoriteRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: toggleFavoriteRecipe,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['filtered-nutrition-items'] }),
  });
};

export const useRemoveLogEntry = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => removeLogEntry(id),
    onMutate: async (entryId) => {
      await queryClient.cancelQueries({ queryKey: ['diary', date] });
      const previousDiary = queryClient.getQueryData<any>(['diary', date]);

      if (previousDiary) {
        queryClient.setQueryData(['diary', date], {
          ...previousDiary,
          mealLogs: (previousDiary.mealLogs || []).filter((log: any) => log.id !== entryId),
        });
      }

      return { previousDiary };
    },
    onError: (err, entryId, context) => {
      if (context?.previousDiary) {
        queryClient.setQueryData(['diary', date], context.previousDiary);
      }
      antMessage.error(i18n.t('common:alerts.entryRemoveFailed'));
    },
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.entryRemoved'));
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
  });
};

export const useUpdateWater = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateWaterForm) => updateWater(date, body),
    onMutate: async (newWater) => {
      await queryClient.cancelQueries({ queryKey: ['diary', date] });
      const previousDiary = queryClient.getQueryData<any>(['diary', date]);

      if (previousDiary) {
        queryClient.setQueryData(['diary', date], {
          ...previousDiary,
          waterLitersConsumed: newWater.waterLiters,
        });
      }

      return { previousDiary };
    },
    onError: (err, newWater, context) => {
      if (context?.previousDiary) {
        queryClient.setQueryData(['diary', date], context.previousDiary);
      }
      antMessage.error(i18n.t('common:alerts.waterUpdateFailed'));
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
  });
};

export const useUpdateSteps = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateStepsForm) => updateSteps(date, body),
    onMutate: async (newSteps) => {
      await queryClient.cancelQueries({ queryKey: ['diary', date] });
      const previousDiary = queryClient.getQueryData<any>(['diary', date]);

      if (previousDiary) {
        queryClient.setQueryData(['diary', date], {
          ...previousDiary,
          stepsWalked: newSteps.steps,
        });
      }

      return { previousDiary };
    },
    onError: (err, newSteps, context) => {
      if (context?.previousDiary) {
        queryClient.setQueryData(['diary', date], context.previousDiary);
      }
      antMessage.error(i18n.t('common:alerts.stepsUpdateFailed'));
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
  });
};
