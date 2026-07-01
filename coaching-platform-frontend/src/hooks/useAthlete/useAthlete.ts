import { useQuery } from '@tanstack/react-query';
import { getAthleteDashboard } from '../../api/athlete';
import type { AthleteDashboardDto } from '../../types/Athlete';

export const useGetDashboard = () =>
  useQuery<AthleteDashboardDto>({
    queryKey: ['athlete-dashboard'],
    queryFn: getAthleteDashboard,
    staleTime: 0,            // always re-fetch fresh data
    refetchInterval: 15_000, // auto-refresh every 15 seconds
    refetchOnWindowFocus: true, // refetch when athlete returns to tab
  });
