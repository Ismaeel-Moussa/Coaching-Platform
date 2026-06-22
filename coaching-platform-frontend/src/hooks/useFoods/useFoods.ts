import { useQuery } from '@tanstack/react-query';
import { searchFoods } from '../../api/food';
import type { FoodsPagedResult, SearchFoodsParams } from '../../types/Food';

export const useSearchFoods = (params: SearchFoodsParams, enabled = true) =>
  useQuery<FoodsPagedResult>({
    queryKey: ['foods', params],
    queryFn: () => searchFoods(params),
    enabled,
    staleTime: 30_000, // food catalog doesn't change frequently
  });
