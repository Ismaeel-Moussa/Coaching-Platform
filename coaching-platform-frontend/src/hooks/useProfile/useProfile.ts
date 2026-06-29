import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import { getUserProfile, updateUserProfile, changePassword, uploadAvatar } from '../../api/profile';
import type { UserProfileDto, UpdateProfileForm, ChangePasswordForm } from '../../types/Profile';
import type { AuthUserDto } from '../../types/auth';

export const useGetProfile = () => {
  return useQuery<UserProfileDto>({
    queryKey: ['user-profile'],
    queryFn: getUserProfile,
  });
};

export const useUpdateProfile = () => {
  const queryClient = useQueryClient();

  return useMutation<AuthUserDto, AxiosError, UpdateProfileForm>({
    mutationFn: updateUserProfile,
    onSuccess: (updatedUser) => {
      // Update local storage so that layouts show new name/avatar immediately
      localStorage.setItem('user', JSON.stringify(updatedUser));
      
      // Invalidate profile query to refetch details
      queryClient.invalidateQueries({ queryKey: ['user-profile'] });
      
      antMessage.success('Profile updated successfully!');
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to update profile. Please try again.';
      antMessage.error(msg);
    },
  });
};

export const useChangePassword = () => {
  return useMutation<{ message: string }, AxiosError, ChangePasswordForm>({
    mutationFn: changePassword,
    onSuccess: () => {
      antMessage.success('Password changed successfully!');
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to change password. Make sure your current password is correct.';
      antMessage.error(msg);
    },
  });
};

export const useUploadAvatar = () => {
  const queryClient = useQueryClient();

  return useMutation<{ url: string; user: AuthUserDto }, AxiosError, File>({
    mutationFn: uploadAvatar,
    onSuccess: (data) => {
      localStorage.setItem('user', JSON.stringify(data.user));
      queryClient.invalidateQueries({ queryKey: ['user-profile'] });
      antMessage.success('Profile picture updated successfully!');
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to upload avatar. Please make sure it is a valid image under 5MB.';
      antMessage.error(msg);
    },
  });
};
