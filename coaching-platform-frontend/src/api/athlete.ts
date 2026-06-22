import axiosInstance from './axiosInstance';
import type { AthleteDashboardDto } from '../types/Athlete';

export const getAthleteDashboard = async (): Promise<AthleteDashboardDto> => {
  const response = await axiosInstance.get<AthleteDashboardDto>('/athletes/me/dashboard');
  return response.data;
};
