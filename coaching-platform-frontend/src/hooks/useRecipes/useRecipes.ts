import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import i18n from '../../i18n/i18n';
import { getRecipes, createRecipe, updateRecipe, quickAddRecipeToDiary, deleteRecipe, uploadRecipeImage } from '../../api/recipe';
import type { RecipeDto, RecipesPagedResult, CreateRecipeForm, UpdateRecipeForm, GetRecipesParams } from '../../types/Recipe';

export const useGetRecipes = (params: GetRecipesParams, enabled = true) =>
  useQuery<RecipesPagedResult>({
    queryKey: ['recipes', params],
    queryFn: () => getRecipes(params),
    enabled,
  });

export const useCreateRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation<RecipeDto, Error, CreateRecipeForm>({
    mutationFn: createRecipe,
    onSuccess: () => {
      // Message handled after full creation step including photo upload
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
    onError: () => antMessage.error(i18n.t('common:alerts.recipeCreateFailed')),
  });
};

export const useUpdateRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation<RecipeDto, Error, { id: number; form: UpdateRecipeForm }>({
    mutationFn: ({ id, form }) => updateRecipe(id, form),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
    onError: () => antMessage.error(i18n.t('common:alerts.recipeUpdateFailed')),
  });
};

export const useQuickAddRecipe = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, mealType }: { id: number; mealType: number }) =>
      quickAddRecipeToDiary(id, mealType),
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.recipeAddedToDiary'));
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error(i18n.t('common:alerts.recipeAddFailed')),
  });
};

export const useDeleteRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation<void, Error, number, { previousQueries: { queryKey: any; data: any }[] }>({
    mutationFn: deleteRecipe,
    onMutate: async (deletedId) => {
      await queryClient.cancelQueries({ queryKey: ['recipes'] });
      const queries = queryClient.getQueriesData<any>({ queryKey: ['recipes'] });
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
    onError: (err, deletedId, context) => {
      context?.previousQueries?.forEach(({ queryKey, data }) => {
        queryClient.setQueryData(queryKey, data);
      });
      antMessage.error(i18n.t('common:alerts.recipeDeleteFailed'));
    },
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.recipeDeleted'));
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
  });
};

export const useUploadRecipeImage = () => {
  const queryClient = useQueryClient();
  return useMutation<RecipeDto, Error, { recipeId: number; imageFile: File }>({
    mutationFn: ({ recipeId, imageFile }) => uploadRecipeImage(recipeId, imageFile),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
    onError: () => antMessage.error(i18n.t('common:alerts.recipeImageUploadFailed')),
  });
};

