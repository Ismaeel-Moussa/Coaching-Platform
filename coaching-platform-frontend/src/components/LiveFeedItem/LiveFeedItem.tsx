import React from 'react';
import { useNavigate } from 'react-router-dom';
import type { LiveFeedItemDto } from '../../types/CoachHub';
import { formatRelativeTime } from '../../utils/date';
import './LiveFeedItem.scss';

interface LiveFeedItemProps {
  item: LiveFeedItemDto;
}

const STATUS_CONFIG = {
  Completed: {
    label: 'Completed',
    className: 'live-feed-item__status--completed',
    icon: 'check_circle',
  },
  InProgress: {
    label: 'In Progress',
    className: 'live-feed-item__status--in-progress',
    icon: 'sync',
  },
  Missed: {
    label: 'Missed',
    className: 'live-feed-item__status--missed',
    icon: 'cancel',
  },
};

const LiveFeedItem: React.FC<LiveFeedItemProps> = ({ item }) => {
  const navigate = useNavigate();

  const getInitials = (name: string) => {
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    }
    return parts[0] ? parts[0][0].toUpperCase() : '';
  };

  const handleAthleteClick = () => {
    if (item.athleteId) {
      navigate(`/coach/roster/${item.athleteId}`);
    }
  };

  const config = STATUS_CONFIG[item.status] || STATUS_CONFIG.InProgress;

  return (
    <div className="live-feed-item">
      <div 
        className="live-feed-item__avatar-container"
        onClick={handleAthleteClick}
      >
        {item.athleteAvatarUrl ? (
          <img
            src={item.athleteAvatarUrl}
            alt={item.athleteName}
            className="live-feed-item__avatar"
          />
        ) : (
          <div className="live-feed-item__avatar-placeholder">
            {getInitials(item.athleteName)}
          </div>
        )}
      </div>

      <div className="live-feed-item__content">
        <div className="live-feed-item__header">
          <span 
            className="live-feed-item__name"
            onClick={handleAthleteClick}
          >
            {item.athleteName}
          </span>
          <span className="live-feed-item__time">
            {item.status === 'Completed' && item.completedAt
              ? formatRelativeTime(item.completedAt)
              : formatRelativeTime(`${item.date}T00:00:00`)}
          </span>
        </div>
        <p className="live-feed-item__action">
          Workout Day: <span className="live-feed-item__workout">{item.workoutDayLabel}</span>
        </p>
      </div>

      <div className={`live-feed-item__status ${config.className}`}>
        <span className="material-symbols-outlined live-feed-item__status-icon">
          {config.icon}
        </span>
        <span className="live-feed-item__status-label">{config.label}</span>
      </div>
    </div>
  );
};

export default LiveFeedItem;
