import axiosInstance from './axiosInstance';
import type { AthleteDashboardDto, DailyLogHistoryDto } from '../types/Athlete';
import type { CoachFeedbackNoteDto } from '../types/CoachHub';

export const getAthleteDashboard = async (): Promise<AthleteDashboardDto> => {
  const response = await axiosInstance.get<AthleteDashboardDto>('/athletes/me/dashboard');
  return response.data;
};

export const getFeedbackHistory = async (): Promise<CoachFeedbackNoteDto[]> => {
  const response = await axiosInstance.get<CoachFeedbackNoteDto[]>('/athletes/me/feedback');
  return response.data;
};

export const getDailyLog = async (athleteId: number, date: string): Promise<DailyLogHistoryDto> => {
  const response = await axiosInstance.get<DailyLogHistoryDto>(`/athletes/${athleteId}/daily-log/${date}`);
  return response.data;
};
