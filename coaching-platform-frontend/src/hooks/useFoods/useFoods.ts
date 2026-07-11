import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import i18n from '../../i18n/i18n';
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
      antMessage.success(i18n.t('common:alerts.foodCreated', { name: data.name }));
      queryClient.invalidateQueries({ queryKey: ['foods'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

export const useUpdateFood = () => {
  const queryClient = useQueryClient();
  return useMutation<FoodDto, AxiosError, { id: number; form: CreateFoodForm }>({
    mutationFn: ({ id, form }) => updateFood(id, form),
    onSuccess: (data) => {
      antMessage.success(i18n.t('common:alerts.foodUpdated', { name: data.name }));
      queryClient.invalidateQueries({ queryKey: ['foods'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

export const useDeleteFood = () => {
  const queryClient = useQueryClient();
  return useMutation<void, AxiosError, number, { previousQueries: { queryKey: any; data: any }[] }>({
    mutationFn: deleteFood,
    onMutate: async (deletedId) => {
      await queryClient.cancelQueries({ queryKey: ['foods'] });
      const queries = queryClient.getQueriesData<any>({ queryKey: ['foods'] });
      const previousQueries = queries.map(([queryKey, data]) => ({ queryKey, data }));

      queries.forEach(([queryKey, data]) => {
        if (data && Array.isArray(data.items)) {
          queryClient.setQueryData(queryKey, {
            ...data,
            items: data.items.filter((item: any) => item.id !== deletedId),
            totalCount: Math.max(0, data.totalCount - 1),
          });
        }
      });

      return { previousQueries };
    },
    onError: (error, deletedId, context) => {
      context?.previousQueries?.forEach(({ queryKey, data }) => {
        queryClient.setQueryData(queryKey, data);
      });
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.foodDeleted'));
      queryClient.invalidateQueries({ queryKey: ['foods'] });
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
          i18n.t('common:alerts.bulkImportPartial', { inserted: data.insertedCount, skipped: data.skippedCount }),
        );
      } else {
        antMessage.success(i18n.t('common:alerts.bulkImportSuccess', { count: data.insertedCount }));
      }
      queryClient.invalidateQueries({ queryKey: ['foods'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
  });
};

