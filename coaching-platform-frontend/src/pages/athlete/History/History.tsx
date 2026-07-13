import React, { useState } from 'react';
import { DatePicker, Button } from 'antd';
import dayjs, { Dayjs } from 'dayjs';
import { useTranslation } from 'react-i18next';
import { useQueryClient, useIsFetching } from '@tanstack/react-query';
import { useGetDashboard } from '../../../hooks/useAthlete/useAthlete';
import DailyLogHistoryView from '../../../components/DailyLogHistoryView/DailyLogHistoryView';
import './History.scss';

const History: React.FC = () => {
  const { t } = useTranslation(['athlete']);
  const [selectedDate, setSelectedDate] = useState<Dayjs>(dayjs());
  const { data: dashboardData, isLoading } = useGetDashboard();
  const queryClient = useQueryClient();

  const athleteId = dashboardData?.athlete?.id;
  const dateStr = selectedDate.format('YYYY-MM-DD');

  const isFetching = useIsFetching({ queryKey: ['daily-log-history', athleteId, dateStr] });
  const isRefreshing = isFetching > 0;

  const handleRefresh = () => {
    if (athleteId) {
      queryClient.invalidateQueries({ queryKey: ['daily-log-history', athleteId, dateStr] });
    }
  };

  return (
    <div id="athlete-history-page" className="athlete-history animate-fade-in">
      <div className="athlete-history__header">
        <div>
          <h1 className="athlete-history__title">{t('athlete:history.title')}</h1>
          <p className="athlete-history__sub">{t('athlete:history.subtitle')}</p>
        </div>

        <div className="athlete-history__date-picker-wrapper">
          <span className="label">{t('athlete:history.selectDate')}:</span>
          <DatePicker
            value={selectedDate}
            onChange={(date) => date && setSelectedDate(date)}
            allowClear={false}
            className="athlete-history__date-picker"
          />
          <Button
            type="text"
            icon={<span className="material-symbols-outlined">refresh</span>}
            onClick={handleRefresh}
            loading={isRefreshing}
            className="athlete-history__refresh-btn"
            style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
          />
        </div>
      </div>

      {isLoading ? (
        <div className="athlete-history__loader" style={{ textAlign: 'center', padding: '60px 0' }}>
          <div className="athlete-history__spinner" />
        </div>
      ) : athleteId ? (
        <DailyLogHistoryView athleteId={athleteId} date={dateStr} />
      ) : (
        <div className="athlete-history__error" style={{ textAlign: 'center', padding: '40px 0' }}>
          <p>{t('athlete:dashboard.errorMsg')}</p>
        </div>
      )}
    </div>
  );
};

export default History;
