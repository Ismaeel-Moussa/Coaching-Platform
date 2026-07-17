import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import {
  getCoachDashboard,
  getCoachActionItems,
  getLiveFeed,
  getCompliance,
  getRoster,
  getAthleteProfile,
  saveFeedbackNote,
  getWeightHistory,
} from '../../api/coachHub';
import type { CoachActionItemsQuery } from '../../api/coachHub';
import type { SaveFeedbackNoteForm } from '../../types/CoachHub';

export const useGetCoachDashboard = () =>
  useQuery({
    queryKey: ['coach-dashboard'],
    queryFn: getCoachDashboard,
    refetchInterval: 30_000,
    staleTime: 30_000,
  });

export const useGetCoachActionItems = (query: CoachActionItemsQuery) =>
  useQuery({
    queryKey: ['coach-action-items', query],
    queryFn: () => getCoachActionItems(query),
    staleTime: 30_000,
    refetchInterval: 30_000,
  });

export const useGetLiveFeed = (page: number, pageSize: number) =>
  useQuery({
    queryKey: ['coach-live-feed', page, pageSize],
    queryFn: () => getLiveFeed(page, pageSize),
    refetchInterval: 30000,
    staleTime: 15_000,
  });

export const useGetCompliance = () =>
  useQuery({
    queryKey: ['coach-compliance'],
    queryFn: getCompliance,
    refetchInterval: 30000,
    staleTime: 15_000,
  });

export const useGetRoster = (page: number, pageSize: number, filter?: string | null) =>
  useQuery({
    queryKey: ['coach-roster', page, pageSize, filter],
    queryFn: () => getRoster(page, pageSize, filter),
    staleTime: 60_000,
  });

export const useGetAthleteProfile = (id: number) =>
  useQuery({
    queryKey: ['coach-athlete-profile', id],
    queryFn: () => getAthleteProfile(id),
    enabled: !!id && !isNaN(id),
    staleTime: 60_000,
  });

export const useSaveFeedbackNote = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: SaveFeedbackNoteForm) => saveFeedbackNote(athleteId, form),
    onMutate: async (newNoteForm) => {
      await queryClient.cancelQueries({ queryKey: ['coach-athlete-profile', athleteId] });
      const previousProfile = queryClient.getQueryData<any>(['coach-athlete-profile', athleteId]);

      if (previousProfile) {
        const optimisticNote = {
          id: -Date.now(),
          noteText: newNoteForm.noteText,
          coachName: 'You',
          createdAt: new Date().toISOString(),
        };

        queryClient.setQueryData(['coach-athlete-profile', athleteId], {
          ...previousProfile,
          feedbackNotes: [optimisticNote, ...(previousProfile.feedbackNotes || [])],
        });
      }

      return { previousProfile };
    },
    onError: (err, newNoteForm, context) => {
      if (context?.previousProfile) {
        queryClient.setQueryData(['coach-athlete-profile', athleteId], context.previousProfile);
      }
      antMessage.error('Failed to save feedback note.');
    },
    onSuccess: () => {
      antMessage.success('Feedback note saved successfully!');
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile', athleteId] });
    },
  });
};

export const useGetWeightHistory = (id: number) =>
  useQuery({
    queryKey: ['coach-athlete-weight-history', id],
    queryFn: () => getWeightHistory(id),
    enabled: !!id && !isNaN(id),
    staleTime: 300_000,
  });
