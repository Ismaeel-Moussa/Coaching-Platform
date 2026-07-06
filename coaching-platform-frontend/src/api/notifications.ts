import axiosInstance from './axiosInstance';
import type { NotificationDto } from '../types/Notification';

export const getNotifications = async (isRead?: boolean): Promise<NotificationDto[]> => {
  const params = isRead !== undefined ? { isRead } : {};
  const response = await axiosInstance.get<NotificationDto[]>('/notifications', { params });
  return response.data;
};

export const getUnreadCount = async (): Promise<{ unreadCount: number }> => {
  const response = await axiosInstance.get<{ unreadCount: number }>('/notifications/count');
  return response.data;
};

export const markAsRead = async (id: number): Promise<void> => {
  await axiosInstance.patch(`/notifications/${id}/read`);
};

export const markAllAsRead = async (): Promise<void> => {
  await axiosInstance.patch('/notifications/read-all');
};
