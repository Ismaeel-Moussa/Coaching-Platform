import axiosInstance from './axiosInstance';
import type { FoodDto, FoodsPagedResult, SearchFoodsParams } from '../types/Food';

export const searchFoods = async (params: SearchFoodsParams): Promise<FoodsPagedResult> => {
  const response = await axiosInstance.get<FoodsPagedResult>('/foods', { params });
  return response.data;
};

export const getFoodById = async (id: number): Promise<FoodDto> => {
  const response = await axiosInstance.get<FoodDto>(`/foods/${id}`);
  return response.data;
};
