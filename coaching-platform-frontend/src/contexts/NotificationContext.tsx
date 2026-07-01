import React, { createContext, useContext, useEffect, useState, useRef } from 'react';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { notification } from 'antd';
import { useQueryClient } from '@tanstack/react-query';
import { getNotifications, markAsRead, markAllAsRead, type NotificationDto } from '../api/notifications';

interface NotificationContextType {
  notifications: NotificationDto[];
  unreadCount: number;
  loading: boolean;
  fetchNotifications: () => Promise<void>;
  markNotificationRead: (id: number) => Promise<void>;
  markAllNotificationsRead: () => Promise<void>;
  connectionState: string;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(true);
  const [connectionState, setConnectionState] = useState<string>('Disconnected');

  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);

  // Get current user and token
  const token = localStorage.getItem('token');
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;
  const isCoach = user?.role === 'Coach' || user?.role === 'Admin';

  const fetchNotifications = async () => {
    if (!token) return;
    try {
      setLoading(true);
      const data = await getNotifications();
      setNotifications(data);
      setUnreadCount(data.filter((n) => !n.isRead).length);
    } catch (err) {
      console.error('Error fetching notifications:', err);
    } finally {
      setLoading(false);
    }
  };

  const markNotificationRead = async (id: number) => {
    try {
      await markAsRead(id);
      setNotifications((prev) =>
        prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
      );
      setUnreadCount((prev) => Math.max(0, prev - 1));
    } catch (err) {
      console.error(`Error marking notification ${id} as read:`, err);
    }
  };

  const markAllNotificationsRead = async () => {
    try {
      await markAllAsRead();
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (err) {
      console.error('Error marking all notifications as read:', err);
    }
  };

  // Fetch initial notifications when token is present
  useEffect(() => {
    if (token) {
      fetchNotifications();
    } else {
      setNotifications([]);
      setUnreadCount(0);
    }
  }, [token]);

  // Manage SignalR Connection
  useEffect(() => {
    if (!token) {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
        setConnectionState('Disconnected');
      }
      return;
    }

    // Build the connection pointing to /hubs/notifications
    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:7000/api';
    const hubUrl = apiUrl.replace(/\/api$/, '') + '/hubs/notifications';

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => localStorage.getItem('token') || '',
      })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    // Handlers
    connection.on('ReceiveNotification', (newNotif: NotificationDto) => {
      setNotifications((prev) => [newNotif, ...prev]);
      setUnreadCount((prev) => prev + 1);

      // Show browser/app toast notification (mostly for athletes, since coaches do not get persistent notifications)
      notification.info({
        message: newNotif.type === 'MacroAlert' 
          ? 'Nutrition Update' 
          : (newNotif.message.includes('workout program template assigned') ? 'Workout Update' : 'New Feedback'),
        description: newNotif.message,
        placement: 'topRight',
        duration: 4.5,
      });
    });

    // Coaches listen to AthleteActivity silent invalidation events
    if (isCoach) {
      connection.on('AthleteActivity', (event: { type: string; athleteId: number; athleteUserId?: number }) => {
        console.log('Received athlete activity event in real-time:', event);

        // Invalidate coach dashboards, compliance, feed & rosters to update live in background
        queryClient.invalidateQueries({ queryKey: ['coach-dashboard'] });
        queryClient.invalidateQueries({ queryKey: ['coach-roster'] });
        queryClient.invalidateQueries({ queryKey: ['coach-compliance'] });
        queryClient.invalidateQueries({ queryKey: ['coach-live-feed'] });
        queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile', event.athleteId] });
        
        if (event.type === 'InvitationAccepted') {
          queryClient.invalidateQueries({ queryKey: ['coach-invitations'] });
        }
      });
    }

    // Start Connection
    const startConnection = async () => {
      try {
        await connection.start();
        setConnectionState('Connected');
        console.log('SignalR connected successfully to NotificationHub.');
      } catch (err: any) {
        // Suppress expected abort errors from React StrictMode mounting/unmounting in dev
        if (err?.name === 'AbortError' || err?.message?.includes('stopped')) {
          return;
        }
        console.error('SignalR Connection Error:', err);
        setConnectionState('Failed');
      }
    };

    startConnection();

    connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting due to error:', error);
      setConnectionState('Reconnecting');
    });

    connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected successfully. Connection ID:', connectionId);
      setConnectionState('Connected');
      fetchNotifications();
    });

    connection.onclose((error) => {
      console.warn('SignalR connection closed:', error);
      setConnectionState('Disconnected');
    });

    // Clean up
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [token, isCoach]);

  return (
    <NotificationContext.Provider
      value={{
        notifications,
        unreadCount,
        loading,
        fetchNotifications,
        markNotificationRead,
        markAllNotificationsRead,
        connectionState,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
};

export const useNotifications = () => {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error('useNotifications must be used within a NotificationProvider');
  }
  return context;
};
