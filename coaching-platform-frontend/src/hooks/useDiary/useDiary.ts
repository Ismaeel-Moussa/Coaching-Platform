import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import {
  getDiary,
  getMacroSummary,
  logFood,
  removeLogEntry,
  updateWater,
  updateSteps,
} from '../../api/diary';
import type { DailyDiaryDto, LogFoodForm, UpdateWaterForm, UpdateStepsForm } from '../../types/Diary';
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
      antMessage.success('Food logged successfully!');
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error('Failed to log food. Please try again.'),
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
      antMessage.error('Failed to remove entry.');
    },
    onSuccess: () => {
      antMessage.success('Entry removed.');
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
      antMessage.error('Failed to update water intake.');
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
      antMessage.error('Failed to update steps.');
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
  });
};
