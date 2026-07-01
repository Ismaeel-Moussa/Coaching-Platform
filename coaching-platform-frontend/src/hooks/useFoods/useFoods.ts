import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import {
  searchFoods,
  createFood,
  updateFood,
  deleteFood,
  bulkImportFoods,
} from '../../api/food';
import type {
  FoodsPagedResult,
  SearchFoodsParams,
  FoodDto,
  CreateFoodForm,
  BulkImportResultDto,
} from '../../types/Food';

export const useSearchFoods = (params: SearchFoodsParams, enabled = true) =>
  useQuery<FoodsPagedResult, AxiosError>({
    queryKey: ['foods', params],
    queryFn: () => searchFoods(params),
    enabled,
    staleTime: 30_000, // food catalog doesn't change frequently
  });

export const useCreateFood = () => {
  const queryClient = useQueryClient();
  return useMutation<FoodDto, AxiosError, CreateFoodForm>({
    mutationFn: createFood,
    onSuccess: (data) => {
      antMessage.success(`Food "${data.name}" created successfully!`);
      queryClient.invalidateQueries({ queryKey: ['foods'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to create food.';
      antMessage.error(msg);
    },
  });
};

export const useUpdateFood = () => {
  const queryClient = useQueryClient();
  return useMutation<FoodDto, AxiosError, { id: number; form: CreateFoodForm }>({
    mutationFn: ({ id, form }) => updateFood(id, form),
    onSuccess: (data) => {
      antMessage.success(`Food "${data.name}" updated successfully!`);
      queryClient.invalidateQueries({ queryKey: ['foods'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to update food.';
      antMessage.error(msg);
    },
  });
};

export const useDeleteFood = () => {
  const queryClient = useQueryClient();
  return useMutation<void, AxiosError, number>({
    mutationFn: deleteFood,
    onSuccess: () => {
      antMessage.success('Food deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['foods'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to delete food.';
      antMessage.error(msg);
    },
  });
};

export const useBulkImportFoods = () => {
  const queryClient = useQueryClient();
  return useMutation<BulkImportResultDto, AxiosError, File>({
    mutationFn: bulkImportFoods,
    onSuccess: (data) => {
      if (data.skippedCount > 0) {
        antMessage.warning(
          `Import complete. Imported: ${data.insertedCount}, Skipped: ${data.skippedCount}. Check errors.`,
        );
      } else {
        antMessage.success(`Successfully imported ${data.insertedCount} foods!`);
      }
      queryClient.invalidateQueries({ queryKey: ['foods'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to bulk import foods.';
      antMessage.error(msg);
    },
  });
};

