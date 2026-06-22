import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import { getRecipes, createRecipe, quickAddRecipeToDiary } from '../../api/recipe';
import type { RecipeDto, RecipesPagedResult, CreateRecipeForm, GetRecipesParams } from '../../types/Recipe';

export const useGetRecipes = (params: GetRecipesParams) =>
  useQuery<RecipesPagedResult>({
    queryKey: ['recipes', params],
    queryFn: () => getRecipes(params),
  });

export const useCreateRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation<RecipeDto, Error, CreateRecipeForm>({
    mutationFn: createRecipe,
    onSuccess: () => {
      antMessage.success('Recipe created successfully!');
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
    onError: () => antMessage.error('Failed to create recipe.'),
  });
};

export const useQuickAddRecipe = (date: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, mealType }: { id: number; mealType: number }) =>
      quickAddRecipeToDiary(id, mealType),
    onSuccess: () => {
      antMessage.success('Recipe added to diary!');
      queryClient.invalidateQueries({ queryKey: ['diary', date] });
      queryClient.invalidateQueries({ queryKey: ['diary-summary', date] });
      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
    },
    onError: () => antMessage.error('Failed to add recipe to diary.'),
  });
};
