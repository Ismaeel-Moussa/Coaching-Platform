import axiosInstance from './axiosInstance';
import type { AthleteDashboardDto } from '../types/Athlete';
import type { CoachFeedbackNoteDto } from '../types/CoachHub';

export const getAthleteDashboard = async (): Promise<AthleteDashboardDto> => {
  const response = await axiosInstance.get<AthleteDashboardDto>('/athletes/me/dashboard');
  return response.data;
};

export const getFeedbackHistory = async (): Promise<CoachFeedbackNoteDto[]> => {
  const response = await axiosInstance.get<CoachFeedbackNoteDto[]>('/athletes/me/feedback');
  return response.data;
};
