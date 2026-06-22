import axiosInstance from './axiosInstance';
import type {
  DailyDiaryDto,
  LogFoodForm,
  MealLogDto,
  UpdateWaterForm,
  UpdateStepsForm,
} from '../types/Diary';
import type { MacroSummaryDto } from '../types/Athlete';

export const getDiary = async (date: string): Promise<DailyDiaryDto> => {
  const response = await axiosInstance.get<DailyDiaryDto>(`/diary/${date}`);
  return response.data;
};

export const getMacroSummary = async (date: string): Promise<MacroSummaryDto> => {
  const response = await axiosInstance.get<MacroSummaryDto>(`/diary/summary/${date}`);
  return response.data;
};

export const logFood = async (form: LogFoodForm): Promise<MealLogDto> => {
  const response = await axiosInstance.post<MealLogDto>('/diary/log', form);
  return response.data;
};

export const removeLogEntry = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/diary/log/${id}`);
};

export const updateWater = async (date: string, body: UpdateWaterForm): Promise<void> => {
  await axiosInstance.patch(`/diary/${date}/water`, body);
};

export const updateSteps = async (date: string, body: UpdateStepsForm): Promise<void> => {
  await axiosInstance.patch(`/diary/${date}/steps`, body);
};
