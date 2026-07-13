import React, { useState } from 'react';
import { Button, Tag, Segmented, Skeleton, Empty, Tooltip } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useNotifications } from '../../../contexts/NotificationContext';
import { useQueryClient } from '@tanstack/react-query';
import { getRoster } from '../../../api/coachHub';
import './Notifications.scss';

const Notifications: React.FC = () => {
  const { t } = useTranslation(['common', 'athlete', 'coach']);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const {
    notifications,
    unreadCount,
    loading,
    markNotificationRead,
    markAllNotificationsRead,
  } = useNotifications();

  const [filter, setFilter] = useState<'all' | 'unread' | 'read'>('all');

  const filteredNotifications = notifications.filter((n) => {
    if (filter === 'unread') return !n.isRead;
    if (filter === 'read') return n.isRead;
    return true;
  });

  const getNotifIcon = (type: string, message: string) => {
    if (type === 'CoachNote' && message.toLowerCase().includes('workout program template assigned')) {
      return 'fitness_center';
    }
    switch (type) {
      case 'MacroAlert':
        return 'nutrition';
      case 'CoachNote':
        return 'chat';
      case 'WorkoutCompleted':
        return 'fitness_center';
      case 'CheckInSubmitted':
        return 'assignment';
      case 'InvitationAccepted':
        return 'person_add';
      default:
        return 'notifications';
    }
  };

  const getNotifTag = (type: string, message: string) => {
    if (type === 'CoachNote' && message.toLowerCase().includes('workout program template assigned')) {
      return <Tag color="success">{t('common:notifications.tags.workout')}</Tag>;
    }
    switch (type) {
      case 'MacroAlert':
        return <Tag color="blue">{t('common:notifications.tags.nutrition')}</Tag>;
      case 'CoachNote':
        return <Tag color="gold">{t('common:notifications.tags.feedback')}</Tag>;
      case 'WorkoutCompleted':
        return <Tag color="success">{t('common:notifications.tags.workout')}</Tag>;
      case 'CheckInSubmitted':
        return <Tag color="cyan">{t('common:notifications.tags.checkIn')}</Tag>;
      case 'InvitationAccepted':
        return <Tag color="purple">{t('common:notifications.tags.roster')}</Tag>;
      default:
        return <Tag color="default">{t('common:notifications.tags.general')}</Tag>;
    }
  };

  const formatTimestamp = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    } catch {
      return dateStr;
    }
  };

  return (
    <div id="notifications-page" className="notifications-page animate-fade-in">
      <div className="notifications-page__header">
        <div>
          <h1 className="notifications-page__title">{t('common:notifications.title')}</h1>
          <p className="notifications-page__subtitle"
             dangerouslySetInnerHTML={{ __html: t('common:notifications.subtitle', { count: unreadCount }) }}
          />
        </div>
        {unreadCount > 0 && (
          <Button
            type="primary"
            onClick={markAllNotificationsRead}
            icon={<span className="material-symbols-outlined btn-icon">done_all</span>}
            className="notifications-page__read-all-btn"
          >
            {t('common:notifications.markAllRead')}
          </Button>
        )}
      </div>

      <div className="notifications-page__filters">
        <Segmented
          options={[
            { label: t('common:notifications.filterAll'), value: 'all' },
            { label: t('common:notifications.filterUnread', { count: unreadCount }), value: 'unread' },
            { label: t('common:notifications.filterRead'), value: 'read' },
          ]}
          value={filter}
          onChange={(value) => setFilter(value as 'all' | 'unread' | 'read')}
          className="notifications-page__segmented"
        />
      </div>

      {loading ? (
        <div className="notifications-page__loading">
          <Skeleton active avatar paragraph={{ rows: 2 }} />
          <Skeleton active avatar paragraph={{ rows: 2 }} className="mt-4" />
          <Skeleton active avatar paragraph={{ rows: 2 }} className="mt-4" />
        </div>
      ) : filteredNotifications.length === 0 ? (
        <div className="notifications-page__empty">
          <Empty
            description={
              <span className="notifications-page__empty-text">
                {filter === 'unread'
                  ? t('common:notifications.emptyUnread')
                  : filter === 'read'
                  ? t('common:notifications.emptyRead')
                  : t('common:notifications.emptyAll')}
              </span>
            }
          />
        </div>
      ) : (
        <div className="notifications-page__list">
          {filteredNotifications.map((notif) => (
            <div
              key={notif.id}
              className={`notifications-page__item ${
                !notif.isRead ? 'notifications-page__item--unread' : ''
              }`}
              onClick={async () => {
                if (!notif.isRead) {
                  markNotificationRead(notif.id);
                }
                const isCoachPath = window.location.pathname.startsWith('/coach');
                if (notif.type === 'CoachNote') {
                  if (notif.message.toLowerCase().includes('workout program template assigned')) {
                    navigate(isCoachPath ? '/coach/roster' : '/athlete/workouts');
                  } else {
                    if (!isCoachPath) {
                      queryClient.invalidateQueries({ queryKey: ['athlete-feedback-history'] });
                      queryClient.invalidateQueries({ queryKey: ['athlete-dashboard'] });
                    }
                    navigate(isCoachPath ? '/coach/roster' : '/athlete/feedback');
                  }
                } else if (notif.type === 'CheckInSubmitted') {
                  if (isCoachPath) {
                    const match = notif.message.match(/^(.*?) (submitted|updated) their weekly check-in\.$/);
                    if (match) {
                      const athleteName = match[1].trim();
                      try {
                        const rosterData = await getRoster(1, 100);
                        const athlete = rosterData.items.find(
                          (a) => a.athleteName.trim().toLowerCase() === athleteName.toLowerCase()
                        );
                        if (athlete) {
                          navigate(`/coach/roster/${athlete.athleteId}#check-in-history`);
                          return;
                        }
                      } catch (err) {
                        console.error('Error fetching roster to locate notification athlete:', err);
                      }
                    }
                    navigate('/coach/roster');
                  }
                } else if (notif.type === 'WorkoutCompleted') {
                  if (isCoachPath) navigate('/coach/dashboard');
                } else if (notif.type === 'MacroAlert') {
                  if (!isCoachPath) navigate('/athlete/dashboard');
                } else if (notif.type === 'InvitationAccepted') {
                  if (isCoachPath) navigate('/coach/roster');
                }
              }}
            >
              <div className="notifications-page__item-icon-wrapper">
                <span className="material-symbols-outlined notifications-page__item-icon">
                  {getNotifIcon(notif.type, notif.message)}
                </span>
                {!notif.isRead && <span className="notifications-page__item-dot" />}
              </div>

              <div className="notifications-page__item-content">
                <div className="notifications-page__item-meta">
                  {getNotifTag(notif.type, notif.message)}
                  <span className="notifications-page__item-time">
                    {formatTimestamp(notif.createdAt)}
                  </span>
                </div>
                <p className="notifications-page__item-message">{notif.message}</p>
              </div>

              {!notif.isRead && (
                <Tooltip title={t('common:notifications.markReadTooltip')}>
                  <button
                    className="notifications-page__item-action"
                    onClick={(e) => {
                      e.stopPropagation();
                      markNotificationRead(notif.id);
                    }}
                    aria-label="Mark as read"
                  >
                    <span className="material-symbols-outlined">done</span>
                  </button>
                </Tooltip>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Notifications;
