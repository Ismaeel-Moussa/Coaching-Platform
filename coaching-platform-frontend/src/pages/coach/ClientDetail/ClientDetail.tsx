import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Skeleton, Input, Button, Avatar, Card, Breadcrumb, Empty, Pagination, Modal, Tag, Progress, Divider, Tabs, DatePicker, Select } from 'antd';
import { useTranslation } from 'react-i18next';
import { useQueryClient, useIsFetching } from '@tanstack/react-query';
import dayjs from 'dayjs';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
} from 'recharts';
import {
  useGetAthleteProfile,
  useSaveFeedbackNote,
} from '../../../hooks/useCoachHub/useCoachHub';
import { useGetCheckInHistory } from '../../../hooks/useCheckIn/useCheckIn';
import { formatDateDisplay } from '../../../utils/date';
import type { CoachFeedbackNoteDto } from '../../../types/CoachHub';
import DailyLogHistoryView from '../../../components/DailyLogHistoryView/DailyLogHistoryView';
import CheckInCard from '../../../components/CheckInCard/CheckInCard';
import OnboardingAssessmentReview from '../../../components/OnboardingAssessmentReview/OnboardingAssessmentReview';
import './ClientDetail.scss';

const { TextArea } = Input;

const ClientDetail: React.FC = () => {
  const { t, i18n } = useTranslation(['common', 'athlete', 'coach']);
  const { athleteId } = useParams<{ athleteId: string }>();
  const navigate = useNavigate();
  const id = athleteId ? parseInt(athleteId, 10) : 0;

  const { data: profile, isLoading, error } = useGetAthleteProfile(id);
  const saveNoteMutation = useSaveFeedbackNote(id);

  const [activeTab, setActiveTab] = useState<string>('overview');
  const [selectedDate, setSelectedDate] = useState<dayjs.Dayjs>(dayjs());

  const queryClient = useQueryClient();
  const dateStr = selectedDate.format('YYYY-MM-DD');
  const isHistoryFetching = useIsFetching({ queryKey: ['daily-log-history', id, dateStr] }) > 0;

  const handleRefreshHistory = () => {
    queryClient.invalidateQueries({ queryKey: ['daily-log-history', id, dateStr] });
  };

  const [noteText, setNoteText] = useState<string>('');
  const [notesList, setNotesList] = useState<CoachFeedbackNoteDto[]>([]);

  // Check-In History Week Dropdown
  const [selectedCheckInId, setSelectedCheckInId] = useState<number | null>(null);
  const { data: checkInHistory, isLoading: isHistoryLoading } = useGetCheckInHistory(1, 100, id);

  const selectedCheckIn = checkInHistory?.items.find(item => item.id === selectedCheckInId) || checkInHistory?.items[0];

  // Sync default selection to the most recent check-in
  useEffect(() => {
    if (checkInHistory?.items && checkInHistory.items.length > 0) {
      const exists = checkInHistory.items.some(item => item.id === selectedCheckInId);
      if (!selectedCheckInId || !exists) {
        setSelectedCheckInId(checkInHistory.items[0].id);
      }
    } else {
      setSelectedCheckInId(null);
    }
  }, [checkInHistory, selectedCheckInId]);

  // Keep notes synchronized when profile loads
  useEffect(() => {
    if (profile?.feedbackNotes) {
      setNotesList(profile.feedbackNotes);
    }
  }, [profile]);

  // Handle tab routing via URL hash
  useEffect(() => {
    const hash = window.location.hash;
    if (hash === '#onboarding-assessment') {
      setActiveTab('onboarding-assessment');
    } else if (hash === '#check-in-history') {
      setActiveTab('check-in-history');
    }
  }, [window.location.hash]);

  // Hash scroll check for check-in history section
  useEffect(() => {
    if (window.location.hash === '#check-in-history' && !isHistoryLoading) {
      const timer = setTimeout(() => {
        const element = document.getElementById('check-in-history-section');
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
      }, 300);
      return () => clearTimeout(timer);
    }
  }, [window.location.hash, isHistoryLoading]);

  const handleSaveNote = () => {
    if (!noteText.trim()) return;
    saveNoteMutation.mutate(
      { noteText: noteText.trim() },
      {
        onSuccess: (newNote) => {
          setNotesList((prev) => [newNote, ...prev]);
          setNoteText('');
        },
      }
    );
  };

  const getInitials = (name: string) => {
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    }
    return parts[0] ? parts[0][0].toUpperCase() : '';
  };

  if (error) {
    return (
      <div className="client-detail client-detail--error">
        <span className="material-symbols-outlined">error_outline</span>
        <h2>{t('coach:clientDetail.errorTitle')}</h2>
        <p>{t('coach:clientDetail.errorDesc')}</p>
        <Button type="primary" onClick={() => navigate('/coach/roster')}>
          {t('coach:clientDetail.backRoster')}
        </Button>
      </div>
    );
  }

  return (
    <div id="client-detail-page" className="client-detail animate-fade-in">
      {/* Breadcrumb Navigation */}
      <div className="client-detail__breadcrumbs">
        <Breadcrumb
          items={[
            { title: <a onClick={() => navigate('/coach/roster')}>{t('coach:roster.title')}</a> },
            { title: isLoading ? t('common:status.loading') : profile?.fullName },
          ]}
        />
      </div>

      {isLoading ? (
        <div className="client-detail__loading">
          <Skeleton active avatar paragraph={{ rows: 4 }} />
        </div>
      ) : profile ? (
        <div className="client-detail__content">
          
          {/* Profile Header Card */}
          <div className="client-detail__card client-detail__card--header">
            <div className="client-detail__profile-info">
              {profile.avatarUrl ? (
                <Avatar src={profile.avatarUrl} size={80} />
              ) : (
                <Avatar size={80} className="client-detail__avatar-placeholder">
                  {getInitials(profile.fullName)}
                </Avatar>
              )}
              <div className="client-detail__profile-text">
                <h1 className="client-detail__name">{profile.fullName}</h1>
                <div className="client-detail__badges">
                  <span className="client-detail__badge-item">
                    <span className="material-symbols-outlined">track_changes</span>
                    {t('coach:clientDetail.goal', { goal: profile.targetGoal })}
                  </span>
                  <span className="client-detail__badge-item">
                    <span className="material-symbols-outlined">scale</span>
                    {t('coach:clientDetail.weight', { weight: profile.weightKg })}
                  </span>
                  <span className="client-detail__badge-item">
                    <span className="material-symbols-outlined">height</span>
                    {t('coach:clientDetail.height', { height: profile.heightCm })}
                  </span>
                </div>
              </div>
            </div>

            {/* Streak metrics */}
            <div className="client-detail__streak-metrics">
              <div className="client-detail__streak-metric">
                <span className="client-detail__streak-icon">🔥</span>
                <div>
                  <span className="client-detail__streak-value mono">{profile.currentStreak}</span>
                  <span className="client-detail__streak-label">{t('coach:clientDetail.currentStreak')}</span>
                </div>
              </div>
              <div className="client-detail__streak-divider" />
              <div className="client-detail__streak-metric">
                <span className="client-detail__streak-icon">🏆</span>
                <div>
                  <span className="client-detail__streak-value mono">{profile.longestStreak}</span>
                  <span className="client-detail__streak-label">{t('coach:clientDetail.longestStreak')}</span>
                </div>
              </div>
            </div>
          </div>

          <Tabs
            activeKey={activeTab}
            onChange={(key) => setActiveTab(key)}
            className="client-detail__tabs"
            items={[
              {
                key: 'overview',
                label: (
                  <span className="client-detail__tab-label">
                    <span className="material-symbols-outlined icon">dashboard</span>
                    {t('common:nav.dashboard')}
                  </span>
                ),
                children: (
                  <>
                    {/* Grid of Targets & Charts */}
                    <div className="client-detail__grid">
                      
                      {/* Left: Targets & Weight History */}
                      <div className="client-detail__left-col">
                        
                        {/* Targets Card */}
                        <div className="client-detail__card">
                          <div className="client-detail__card-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                              <span className="material-symbols-outlined text-gold">adjust</span>
                              <h3>{t('coach:clientDetail.assignedTargets')}</h3>
                            </div>
                            <Button
                              type="link"
                              icon={<span className="material-symbols-outlined" style={{ fontSize: '18px' }}>edit</span>}
                              onClick={() => navigate(`/coach/athlete-hub?athleteId=${id}`)}
                              className="client-detail__edit-targets-link"
                              style={{ padding: 0, height: 'auto', display: 'flex', alignItems: 'center', gap: '4px', color: 'var(--color-gold)' }}
                            >
                              {t('common:actions.edit')}
                            </Button>
                          </div>
                          {profile.currentTargets ? (
                            <div className="client-detail__targets-grid">
                              <div className="client-detail__target-item">
                                <span className="client-detail__target-label">{t('common:labels.calories')}</span>
                                <span className="client-detail__target-val mono">
                                  {Math.round(profile.currentTargets.targetCalories)} <span className="unit">{t('common:units.kcal')}</span>
                                </span>
                              </div>
                              <div className="client-detail__target-item">
                                <span className="client-detail__target-label">{t('common:labels.protein')}</span>
                                <span className="client-detail__target-val mono">
                                  {Math.round(profile.currentTargets.targetProtein)} <span className="unit">{t('common:units.grams')}</span>
                                </span>
                              </div>
                              <div className="client-detail__target-item">
                                <span className="client-detail__target-label">{t('common:labels.carbs')}</span>
                                <span className="client-detail__target-val mono">
                                  {Math.round(profile.currentTargets.targetCarbs)} <span className="unit">{t('common:units.grams')}</span>
                                </span>
                              </div>
                              <div className="client-detail__target-item">
                                <span className="client-detail__target-label">{t('common:labels.fat')}</span>
                                <span className="client-detail__target-val mono">
                                  {Math.round(profile.currentTargets.targetFat)} <span className="unit">{t('common:units.grams')}</span>
                                </span>
                              </div>
                              <div className="client-detail__target-item">
                                <span className="client-detail__target-label">{t('athlete:dashboard.targets.hydration')}</span>
                                <span className="client-detail__target-val mono">
                                  {profile.currentTargets.waterLitersTarget} <span className="unit">{t('common:units.liters')}</span>
                                </span>
                              </div>
                              <div className="client-detail__target-item">
                                <span className="client-detail__target-label">{t('athlete:dashboard.targets.steps')}</span>
                                <span className="client-detail__target-val mono">
                                  {profile.currentTargets.stepsTarget.toLocaleString()} <span className="unit">{t('common:units.steps')}</span>
                                </span>
                              </div>
                            </div>
                          ) : (
                            <Empty description={t('coach:clientDetail.noTargets')} style={{ padding: '20px 0' }} />
                          )}
                        </div>

                        
                      </div>

                      {/* Right: Weight Chart & Feedback Notes */}
                      <div className="client-detail__right-col">
                        
                        {/* Weight Trend Chart */}
                        <div className="client-detail__card">
                          <div className="client-detail__card-header">
                            <span className="material-symbols-outlined text-gold">show_chart</span>
                            <h3>{t('coach:clientDetail.weightChart')}</h3>
                          </div>
                          <div className="client-detail__chart-container">
                            {profile.weightHistory && profile.weightHistory.length > 0 ? (
                              <ResponsiveContainer width="100%" height={300}>
                                <LineChart
                                  data={profile.weightHistory}
                                  margin={{ top: 15, right: 30, left: -10, bottom: 5 }}
                                >
                                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--color-border-light)" />
                                  <XAxis
                                    dataKey="weekOf"
                                    tickFormatter={(v) => formatDateDisplay(v)}
                                    stroke="var(--color-text-secondary)"
                                    style={{ fontFamily: 'var(--font-data)', fontSize: 11 }}
                                  />
                                  <YAxis
                                    stroke="var(--color-text-secondary)"
                                    style={{ fontFamily: 'var(--font-data)', fontSize: 11 }}
                                    unit={` ${t('common:units.kg')}`}
                                    domain={['auto', 'auto']}
                                  />
                                  <Tooltip
                                    labelFormatter={(label) => `${t('coach:clientDetail.weekOf', { date: formatDateDisplay(label) })}`}
                                    formatter={(value) => [`${value} ${t('common:units.kg')}`, t('common:labels.weight')]}
                                    contentStyle={{
                                      backgroundColor: 'var(--color-white)',
                                      borderRadius: 'var(--radius-card)',
                                      border: '1px solid var(--color-border-light)',
                                      boxShadow: 'var(--shadow-md)',
                                      fontFamily: 'var(--font-body)',
                                      fontSize: '13px',
                                    }}
                                  />
                                  <Line
                                    type="monotone"
                                    dataKey="weightKg"
                                    stroke="var(--color-gold)"
                                    strokeWidth={3}
                                    activeDot={{ r: 6 }}
                                    dot={{ stroke: 'var(--color-navy)', strokeWidth: 2, r: 4, fill: 'var(--color-gold)' }}
                                  />
                                </LineChart>
                              </ResponsiveContainer>
                            ) : (
                              <Empty description={t('coach:clientDetail.noWeightData')} style={{ padding: '40px 0' }} />
                            )}
                          </div>
                        </div>
                      </div>

                    </div>

                    {/* Feedback Notes (Full Width) */}
                    <div className="client-detail__card client-detail__card--notes" style={{ marginTop: '24px' }}>
                      <div className="client-detail__card-header">
                        <span className="material-symbols-outlined text-gold">feedback</span>
                        <h3>{t('coach:clientDetail.feedbackTitle')}</h3>
                      </div>

                      {/* Add Note Input */}
                      <div className="client-detail__add-note">
                        <TextArea
                          placeholder={t('coach:clientDetail.notePlaceholder')}
                          rows={4}
                          value={noteText}
                          onChange={(e) => setNoteText(e.target.value)}
                          maxLength={2000}
                          disabled={saveNoteMutation.isPending}
                        />
                        <div className="client-detail__note-actions">
                          <span className="client-detail__note-count">
                            {t('coach:clientDetail.charCount', { count: noteText.length })}
                          </span>
                          <Button
                            type="primary"
                            onClick={handleSaveNote}
                            loading={saveNoteMutation.isPending}
                            disabled={!noteText.trim()}
                            className="client-detail__submit-note-btn"
                          >
                            {t('coach:clientDetail.saveNote')}
                          </Button>
                        </div>
                      </div>

                      {/* Notes List */}
                      <div className="client-detail__notes-list">
                        {notesList.length > 0 ? (
                          notesList.map((note) => (
                            <div className="client-detail__note-item" key={note.id}>
                              <div className="client-detail__note-header">
                                <span className="client-detail__note-author">{t('coach:templateBuilder.byCoach', { name: note.coachName })}</span>
                                <span className="client-detail__note-date mono">
                                  {formatDateDisplay(note.createdAt.substring(0, 10))}
                                </span>
                              </div>
                              <p className="client-detail__note-text">{note.noteText}</p>
                            </div>
                          ))
                        ) : (
                          <div className="client-detail__no-notes">
                            <span className="material-symbols-outlined">forum</span>
                            <p>{t('coach:clientDetail.noNotes')}</p>
                          </div>
                        )}
                      </div>
                    </div>
                  </>
                )
              },
              {
                key: 'onboarding-assessment',
                label: (
                  <span className="client-detail__tab-label">
                    <span className="material-symbols-outlined icon">assignment</span>
                    {t('coach:onboarding.tab')}
                  </span>
                ),
                children: (
                  <div style={{ marginTop: '16px' }}>
                    <OnboardingAssessmentReview athleteId={id} />
                  </div>
                )
              },
              {
                key: 'check-in-history',
                label: (
                  <span className="client-detail__tab-label">
                    <span className="material-symbols-outlined icon">assignment_turned_in</span>
                    {t('coach:clientDetail.checkinHistory')}
                  </span>
                ),
                children: (
                  <div className="client-detail__history-section" style={{ marginTop: '16px' }}>
                    <div className="client-detail__card">
                      {isHistoryLoading ? (
                        <div style={{ padding: '20px' }}>
                          <Skeleton active paragraph={{ rows: 6 }} />
                        </div>
                      ) : checkInHistory?.items && checkInHistory.items.length > 0 ? (
                        <div className="client-detail__history-list" style={{ gap: '16px' }}>
                          <div className="client-detail__history-tab-header" style={{ display: 'flex', alignItems: 'center', gap: '12px', background: 'var(--color-white)', border: '1px solid var(--color-border-light)', padding: '8px 16px', borderRadius: 'var(--radius-md)', width: 'fit-content', boxShadow: 'var(--shadow-sm)', marginBottom: '16px' }}>
                            <span className="label" style={{ fontSize: '13px', fontWeight: 500, color: 'var(--color-text-primary)' }}>{t('athlete:history.selectWeek', 'Select Week')}:</span>
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
                              isCoach={true}
                            />
                          ) : (
                            <Empty description={t('coach:clientDetail.noCheckins')} style={{ padding: '40px 0' }} />
                          )}
                        </div>
                      ) : (
                        <Empty description={t('coach:clientDetail.noCheckins')} style={{ padding: '40px 0' }} />
                      )}
                    </div>
                  </div>
                )
              },
              {
                key: 'history',
                label: (
                  <span className="client-detail__tab-label">
                    <span className="material-symbols-outlined icon">history</span>
                    {t('coach:clientDetail.dailyHistoryTab')}
                  </span>
                ),
                children: (
                  <div className="client-detail__history-tab-content" style={{ marginTop: '16px' }}>
                    <div className="client-detail__history-tab-header" style={{ display: 'flex', alignItems: 'center', gap: '12px', background: 'var(--color-white)', border: '1px solid var(--color-border-light)', padding: '8px 16px', borderRadius: 'var(--radius-md)', width: 'fit-content', marginBottom: '24px', boxShadow: 'var(--shadow-sm)' }}>
                      <span className="label" style={{ fontSize: '13px', fontWeight: 500, color: 'var(--color-text-primary)' }}>{t('athlete:history.selectDate')}:</span>
                      <DatePicker
                        value={selectedDate}
                        onChange={(date) => date && setSelectedDate(date)}
                        allowClear={false}
                        className="client-detail__history-datepicker"
                        style={{ borderRadius: 'var(--radius-sm)', borderColor: 'var(--color-border-light)', fontFamily: 'var(--font-data)' }}
                      />
                      <Button
                        type="text"
                        icon={<span className="material-symbols-outlined">refresh</span>}
                        onClick={handleRefreshHistory}
                        loading={isHistoryFetching}
                        className="client-detail__history-refresh-btn"
                        style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                      />
                    </div>
                    <DailyLogHistoryView athleteId={id} date={selectedDate.format('YYYY-MM-DD')} />
                  </div>
                )
              }
            ]}
          />
        </div>
      ) : null}

    </div>
  );
};

export default ClientDetail;
