import React, { useState } from 'react';
import { Skeleton, Empty, Button } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  useGetCoachDashboard,
  useGetLiveFeed,
  useGetCompliance,
} from '../../../hooks/useCoachHub/useCoachHub';
import LiveFeedItem from '../../../components/LiveFeedItem/LiveFeedItem';
import ComplianceBar from '../../../components/ComplianceBar/ComplianceBar';
import CoachActionCenter from '../../../components/CoachActionCenter/CoachActionCenter';
import './CoachDashboard.scss';

const CoachDashboard: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [feedPage, setFeedPage] = useState<number>(1);
  const FEED_PAGE_SIZE = 5;

  const [compliancePage, setCompliancePage] = useState<number>(1);
  const COMPLIANCE_PAGE_SIZE = 5;

  // Fetch data using TanStack React Query
  const {
    data: dashboardData,
    isLoading: isDashboardLoading,
    error: dashboardError,
  } = useGetCoachDashboard();

  const {
    data: feedData,
    isLoading: isFeedLoading,
    error: feedError,
  } = useGetLiveFeed(feedPage, FEED_PAGE_SIZE);

  const {
    data: complianceData,
    isLoading: isComplianceLoading,
    error: complianceError,
  } = useGetCompliance();

  const handleRosterClick = () => {
    navigate('/coach/roster');
  };

  const handleNextPage = () => {
    if (feedData && feedData.hasNextPage) {
      setFeedPage((prev) => prev + 1);
    }
  };

  const handlePrevPage = () => {
    if (feedPage > 1) {
      setFeedPage((prev) => prev - 1);
    }
  };

  const hasErrors = dashboardError || feedError || complianceError;

  if (hasErrors) {
    return (
      <div className="coach-dashboard coach-dashboard--error">
        <span className="material-symbols-outlined">error_outline</span>
        <h2>{t('coach:dashboard.errorTitle')}</h2>
        <p>{t('coach:dashboard.errorDesc')}</p>
        <Button type="primary" onClick={() => window.location.reload()}>
          {t('coach:dashboard.refreshBtn')}
        </Button>
      </div>
    );
  }

  // Calculate SVG ring stroke properties
  const completionPercent = dashboardData?.avgWorkoutCompletionPercent ?? 0;
  const radius = 32;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (completionPercent / 100) * circumference;

  return (
    <div id="coach-dashboard-page" className="coach-dashboard animate-fade-in">
      {/* Header */}
      <div className="coach-dashboard__header">
        <div>
          <h1 className="coach-dashboard__title">{t('coach:dashboard.title')}</h1>
          <p className="coach-dashboard__subtitle">{t('coach:dashboard.subtitle')}</p>
        </div>
        <button className="coach-dashboard__action-btn" onClick={handleRosterClick}>
          <span className="material-symbols-outlined">group</span>
          {t('coach:dashboard.viewRosterBtn')}
        </button>
      </div>

      {/* KPI Cards */}
      <div className="coach-dashboard__kpis">
        {/* KPI 1: Active Athletes */}
        <div className="coach-dashboard__kpi-card">
          <div className="coach-dashboard__kpi-header">
            <div className="coach-dashboard__kpi-icon coach-dashboard__kpi-icon--navy">
              <span className="material-symbols-outlined">groups</span>
            </div>
          </div>
          <div className="coach-dashboard__kpi-body">
            {isDashboardLoading ? (
              <Skeleton.Input active size="small" style={{ width: 80 }} />
            ) : (
              <span className="coach-dashboard__kpi-value mono">
                {dashboardData?.activeAthleteCount ?? 0}
              </span>
            )}
            <span className="coach-dashboard__kpi-label">{t('coach:dashboard.activeAthletes')}</span>
          </div>
        </div>

        {/* KPI 2: Workout Completion Rate */}
        <div className="coach-dashboard__kpi-card">
          <div className="coach-dashboard__kpi-header">
            <div className="coach-dashboard__kpi-chart">
              <svg width="40" height="40" viewBox="0 0 80 80">
                <circle
                  cx="40"
                  cy="40"
                  r={radius}
                  fill="transparent"
                  stroke="var(--surface-container)"
                  strokeWidth="8"
                />
                <circle
                  cx="40"
                  cy="40"
                  r={radius}
                  fill="transparent"
                  stroke="var(--color-gold)"
                  strokeWidth="8"
                  strokeDasharray={circumference}
                  strokeDashoffset={isDashboardLoading ? circumference : strokeDashoffset}
                  strokeLinecap="round"
                  transform="rotate(-90 40 40)"
                  style={{ transition: 'stroke-dashoffset 0.5s ease' }}
                />
              </svg>
              <div className="coach-dashboard__kpi-chart-icon">
                <span className="material-symbols-outlined">fitness_center</span>
              </div>
            </div>
          </div>
          <div className="coach-dashboard__kpi-body">
            {isDashboardLoading ? (
              <Skeleton.Input active size="small" style={{ width: 80 }} />
            ) : (
              <span className="coach-dashboard__kpi-value mono">
                {Math.round(completionPercent)}%
              </span>
            )}
            <span className="coach-dashboard__kpi-label">{t('coach:dashboard.workoutCompletion')}</span>
            <span className="coach-dashboard__kpi-sub">{t('coach:dashboard.weeklyAvg')}</span>
          </div>
        </div>

        {/* KPI 3: Pending Check-ins */}
        <div
          className={`coach-dashboard__kpi-card coach-dashboard__kpi-card--clickable ${
            (dashboardData?.pendingCheckInsCount ?? 0) > 0 ? 'coach-dashboard__kpi-card--alert' : ''
          }`}
          onClick={() => navigate('/coach/roster?filter=NoRecentCheckIn')}
        >
          <div className="coach-dashboard__kpi-header">
            <div className={`coach-dashboard__kpi-icon ${
              (dashboardData?.pendingCheckInsCount ?? 0) > 0 
                ? 'coach-dashboard__kpi-icon--red' 
                : 'coach-dashboard__kpi-icon--navy'
            }`}>
              <span className="material-symbols-outlined">pending_actions</span>
            </div>
            {(dashboardData?.pendingCheckInsCount ?? 0) > 0 && (
              <span className="coach-dashboard__kpi-alert-badge">{t('coach:dashboard.actionRequired')}</span>
            )}
          </div>
          <div className="coach-dashboard__kpi-body">
            {isDashboardLoading ? (
              <Skeleton.Input active size="small" style={{ width: 80 }} />
            ) : (
              <span className="coach-dashboard__kpi-value mono">
                {dashboardData?.pendingCheckInsCount ?? 0}
              </span>
            )}
            <span className="coach-dashboard__kpi-label">{t('coach:dashboard.pendingCheckIns')}</span>
          </div>
        </div>

        {/* KPI 4: Assessments awaiting review */}
        <div
          className={`coach-dashboard__kpi-card coach-dashboard__kpi-card--clickable ${
            (dashboardData?.pendingOnboardingAssessmentsCount ?? 0) > 0
              ? 'coach-dashboard__kpi-card--attention'
              : ''
          }`}
          onClick={() => navigate('/coach/roster?filter=AwaitingAssessmentReview')}
        >
          <div className="coach-dashboard__kpi-header">
            <div className={`coach-dashboard__kpi-icon ${
              (dashboardData?.pendingOnboardingAssessmentsCount ?? 0) > 0
                ? 'coach-dashboard__kpi-icon--gold'
                : 'coach-dashboard__kpi-icon--navy'
            }`}>
              <span className="material-symbols-outlined">assignment_late</span>
            </div>
            {(dashboardData?.pendingOnboardingAssessmentsCount ?? 0) > 0 && (
              <span className="coach-dashboard__kpi-alert-badge coach-dashboard__kpi-alert-badge--gold">
                {t('coach:dashboard.reviewRequired')}
              </span>
            )}
          </div>
          <div className="coach-dashboard__kpi-body">
            {isDashboardLoading ? (
              <Skeleton.Input active size="small" style={{ width: 80 }} />
            ) : (
              <span className="coach-dashboard__kpi-value mono">
                {dashboardData?.pendingOnboardingAssessmentsCount ?? 0}
              </span>
            )}
            <span className="coach-dashboard__kpi-label">{t('coach:dashboard.pendingAssessments')}</span>
          </div>
        </div>

        {/* KPI 5: Athletes needing setup */}
        <div
          className={`coach-dashboard__kpi-card coach-dashboard__kpi-card--clickable ${
            (dashboardData?.athletesNeedingSetupCount ?? 0) > 0
              ? 'coach-dashboard__kpi-card--attention'
              : ''
          }`}
          onClick={() => navigate('/coach/roster?filter=SetupRequired')}
          role="button"
          tabIndex={0}
          onKeyDown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              event.preventDefault();
              navigate('/coach/roster?filter=SetupRequired');
            }
          }}
        >
          <div className="coach-dashboard__kpi-header">
            <div className={`coach-dashboard__kpi-icon ${
              (dashboardData?.athletesNeedingSetupCount ?? 0) > 0
                ? 'coach-dashboard__kpi-icon--gold'
                : 'coach-dashboard__kpi-icon--navy'
            }`}>
              <span className="material-symbols-outlined">rule</span>
            </div>
            {(dashboardData?.athletesNeedingSetupCount ?? 0) > 0 && (
              <span className="coach-dashboard__kpi-alert-badge coach-dashboard__kpi-alert-badge--gold">
                {t('coach:dashboard.setupRequired')}
              </span>
            )}
          </div>
          <div className="coach-dashboard__kpi-body">
            {isDashboardLoading ? (
              <Skeleton.Input active size="small" style={{ width: 80 }} />
            ) : (
              <span className="coach-dashboard__kpi-value mono">
                {dashboardData?.athletesNeedingSetupCount ?? 0}
              </span>
            )}
            <span className="coach-dashboard__kpi-label">{t('coach:dashboard.needingSetup')}</span>
          </div>
        </div>
      </div>

      <CoachActionCenter
        items={dashboardData?.actionItems ?? []}
        isLoading={isDashboardLoading}
        maxItems={4}
      />

      {/* Main Panels Grid */}
      <div className="coach-dashboard__grid">
        
        {/* Left Panel: Real-Time Live Feed */}
        <div className="coach-dashboard__panel coach-dashboard__panel--feed">
          <div className="coach-dashboard__panel-header">
            <div className="coach-dashboard__panel-title-wrapper">
              <span className="material-symbols-outlined">bolt</span>
              <h2>{t('coach:dashboard.liveFeed')}</h2>
            </div>
            <span className="coach-dashboard__pulse-indicator" title={t('coach:dashboard.monitoringLogs')}></span>
          </div>

          <div className="coach-dashboard__panel-body">
            {isFeedLoading && feedPage === 1 ? (
              <div className="coach-dashboard__loading-stack">
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} active avatar paragraph={{ rows: 2 }} />
                ))}
              </div>
            ) : feedData && feedData.items.length > 0 ? (
              <div className="coach-dashboard__feed-list">
                {feedData.items.map((item, idx) => (
                  <LiveFeedItem key={`${item.athleteId}-${item.date}-${idx}`} item={item} />
                ))}
              </div>
            ) : (
              <Empty description={t('coach:dashboard.noLogs')} style={{ padding: '40px 0' }} />
            )}
          </div>

          {/* Pagination controls for Feed */}
          {feedData && feedData.totalCount > FEED_PAGE_SIZE && (
            <div className="coach-dashboard__panel-footer">
              <button
                className="coach-dashboard__page-btn"
                onClick={handlePrevPage}
                disabled={feedPage === 1}
              >
                <span className="material-symbols-outlined">chevron_left</span>
                {t('coach:dashboard.prevBtn')}
              </button>
              <span className="coach-dashboard__page-num mono">
                {t('coach:dashboard.pageNum', { page: feedPage, total: feedData.totalPages })}
              </span>
              <button
                className="coach-dashboard__page-btn"
                onClick={handleNextPage}
                disabled={!feedData.hasNextPage}
              >
                {t('coach:dashboard.nextBtn')}
                <span className="material-symbols-outlined">chevron_right</span>
              </button>
            </div>
          )}
        </div>

        {/* Right Panel: Nutrition Compliance */}
        <div className="coach-dashboard__panel coach-dashboard__panel--compliance">
          <div className="coach-dashboard__panel-header">
            <div className="coach-dashboard__panel-title-wrapper">
              <span className="material-symbols-outlined">restaurant</span>
              <h2>{t('coach:dashboard.compliance')}</h2>
            </div>
          </div>

          <div className="coach-dashboard__panel-body">
            {isComplianceLoading ? (
              <div className="coach-dashboard__loading-stack">
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} active paragraph={{ rows: 1 }} title={true} />
                ))}
              </div>
            ) : complianceData && complianceData.length > 0 ? (
              <div className="coach-dashboard__compliance-list">
                {complianceData
                  .slice((compliancePage - 1) * COMPLIANCE_PAGE_SIZE, compliancePage * COMPLIANCE_PAGE_SIZE)
                  .map((item) => (
                    <ComplianceBar
                      key={item.athleteId}
                      athleteId={item.athleteId}
                      athleteName={item.athleteName}
                      consumed={item.consumedCalories}
                      target={item.targetCalories}
                      isOverTarget={item.isOverCalorieTarget}
                      compliancePercent={item.compliancePercent}
                    />
                  ))}
              </div>
            ) : (
              <Empty description={t('coach:dashboard.noCompliance')} style={{ padding: '40px 0' }} />
            )}
          </div>

          {/* Pagination controls for Compliance */}
          {complianceData && complianceData.length > COMPLIANCE_PAGE_SIZE && (
            <div className="coach-dashboard__panel-footer">
              <button
                className="coach-dashboard__page-btn"
                onClick={() => setCompliancePage((prev) => Math.max(prev - 1, 1))}
                disabled={compliancePage === 1}
              >
                <span className="material-symbols-outlined">chevron_left</span>
                {t('coach:dashboard.prevBtn')}
              </button>
              <span className="coach-dashboard__page-num mono">
                {t('coach:dashboard.pageNum', {
                  page: compliancePage,
                  total: Math.ceil(complianceData.length / COMPLIANCE_PAGE_SIZE),
                })}
              </span>
              <button
                className="coach-dashboard__page-btn"
                onClick={() =>
                  setCompliancePage((prev) =>
                    Math.min(prev + 1, Math.ceil(complianceData.length / COMPLIANCE_PAGE_SIZE))
                  )
                }
                disabled={compliancePage >= Math.ceil(complianceData.length / COMPLIANCE_PAGE_SIZE)}
              >
                {t('coach:dashboard.nextBtn')}
                <span className="material-symbols-outlined">chevron_right</span>
              </button>
            </div>
          )}
        </div>

      </div>
    </div>
  );
};

export default CoachDashboard;
