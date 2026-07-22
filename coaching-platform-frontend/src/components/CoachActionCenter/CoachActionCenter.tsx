import React from 'react';
import { Avatar, Empty, Skeleton } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { CoachActionItemDto, CoachActionType } from '../../types/CoachHub';
import './CoachActionCenter.scss';

interface CoachActionCenterProps {
  items: CoachActionItemDto[];
  isLoading: boolean;
  maxItems?: number;
}

const actionIcons: Record<CoachActionType, string> = {
  AssessmentReview: 'assignment_late',
  SetupRequired: 'rule',
  CheckInPending: 'event_busy',
  ComplianceAlert: 'warning',
};

const CoachActionCenter: React.FC<CoachActionCenterProps> = ({ items, isLoading, maxItems }) => {
  const { t } = useTranslation(['coach']);
  const navigate = useNavigate();

  const displayedItems = maxItems ? items.slice(0, maxItems) : items;

  const openAction = (item: CoachActionItemDto) => {
    switch (item.type) {
      case 'AssessmentReview':
        navigate(`/coach/roster/${item.athleteId}#onboarding-assessment`);
        break;
      case 'SetupRequired':
        navigate(`/coach/athlete-hub?athleteId=${item.athleteId}`);
        break;
      case 'CheckInPending':
        navigate(`/coach/roster/${item.athleteId}#check-in-history`);
        break;
      case 'ComplianceAlert':
        navigate(`/coach/roster/${item.athleteId}#daily-history`);
        break;
    }
  };

  const description = (item: CoachActionItemDto) => {
    if (item.type === 'SetupRequired') {
      return t('coach:actionCenter.descriptions.SetupRequired', {
        current: item.progressCurrent ?? 0,
        total: item.progressTotal ?? 5,
      });
    }

    if (item.type === 'ComplianceAlert') {
      return t('coach:actionCenter.descriptions.ComplianceAlert', {
        value: Math.round(item.metricValue ?? 0),
      });
    }

    return t(`coach:actionCenter.descriptions.${item.type}`);
  };

  return (
    <section className="coach-action-center" aria-labelledby="coach-action-center-title">
      <header className="coach-action-center__header">
        <div className="coach-action-center__heading">
          <span className="coach-action-center__heading-icon material-symbols-outlined">task_alt</span>
          <div>
            <h2 id="coach-action-center-title">{t('coach:actionCenter.title')}</h2>
            <p>{t('coach:actionCenter.subtitle')}</p>
          </div>
        </div>
        <div className="coach-action-center__header-actions">
          {!isLoading && items.length > 0 && (
            <span className="coach-action-center__count mono">
              {t('coach:actionCenter.count', { count: items.length })}
            </span>
          )}
          <button type="button" className="coach-action-center__view-all" onClick={() => navigate('/coach/tasks')}>
            {t('coach:actionCenter.viewAll')}
            <span className="material-symbols-outlined" aria-hidden="true">arrow_forward</span>
          </button>
        </div>
      </header>

      {isLoading ? (
        <div className="coach-action-center__loading">
          {[1, 2, 3].map((item) => <Skeleton key={item} active avatar paragraph={{ rows: 1 }} />)}
        </div>
      ) : items.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={t('coach:actionCenter.empty')}
          className="coach-action-center__empty"
        />
      ) : (
        <div className="coach-action-center__list">
          {displayedItems.map((item) => (
            <article key={`${item.athleteId}-${item.type}`} className="coach-action-center__item">
              <div className={`coach-action-center__type-icon coach-action-center__type-icon--${item.priority.toLowerCase()}`}>
                <span className="material-symbols-outlined">{actionIcons[item.type]}</span>
              </div>
              <Avatar src={item.athleteAvatarUrl ?? undefined} className="coach-action-center__avatar">
                {item.athleteName.charAt(0)}
              </Avatar>
              <div className="coach-action-center__content">
                <div className="coach-action-center__title-row">
                  <strong>{item.athleteName}</strong>
                  <span className={`coach-action-center__priority coach-action-center__priority--${item.priority.toLowerCase()}`}>
                    {t(`coach:actionCenter.priorities.${item.priority}`)}
                  </span>
                </div>
                <p>{description(item)}</p>
              </div>
              <button type="button" className="coach-action-center__action" onClick={() => openAction(item)}>
                {t(`coach:actionCenter.actions.${item.type}`)}
                <span className="material-symbols-outlined">arrow_forward</span>
              </button>
            </article>
          ))}
        </div>
      )}
    </section>
  );
};

export default CoachActionCenter;
