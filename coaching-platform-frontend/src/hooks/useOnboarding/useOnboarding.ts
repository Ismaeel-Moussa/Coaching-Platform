import { message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { AxiosError } from 'axios';
import i18n from '../../i18n/i18n';
import {
  getAthleteOnboardingAssessment,
  getMyOnboardingAssessment,
  reviewAthleteOnboardingAssessment,
  saveMyOnboardingDraft,
  submitMyOnboardingAssessment,
  uploadOnboardingPhotos,
  deleteOnboardingPhoto,
} from '../../api/onboarding';
import type { OnboardingAssessmentForm, ReviewOnboardingAssessmentForm } from '../../types/Onboarding';

interface ApiErrorBody { message?: string }

const getErrorMessage = (error: unknown, fallback: string) =>
  (error as AxiosError<ApiErrorBody>)?.response?.data?.message ?? fallback;

export const useMyOnboardingAssessment = () => useQuery({
  queryKey: ['onboarding', 'me'],
  queryFn: getMyOnboardingAssessment,
  staleTime: 30_000,
  retry: 1,
});

export const useSaveOnboardingDraft = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: OnboardingAssessmentForm) => saveMyOnboardingDraft(form),
    onSuccess: (data) => {
      queryClient.setQueryData(['onboarding', 'me'], data);
      message.success(i18n.t('athlete:onboarding.messages.draftSaved'));
    },
    onError: (error) => message.error(getErrorMessage(error, i18n.t('athlete:onboarding.messages.saveFailed'))),
  });
};

export const useSubmitOnboardingAssessment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: OnboardingAssessmentForm) => submitMyOnboardingAssessment(form),
    onSuccess: (data) => {
      queryClient.setQueryData(['onboarding', 'me'], data);
      message.success(i18n.t('athlete:onboarding.messages.submitted'));
    },
    onError: (error) => message.error(getErrorMessage(error, i18n.t('athlete:onboarding.messages.submitFailed'))),
  });
};

export const useAthleteOnboardingAssessment = (athleteId: number) => useQuery({
  queryKey: ['onboarding', 'athlete', athleteId],
  queryFn: () => getAthleteOnboardingAssessment(athleteId),
  enabled: athleteId > 0,
});

export const useReviewOnboardingAssessment = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: ReviewOnboardingAssessmentForm) => reviewAthleteOnboardingAssessment(athleteId, form),
    onSuccess: (data) => {
      queryClient.setQueryData(['onboarding', 'athlete', athleteId], data);
      message.success(i18n.t('coach:onboarding.messages.reviewed'));
    },
    onError: (error) => message.error(getErrorMessage(error, i18n.t('coach:onboarding.messages.reviewFailed'))),
  });
};

export const useUploadOnboardingPhotos = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (args: { files: { Front?: File; Side?: File; Back?: File }; onProgress?: (progress: number) => void }) =>
      uploadOnboardingPhotos(args.files, args.onProgress),
    onSuccess: (data) => {
      queryClient.setQueryData(['onboarding', 'me'], data);
      message.success(i18n.t('athlete:onboarding.messages.photosUploaded'));
    },
    onError: (error) => message.error(getErrorMessage(error, i18n.t('athlete:onboarding.messages.uploadFailed'))),
  });
};

export const useDeleteOnboardingPhoto = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (angle: 'Front' | 'Side' | 'Back') => deleteOnboardingPhoto(angle),
    onSuccess: (_, angle) => {
      queryClient.invalidateQueries({ queryKey: ['onboarding', 'me'] });
      message.success(i18n.t('athlete:onboarding.messages.photoDeleted'));
    },
    onError: (error) => message.error(getErrorMessage(error, i18n.t('athlete:onboarding.messages.deleteFailed'))),
  });
};

