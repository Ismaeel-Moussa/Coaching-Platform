import { useQuery } from '@tanstack/react-query';
import { getAthleteDashboard, getFeedbackHistory, getDailyLog } from '../../api/athlete';
import type { AthleteDashboardDto, DailyLogHistoryDto } from '../../types/Athlete';
import type { CoachFeedbackNoteDto } from '../../types/CoachHub';

export const useGetDashboard = () =>
  useQuery<AthleteDashboardDto>({
    queryKey: ['athlete-dashboard'],
    queryFn: getAthleteDashboard,
    staleTime: 0,            // always re-fetch fresh data
    refetchInterval: 15_000, // auto-refresh every 15 seconds
    refetchOnWindowFocus: true, // refetch when athlete returns to tab
  });

export const useGetFeedbackHistory = () =>
  useQuery<CoachFeedbackNoteDto[]>({
    queryKey: ['athlete-feedback-history'],
    queryFn: getFeedbackHistory,
  });

export const useGetDailyLog = (athleteId: number, date: string) =>
  useQuery<DailyLogHistoryDto>({
    queryKey: ['daily-log-history', athleteId, date],
    queryFn: () => getDailyLog(athleteId, date),
    enabled: !!athleteId && !!date,
  });
