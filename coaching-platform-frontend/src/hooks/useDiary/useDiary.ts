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
    onSuccess: () => {
      antMessage.success('Entry removed.');
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error('Failed to remove entry.'),
  });
};

export const useUpdateWater = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateWaterForm) => updateWater(date, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error('Failed to update water intake.'),
  });
};

export const useUpdateSteps = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateStepsForm) => updateSteps(date, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error('Failed to update steps.'),
  });
};
