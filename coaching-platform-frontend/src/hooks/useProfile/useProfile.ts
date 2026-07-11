import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import i18n from '../../i18n/i18n';
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
      
      antMessage.success(i18n.t('common:alerts.profileUpdated'));
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.profileUpdateFailed');
      antMessage.error(msg);
    },
  });
};

export const useChangePassword = () => {
  return useMutation<{ message: string }, AxiosError, ChangePasswordForm>({
    mutationFn: changePassword,
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.passwordChanged'));
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.passwordChangeFailed');
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
      antMessage.success(i18n.t('common:alerts.avatarUpdated'));
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.avatarUploadFailed');
      antMessage.error(msg);
    },
  });
};
