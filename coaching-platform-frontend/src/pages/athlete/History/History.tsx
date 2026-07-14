import React, { useState } from 'react';
import { DatePicker, Button, Tabs, Pagination, Skeleton, Empty, Modal, Select } from 'antd';
import dayjs, { Dayjs } from 'dayjs';
import { useTranslation } from 'react-i18next';
import { useQueryClient, useIsFetching } from '@tanstack/react-query';
import { useGetDashboard } from '../../../hooks/useAthlete/useAthlete';
import { useGetCheckInHistory } from '../../../hooks/useCheckIn/useCheckIn';
import DailyLogHistoryView from '../../../components/DailyLogHistoryView/DailyLogHistoryView';
import CheckInCard from '../../../components/CheckInCard/CheckInCard';
import { formatDateDisplay } from '../../../utils/date';
import './History.scss';

const History: React.FC = () => {
  const { t } = useTranslation(['athlete', 'common', 'coach']);
  const [selectedDate, setSelectedDate] = useState<Dayjs>(dayjs());
  const [activeTab, setActiveTab] = useState<string>('daily-logs');
  const [selectedCheckInId, setSelectedCheckInId] = useState<number | null>(null);
  const [lightboxPhoto, setLightboxPhoto] = useState<string | null>(null);

  const { data: dashboardData, isLoading: isDashboardLoading } = useGetDashboard();
  const queryClient = useQueryClient();

  const athleteId = dashboardData?.athlete?.id;
  const dateStr = selectedDate.format('YYYY-MM-DD');

  // Daily log fetching state
  const isFetching = useIsFetching({ queryKey: ['daily-log-history', athleteId, dateStr] });
  const isRefreshing = isFetching > 0;

  // Check-in history query
  const { data: checkInHistory, isLoading: isCheckInHistoryLoading } = useGetCheckInHistory(1, 100);

  const selectedCheckIn = checkInHistory?.items.find(item => item.id === selectedCheckInId) || checkInHistory?.items[0];

  // Sync default selection to the most recent check-in
  React.useEffect(() => {
    if (checkInHistory?.items && checkInHistory.items.length > 0) {
      const exists = checkInHistory.items.some(item => item.id === selectedCheckInId);
      if (!selectedCheckInId || !exists) {
        setSelectedCheckInId(checkInHistory.items[0].id);
      }
    } else {
      setSelectedCheckInId(null);
    }
  }, [checkInHistory, selectedCheckInId]);

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
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={(key) => setActiveTab(key)}
        className="athlete-history__tabs"
        items={[
          {
            key: 'daily-logs',
            label: (
              <span className="athlete-history__tab-label">
                <span className="material-symbols-outlined icon">calendar_today</span>
                {t('athlete:history.dailyLogsTab', 'Daily Logs')}
              </span>
            ),
            children: (
              <div className="athlete-history__tab-content" style={{ marginTop: '16px' }}>
                <div className="athlete-history__date-picker-bar">
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

                {isDashboardLoading ? (
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
            )
          },
          {
            key: 'check-in-history',
            label: (
              <span className="athlete-history__tab-label">
                <span className="material-symbols-outlined icon">assignment_turned_in</span>
                {t('athlete:history.checkInHistoryTab', 'Weekly Check-ins')}
              </span>
            ),
            children: (
              <div className="athlete-history__tab-content" style={{ marginTop: '16px' }}>
                <div className="athlete-history__checkins-container">
                  {isCheckInHistoryLoading ? (
                    <div style={{ padding: '20px' }}>
                      <Skeleton active paragraph={{ rows: 6 }} />
                    </div>
                  ) : checkInHistory?.items && checkInHistory.items.length > 0 ? (
                    <div className="athlete-history__checkins-list" style={{ gap: '16px' }}>
                      <div className="athlete-history__date-picker-bar" style={{ marginBottom: '16px' }}>
                        <span className="label">{t('athlete:history.selectWeek', 'Select Week')}:</span>
                        <Select
                          value={selectedCheckInId}
                          onChange={(val) => setSelectedCheckInId(val)}
                          style={{ minWidth: 200 }}
                          options={checkInHistory.items.map(item => ({
                            value: item.id,
                            label: t('coach:clientDetail.weekOf', { date: formatDateDisplay(item.weekOf) })
                          }))}
                        />
                      </div>
                      {selectedCheckIn ? (
                        <CheckInCard
                          key={selectedCheckIn.id}
                          checkIn={selectedCheckIn}
                          isCoach={false}
                          onPhotoClick={(url) => setLightboxPhoto(url)}
                        />
                      ) : (
                        <Empty description={t('coach:clientDetail.noCheckins', 'No check-ins submitted yet.')} style={{ padding: '40px 0' }} />
                      )}
                    </div>
                  ) : (
                    <Empty description={t('coach:clientDetail.noCheckins', 'No check-ins submitted yet.')} style={{ padding: '40px 0' }} />
                  )}
                </div>
              </div>
            )
          }
        ]}
      />

      {/* Lightbox Modal */}
      <Modal
        open={!!lightboxPhoto}
        onCancel={() => setLightboxPhoto(null)}
        footer={null}
        width={800}
        centered
        styles={{ body: { padding: 0 } }}
      >
        {lightboxPhoto && (
          <img
            src={lightboxPhoto}
            alt="Enlarged progress preview"
            style={{ width: '100%', height: 'auto', display: 'block' }}
          />
        )}
      </Modal>
    </div>
  );
};

export default History;
