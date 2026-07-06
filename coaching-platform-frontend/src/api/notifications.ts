import axiosInstance from './axiosInstance';
import type { NotificationDto } from '../types/Notification';
import type { PagedResult } from '../types/CoachHub';

export const getNotifications = async (
  page: number = 1,
  pageSize: number = 20
): Promise<PagedResult<NotificationDto>> => {
  const response = await axiosInstance.get<PagedResult<NotificationDto>>('/notifications', {
    params: { page, pageSize },
  });
  return response.data;
};

export const getUnreadCount = async (): Promise<{ unreadCount: number }> => {
  const response = await axiosInstance.get<{ unreadCount: number }>('/notifications/count');
  return response.data;
};

export const markAsRead = async (id: number): Promise<void> => {
  await axiosInstance.put(`/notifications/${id}/read`);
};

export const markAllAsRead = async (): Promise<void> => {
  await axiosInstance.put('/notifications/read-all');
};
