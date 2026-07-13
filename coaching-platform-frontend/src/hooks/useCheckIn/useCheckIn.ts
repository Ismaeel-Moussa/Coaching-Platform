import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import i18n from '../../i18n/i18n';
import {
  submitCheckIn,
  uploadPhotos,
  deletePhoto,
  getCheckInHistory,
  getPendingCheckIns,
  addCoachNotes,
  getCheckInPhotos,
  getCheckInById,
} from '../../api/checkIn';
import type { SubmitCheckInForm, AddCoachNotesForm, PhotoAngle } from '../../types/CheckIn';

export const useSubmitCheckIn = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: SubmitCheckInForm) => submitCheckIn(form),
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.checkInSubmitted'));
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['coach-dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['coach-pending-checkins'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || i18n.t('common:alerts.checkInFailed'));
    },
  });
};

export const useUploadPhotos = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (variables: {
      checkInId: number;
      files: { Front?: File; Side?: File; Back?: File };
      onProgress?: (progress: number) => void;
    }) => uploadPhotos(variables.checkInId, variables.files, variables.onProgress),
    onSuccess: (data, variables) => {
      antMessage.success(i18n.t('common:alerts.photosUploaded'));
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['checkin-photos', variables.checkInId] });
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || i18n.t('common:alerts.photosUploadFailed'));
    },
  });
};

export const useDeletePhoto = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (variables: { id: number; angle: PhotoAngle }) =>
      deletePhoto(variables.id, variables.angle),
    onSuccess: (_, variables) => {
      antMessage.success(i18n.t('common:alerts.photoDeleted'));
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['checkin-photos', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || i18n.t('common:alerts.photoDeleteFailed'));
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
      antMessage.success(i18n.t('common:alerts.feedbackSaved'));
      queryClient.invalidateQueries({ queryKey: ['checkin-history'] });
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile'] });
    },
    onError: (err: any) => {
      antMessage.error(err.response?.data?.message || i18n.t('common:alerts.feedbackSaveFailed'));
    },
  });
};

export const useGetCheckInPhotos = (id: number) =>
  useQuery({
    queryKey: ['checkin-photos', id],
    queryFn: () => getCheckInPhotos(id),
    enabled: !!id,
  });

export const useGetCheckInById = (id: number, enabled: boolean = true) =>
  useQuery({
    queryKey: ['checkin-detail', id],
    queryFn: () => getCheckInById(id),
    enabled: enabled && !!id,
  });
