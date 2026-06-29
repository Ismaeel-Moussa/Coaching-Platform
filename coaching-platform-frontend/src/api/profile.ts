import axiosInstance from './axiosInstance';
import type { UserProfileDto, UpdateProfileForm, ChangePasswordForm } from '../types/Profile';
import type { AuthUserDto } from '../types/auth';

export const getUserProfile = async (): Promise<UserProfileDto> => {
  const response = await axiosInstance.get<UserProfileDto>('/profile');
  return response.data;
};

export const updateUserProfile = async (form: UpdateProfileForm): Promise<AuthUserDto> => {
  const response = await axiosInstance.put<AuthUserDto>('/profile', form);
  return response.data;
};

export const changePassword = async (form: ChangePasswordForm): Promise<{ message: string }> => {
  const response = await axiosInstance.put<{ message: string }>('/profile/change-password', form);
  return response.data;
};
