import React, { useEffect, useState } from 'react';
import { Alert, Avatar, Empty, Input, Pagination, Select, Skeleton } from 'antd';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useGetCoachActionItems } from '../../../hooks/useCoachHub/useCoachHub';
import type { CoachActionItemDto, CoachActionPriority, CoachActionType } from '../../../types/CoachHub';
import './CoachTasks.scss';

const PAGE_SIZE = 12;
const actionTypes: CoachActionType[] = [
  'AssessmentReview',
  'SetupRequired',
  'CheckInPending',
  'ComplianceAlert',
];
const priorities: CoachActionPriority[] = ['High', 'Medium'];

const actionIcons: Record<CoachActionType, string> = {
  AssessmentReview: 'assignment_late',
  SetupRequired: 'rule',
  CheckInPending: 'event_busy',
  ComplianceAlert: 'warning',
};

const isActionType = (value: string | null): value is CoachActionType =>
  actionTypes.some((type) => type === value);

const isPriority = (value: string | null): value is CoachActionPriority =>
  priorities.some((priority) => priority === value);

const CoachTasks: React.FC = () => {
  const { t } = useTranslation(['coach']);
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const typeParam = searchParams.get('type');
  const priorityParam = searchParams.get('priority');
  const type = isActionType(typeParam) ? typeParam : undefined;
  const priority = isPriority(priorityParam) ? priorityParam : undefined;
  const search = searchParams.get('search')?.trim() ?? '';
  const requestedPage = Number(searchParams.get('page'));
  const page = Number.isSafeInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1;
  const [searchInput, setSearchInput] = useState(search);

  useEffect(() => {
    setSearchInput(search);
  }, [search]);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      const nextSearch = searchInput.trim();
      if (nextSearch === search) return;
      const next = new URLSearchParams(searchParams);
      if (nextSearch) next.set('search', nextSearch);
      else next.delete('search');
      next.delete('page');
      setSearchParams(next, { replace: true });
    }, 300);
    return () => window.clearTimeout(timeout);
  }, [search, searchInput, searchParams, setSearchParams]);

  const { data, isLoading, isFetching, error } = useGetCoachActionItems({
    page,
    pageSize: PAGE_SIZE,
    type,
    priority,
    search: search || undefined,
  });

  useEffect(() => {
    if (!data || isLoading) return;
    const lastPage = Math.max(1, data.totalPages);
    if (page <= lastPage) return;
    const next = new URLSearchParams(searchParams);
    if (lastPage > 1) next.set('page', String(lastPage));
    else next.delete('page');
    setSearchParams(next, { replace: true });
  }, [data, isLoading, page, searchParams, setSearchParams]);

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

  const clearFilters = () => {
    setSearchInput('');
    setSearchParams({}, { replace: true });
  };

  const updateFilter = (key: 'type' | 'priority', value?: string) => {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value);
    else next.delete(key);
    next.delete('page');
    setSearchParams(next, { replace: true });
  };

  const items = data?.items ?? [];
  const hasFilters = Boolean(type || priority || search);

  return (
    <div className="coach-tasks animate-fade-in">
      <header className="coach-tasks__hero">
        <div className="coach-tasks__hero-copy">
          <span className="coach-tasks__hero-icon material-symbols-outlined" aria-hidden="true">task_alt</span>
          <div>
            <p className="coach-tasks__eyebrow">{t('coach:tasks.eyebrow')}</p>
            <h1>{t('coach:tasks.title')}</h1>
            <p className="coach-tasks__subtitle">{t('coach:tasks.subtitle')}</p>
          </div>
        </div>
        {!isLoading && (
          <div className="coach-tasks__total" aria-live="polite">
            <strong className="mono">{data?.totalCount ?? 0}</strong>
            <span>{t('coach:tasks.totalLabel')}</span>
          </div>
        )}
      </header>

      <section className="coach-tasks__controls" aria-label={t('coach:tasks.filtersLabel')}>
        <Input
          allowClear
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          placeholder={t('coach:tasks.searchPlaceholder')}
          prefix={<span className="material-symbols-outlined" aria-hidden="true">search</span>}
          className="coach-tasks__search"
          aria-label={t('coach:tasks.searchPlaceholder')}
        />
        <Select
          allowClear
          value={type}
          onChange={(value) => updateFilter('type', value)}
          placeholder={t('coach:tasks.typeFilter')}
          className="coach-tasks__select"
          options={actionTypes.map((value) => ({
            value,
            label: t(`coach:tasks.types.${value}`),
          }))}
          aria-label={t('coach:tasks.typeFilter')}
        />
        <Select
          allowClear
          value={priority}
          onChange={(value) => updateFilter('priority', value)}
          placeholder={t('coach:tasks.priorityFilter')}
          className="coach-tasks__select"
          options={priorities.map((value) => ({
            value,
            label: t(`coach:actionCenter.priorities.${value}`),
          }))}
          aria-label={t('coach:tasks.priorityFilter')}
        />
        {hasFilters && (
          <button type="button" className="coach-tasks__clear" onClick={clearFilters}>
            <span className="material-symbols-outlined" aria-hidden="true">filter_alt_off</span>
            {t('coach:tasks.clearFilters')}
          </button>
        )}
      </section>

      {error ? (
        <Alert type="error" showIcon message={t('coach:tasks.errorTitle')} description={t('coach:tasks.errorDescription')} />
      ) : isLoading ? (
        <div className="coach-tasks__loading">
          {[1, 2, 3, 4].map((item) => <Skeleton key={item} active avatar paragraph={{ rows: 2 }} />)}
        </div>
      ) : items.length === 0 ? (
        <div className="coach-tasks__empty">
          <Empty description={hasFilters ? t('coach:tasks.emptyFiltered') : t('coach:tasks.empty')} />
          {hasFilters && (
            <button type="button" className="coach-tasks__empty-action" onClick={clearFilters}>
              {t('coach:tasks.clearFilters')}
            </button>
          )}
        </div>
      ) : (
        <>
          <div className={`coach-tasks__grid ${isFetching ? 'coach-tasks__grid--refreshing' : ''}`}>
            {items.map((item) => (
              <article key={`${item.athleteId}-${item.type}`} className={`coach-task-card coach-task-card--${item.priority.toLowerCase()}`}>
                <div className="coach-task-card__topline">
                  <span className={`coach-task-card__type-icon coach-task-card__type-icon--${item.priority.toLowerCase()} material-symbols-outlined`} aria-hidden="true">
                    {actionIcons[item.type]}
                  </span>
                  <span className={`coach-task-card__priority coach-task-card__priority--${item.priority.toLowerCase()}`}>
                    {t(`coach:actionCenter.priorities.${item.priority}`)}
                  </span>
                </div>
                <div className="coach-task-card__athlete">
                  <Avatar src={item.athleteAvatarUrl ?? undefined} size={44}>
                    {item.athleteName.charAt(0)}
                  </Avatar>
                  <div>
                    <h2>{item.athleteName}</h2>
                    <span>{t(`coach:tasks.types.${item.type}`)}</span>
                  </div>
                </div>
                <p className="coach-task-card__description">{description(item)}</p>
                <button type="button" className="coach-task-card__action" onClick={() => openAction(item)}>
                  {t(`coach:actionCenter.actions.${item.type}`)}
                  <span className="material-symbols-outlined" aria-hidden="true">arrow_forward</span>
                </button>
              </article>
            ))}
          </div>
          {data && data.totalCount > PAGE_SIZE && (
            <div className="coach-tasks__pagination">
              <Pagination
                current={page}
                pageSize={PAGE_SIZE}
                total={data.totalCount}
                showSizeChanger={false}
                onChange={(nextPage) => {
                  const next = new URLSearchParams(searchParams);
                  if (nextPage > 1) next.set('page', String(nextPage));
                  else next.delete('page');
                  setSearchParams(next);
                }}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default CoachTasks;
