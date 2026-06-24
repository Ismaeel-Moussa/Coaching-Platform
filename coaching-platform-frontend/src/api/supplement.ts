import axiosInstance from './axiosInstance';
import type { SupplementDto, ToggleSupplementForm } from '../types/Supplement';

export const getSupplements = async (): Promise<SupplementDto[]> => {
  const response = await axiosInstance.get<SupplementDto[]>('/supplements');
  return response.data;
};

export const toggleSupplement = async (form: ToggleSupplementForm): Promise<SupplementDto> => {
  const response = await axiosInstance.post<SupplementDto>('/supplements/log', form);
  return response.data;
};
