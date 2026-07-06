import axios from 'axios';
import type { HealthStatusDto } from '../types/Health';

const healthApi = axios.create({
  baseURL: import.meta.env.VITE_API_URL as string,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const getHealthStatus = async (): Promise<HealthStatusDto> => {
  const response = await healthApi.get<HealthStatusDto>('/health');
  return response.data;
};
