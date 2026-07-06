import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
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
    onError: () => antMessage.error('Failed to create recipe.'),
  });
};

export const useUpdateRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation<RecipeDto, Error, { id: number; form: UpdateRecipeForm }>({
    mutationFn: ({ id, form }) => updateRecipe(id, form),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
    onError: () => antMessage.error('Failed to update recipe.'),
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

export const useDeleteRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation<void, Error, number>({
    mutationFn: deleteRecipe,
    onSuccess: () => {
      antMessage.success('Recipe deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
    onError: () => antMessage.error('Failed to delete recipe.'),
  });
};

export const useUploadRecipeImage = () => {
  const queryClient = useQueryClient();
  return useMutation<RecipeDto, Error, { recipeId: number; imageFile: File }>({
    mutationFn: ({ recipeId, imageFile }) => uploadRecipeImage(recipeId, imageFile),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recipes'] });
    },
    onError: () => antMessage.error('Failed to upload recipe image.'),
  });
};

