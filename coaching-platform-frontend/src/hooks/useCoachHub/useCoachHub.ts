import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import {
  getCoachDashboard,
  getLiveFeed,
  getCompliance,
  getRoster,
  getAthleteProfile,
  saveFeedbackNote,
  getWeightHistory,
} from '../../api/coachHub';
import type { SaveFeedbackNoteForm } from '../../types/CoachHub';

export const useGetCoachDashboard = () =>
  useQuery({
    queryKey: ['coach-dashboard'],
    queryFn: getCoachDashboard,
  });

export const useGetLiveFeed = (page: number, pageSize: number) =>
  useQuery({
    queryKey: ['coach-live-feed', page, pageSize],
    queryFn: () => getLiveFeed(page, pageSize),
    refetchInterval: 30000,
  });

export const useGetCompliance = () =>
  useQuery({
    queryKey: ['coach-compliance'],
    queryFn: getCompliance,
    refetchInterval: 30000,
  });

export const useGetRoster = (page: number, pageSize: number, filter?: string | null) =>
  useQuery({
    queryKey: ['coach-roster', page, pageSize, filter],
    queryFn: () => getRoster(page, pageSize, filter),
  });

export const useGetAthleteProfile = (id: number) =>
  useQuery({
    queryKey: ['coach-athlete-profile', id],
    queryFn: () => getAthleteProfile(id),
    enabled: !!id && !isNaN(id),
  });

export const useSaveFeedbackNote = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: SaveFeedbackNoteForm) => saveFeedbackNote(athleteId, form),
    onSuccess: () => {
      antMessage.success('Feedback note saved successfully!');
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile', athleteId] });
    },
    onError: () => {
      antMessage.error('Failed to save feedback note.');
    },
  });
};

export const useGetWeightHistory = (id: number) =>
  useQuery({
    queryKey: ['coach-athlete-weight-history', id],
    queryFn: () => getWeightHistory(id),
    enabled: !!id && !isNaN(id),
  });
