import axiosInstance from './axiosInstance';

export interface NotificationDto {
  id: number;
  recipientUserId: number;
  type: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export const getNotifications = async (isRead?: boolean): Promise<NotificationDto[]> => {
  const params = isRead !== undefined ? { isRead } : {};
  const response = await axiosInstance.get<NotificationDto[]>('/notifications', { params });
  return response.data;
};

export const markAsRead = async (id: number): Promise<void> => {
  await axiosInstance.patch(`/notifications/${id}/read`);
};

export const markAllAsRead = async (): Promise<void> => {
  await axiosInstance.patch('/notifications/read-all');
};
