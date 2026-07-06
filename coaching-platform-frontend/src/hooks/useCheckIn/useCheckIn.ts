import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import {
  submitCheckIn,
  uploadPhotos,
  deletePhoto,
  getCheckInHistory,
  getPendingCheckIns,
  addCoachNotes,
  getCheckInPhotos,
} from '../../api/checkIn';
import type { SubmitCheckInForm, AddCoachNotesForm, PhotoAngle } from '../../types/CheckIn';

export const useSubmitCheckIn = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: SubmitCheckInForm) => submitCheckIn(form),
    onSuccess: () => {
      antMessage.success('Check-in submitted successfully!');
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['coach-dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['coach-pending-checkins'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || 'Failed to submit check-in.');
    },
  });
};

export const useUploadPhotos = (checkInId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (variables: {
      files: { Front?: File; Side?: File; Back?: File };
      onProgress?: (progress: number) => void;
    }) => uploadPhotos(checkInId, variables.files, variables.onProgress),
    onSuccess: () => {
      antMessage.success('Progress photos uploaded successfully!');
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['checkin-photos', checkInId] });
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || 'Failed to upload photos.');
    },
  });
};

export const useDeletePhoto = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (variables: { id: number; angle: PhotoAngle }) =>
      deletePhoto(variables.id, variables.angle),
    onSuccess: (_, variables) => {
      antMessage.success('Photo deleted.');
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['checkin-photos', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || 'Failed to delete photo.');
    },
  });
};

export const useGetCheckInHistory = (page: number = 1, pageSize: number = 10, athleteId?: number) =>
  useQuery({
    queryKey: ['checkin-history', page, pageSize, athleteId],
    queryFn: () => getCheckInHistory(page, pageSize, athleteId),
  });

export const useGetPendingCheckIns = (page: number = 1, pageSize: number = 20) =>
  useQuery({
    queryKey: ['coach-pending-checkins', page, pageSize],
    queryFn: () => getPendingCheckIns(page, pageSize),
  });

export const useAddCoachNotes = (checkInId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: AddCoachNotesForm) => addCoachNotes(checkInId, form),
    onSuccess: () => {
      antMessage.success('Feedback note saved successfully!');
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || 'Failed to save coach notes.');
    },
  });
};

export const useGetCheckInPhotos = (id: number) =>
  useQuery({
    queryKey: ['checkin-photos', id],
    queryFn: () => getCheckInPhotos(id),
    enabled: !!id,
  });
