import axiosInstance from './axiosInstance';
import type { CheckInDto, SubmitCheckInForm, AddCoachNotesForm, PendingCheckInDto, CheckInPhotoDto, PhotoAngle } from '../types/CheckIn';
import type { PagedResult } from '../types/CoachHub';

export const submitCheckIn = async (form: SubmitCheckInForm): Promise<CheckInDto> => {
  const response = await axiosInstance.post<CheckInDto>('/checkins', form);
  return response.data;
};

export const uploadPhotos = async (
  id: number,
  files: { Front?: File; Side?: File; Back?: File },
  onProgress?: (progress: number) => void
): Promise<CheckInDto> => {
  const formData = new FormData();
  if (files.Front) formData.append('Front', files.Front);
  if (files.Side) formData.append('Side', files.Side);
  if (files.Back) formData.append('Back', files.Back);

  const response = await axiosInstance.post<CheckInDto>(`/checkins/${id}/photos`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
    onUploadProgress: (progressEvent) => {
      if (progressEvent.total && onProgress) {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        onProgress(percentCompleted);
      }
    },
  });
  return response.data;
};

export const deletePhoto = async (id: number, angle: PhotoAngle): Promise<void> => {
  await axiosInstance.delete(`/checkins/${id}/photos/${angle}`);
};

export const getCheckInHistory = async (
  page: number = 1,
  pageSize: number = 10,
  athleteId?: number
): Promise<PagedResult<CheckInDto>> => {
  const response = await axiosInstance.get<PagedResult<CheckInDto>>('/checkins/history', {
    params: { page, pageSize, athleteId },
  });
  return response.data;
};

export const getPendingCheckIns = async (
  page: number = 1,
  pageSize: number = 20
): Promise<PagedResult<PendingCheckInDto>> => {
  const response = await axiosInstance.get<PagedResult<PendingCheckInDto>>('/checkins/pending', {
    params: { page, pageSize },
  });
  return response.data;
};

export const addCoachNotes = async (
  id: number,
  form: AddCoachNotesForm
): Promise<CheckInDto> => {
  const response = await axiosInstance.put<CheckInDto>(`/checkins/${id}/coach-notes`, form);
  return response.data;
};

export const getCheckInPhotos = async (id: number): Promise<CheckInPhotoDto[]> => {
  const response = await axiosInstance.get<CheckInPhotoDto[]>(`/checkins/${id}/photos`);
  return response.data;
};

export const getCheckInById = async (id: number): Promise<CheckInDto> => {
  const response = await axiosInstance.get<CheckInDto>(`/checkins/${id}`);
  return response.data;
};
