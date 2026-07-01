import axiosInstance from './axiosInstance';
import type {
  FoodDto,
  FoodsPagedResult,
  SearchFoodsParams,
  CreateFoodForm,
  BulkImportResultDto,
} from '../types/Food';

export const searchFoods = async (params: SearchFoodsParams): Promise<FoodsPagedResult> => {
  const response = await axiosInstance.get<FoodsPagedResult>('/foods', { params });
  return response.data;
};

export const getFoodById = async (id: number): Promise<FoodDto> => {
  const response = await axiosInstance.get<FoodDto>(`/foods/${id}`);
  return response.data;
};

export const createFood = async (form: CreateFoodForm): Promise<FoodDto> => {
  const response = await axiosInstance.post<FoodDto>('/foods', form);
  return response.data;
};

export const updateFood = async (id: number, form: CreateFoodForm): Promise<FoodDto> => {
  const response = await axiosInstance.put<FoodDto>(`/foods/${id}`, form);
  return response.data;
};

export const deleteFood = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/foods/${id}`);
};

export const bulkImportFoods = async (file: File): Promise<BulkImportResultDto> => {
  const formData = new FormData();
  formData.append('csvFile', file);
  const response = await axiosInstance.post<BulkImportResultDto>('/foods/bulk-import', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  return response.data;
};

