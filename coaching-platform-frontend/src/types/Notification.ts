export type NotificationType = 'CheckInSubmitted' | 'WorkoutCompleted' | 'CoachNote' | 'MacroAlert' | 'InvitationAccepted';

export interface NotificationDto {
  id: number;
  type: NotificationType;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface UnreadNotificationCountDto {
  unreadCount: number;
}
