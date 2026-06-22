import { useQuery } from '@tanstack/react-query';
import { getAthleteDashboard } from '../../api/athlete';
import type { AthleteDashboardDto } from '../../types/Athlete';

export const useGetDashboard = () =>
  useQuery<AthleteDashboardDto>({
    queryKey: ['athlete-dashboard'],
    queryFn: getAthleteDashboard,
    refetchInterval: 60_000, // auto-refresh every 60 seconds
  });
