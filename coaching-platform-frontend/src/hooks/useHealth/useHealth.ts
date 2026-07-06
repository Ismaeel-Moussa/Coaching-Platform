import { useQuery } from '@tanstack/react-query';
import { getHealthStatus } from '../../api/health';
import type { HealthStatusDto } from '../../types/Health';

export const useGetHealth = () => {
  return useQuery<HealthStatusDto>({
    queryKey: ['health-status'],
    queryFn: getHealthStatus,
    refetchInterval: 60000, // Poll every 60 seconds
    staleTime: 55000,
    retry: 1, // Minimize retry attempts since we poll anyway
  });
};
