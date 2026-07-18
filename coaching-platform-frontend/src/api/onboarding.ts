import axiosInstance from './axiosInstance';
import type {
  OnboardingAssessmentDto,
  OnboardingAssessmentForm,
  ReviewOnboardingAssessmentForm,
  ReopenOnboardingAssessmentForm,
} from '../types/Onboarding';

export const getMyOnboardingAssessment = async (): Promise<OnboardingAssessmentDto> => {
  const response = await axiosInstance.get<OnboardingAssessmentDto>('/onboarding/me');
  return response.data;
};

export const saveMyOnboardingDraft = async (
  form: OnboardingAssessmentForm,
): Promise<OnboardingAssessmentDto> => {
  const response = await axiosInstance.put<OnboardingAssessmentDto>('/onboarding/me', form);
  return response.data;
};

export const submitMyOnboardingAssessment = async (
  form: OnboardingAssessmentForm,
): Promise<OnboardingAssessmentDto> => {
  const response = await axiosInstance.post<OnboardingAssessmentDto>('/onboarding/me/submit', form);
  return response.data;
};

export const getAthleteOnboardingAssessment = async (
  athleteId: number,
): Promise<OnboardingAssessmentDto> => {
  const response = await axiosInstance.get<OnboardingAssessmentDto>(`/onboarding/athletes/${athleteId}`);
  return response.data;
};

export const reviewAthleteOnboardingAssessment = async (
  athleteId: number,
  form: ReviewOnboardingAssessmentForm,
): Promise<OnboardingAssessmentDto> => {
  const response = await axiosInstance.put<OnboardingAssessmentDto>(
    `/onboarding/athletes/${athleteId}/review`,
    form,
  );
  return response.data;
};

export const reopenAthleteOnboardingAssessment = async (
  athleteId: number,
  form: ReopenOnboardingAssessmentForm,
): Promise<OnboardingAssessmentDto> => {
  const response = await axiosInstance.put<OnboardingAssessmentDto>(
    `/onboarding/athletes/${athleteId}/reopen`,
    form,
  );
  return response.data;
};

export const uploadOnboardingPhotos = async (
  files: { Front?: File; Side?: File; Back?: File },
  onProgress?: (progress: number) => void
): Promise<OnboardingAssessmentDto> => {
  const formData = new FormData();
  if (files.Front) formData.append('Front', files.Front);
  if (files.Side) formData.append('Side', files.Side);
  if (files.Back) formData.append('Back', files.Back);

  const response = await axiosInstance.post<OnboardingAssessmentDto>('/onboarding/me/photos', formData, {
    onUploadProgress: (progressEvent) => {
      if (progressEvent.total && onProgress) {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        onProgress(percentCompleted);
      }
    },
  });
  return response.data;
};

export const deleteOnboardingPhoto = async (angle: 'Front' | 'Side' | 'Back'): Promise<void> => {
  await axiosInstance.delete(`/onboarding/me/photos/${angle}`);
};

