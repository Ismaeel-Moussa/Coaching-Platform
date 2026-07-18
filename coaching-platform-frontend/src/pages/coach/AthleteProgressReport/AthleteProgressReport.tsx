import { useMemo, useState } from 'react';
import {
  Alert,
  Avatar,
  Breadcrumb,
  Button,
  Empty,
  Progress,
  Segmented,
  Skeleton,
  Switch,
  message,
} from 'antd';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate, useParams } from 'react-router-dom';
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import {
  useDownloadAthleteProgressReport,
  useGetAthleteProgressReport,
} from '../../../hooks/useCoachHub/useCoachHub';
import type { ProgressReportOptions } from '../../../types/CoachHub';
import './AthleteProgressReport.scss';

const AthleteProgressReport = () => {
  const { t, i18n } = useTranslation(['coach', 'common']);
  const navigate = useNavigate();
  const { athleteId } = useParams();
  const id = Number(athleteId);
  const [options, setOptions] = useState<ProgressReportOptions>({
    weeks: 8,
    includeCoachNotes: false,
    includePhotos: false,
    language: i18n.language.startsWith('ar') ? 'ar' : 'en',
  });
  const previewOptions = useMemo(
    () => ({ weeks: options.weeks, includeCoachNotes: options.includeCoachNotes }),
    [options.weeks, options.includeCoachNotes],
  );
  const { data: report, isLoading, isFetching, error } = useGetAthleteProgressReport(id, previewOptions);
  const downloadReport = useDownloadAthleteProgressReport(id);

  const locale = i18n.language.startsWith('ar') ? 'ar' : 'en';
  const dateFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'short', year: 'numeric' }),
    [locale],
  );
  const formatDate = (value: string) => dateFormatter.format(new Date(`${value}T00:00:00`));
  const formatPercent = (value: number | null) => value == null ? '—' : `${Math.round(value)}%`;
  const initials = (name: string) => name.split(/\s+/).slice(0, 2).map(part => part[0]).join('').toUpperCase();

  const handleDownload = async () => {
    if (!report) return;
    try {
      const blob = await downloadReport.mutateAsync(options);
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${report.athleteName.replace(/\s+/g, '-').toLowerCase()}-progress-report-${report.periodEnd}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.setTimeout(() => URL.revokeObjectURL(url), 1_000);
      message.success(t('coach:progressReport.downloadSuccess'));
    } catch {
      message.error(t('coach:progressReport.downloadError'));
    }
  };

  if (!Number.isInteger(id) || id <= 0 || error) {
    return (
      <div className="progress-report progress-report--error">
        <Alert
          type="error"
          showIcon
          message={t('coach:progressReport.errorTitle')}
          description={t('coach:progressReport.errorDescription')}
          action={<Button onClick={() => navigate('/coach/roster')}>{t('coach:clientDetail.backRoster')}</Button>}
        />
      </div>
    );
  }

  return (
    <div className="progress-report animate-fade-in">
      <Breadcrumb
        items={[
          { title: <Link to="/coach/roster">{t('coach:roster.title')}</Link> },
          { title: <Link to={`/coach/roster/${id}`}>{report?.athleteName ?? t('common:status.loading')}</Link> },
          { title: t('coach:progressReport.title') },
        ]}
      />

      <section className="progress-report__hero">
        <div className="progress-report__identity">
          {report?.avatarUrl ? (
            <Avatar size={68} src={report.avatarUrl} />
          ) : (
            <Avatar size={68}>{report ? initials(report.athleteName) : ''}</Avatar>
          )}
          <div>
            <span className="progress-report__eyebrow">{t('coach:progressReport.eyebrow')}</span>
            <h1>{report?.athleteName ?? t('coach:progressReport.title')}</h1>
            {report && (
              <p>{t('coach:progressReport.period', {
                start: formatDate(report.periodStart),
                end: formatDate(report.periodEnd),
              })}</p>
            )}
          </div>
        </div>
        <Button
          type="primary"
          size="large"
          icon={<span className="material-symbols-outlined">download</span>}
          loading={downloadReport.isPending}
          disabled={!report}
          onClick={handleDownload}
        >
          {t('coach:progressReport.downloadPdf')}
        </Button>
      </section>

      <section className="progress-report__controls" aria-label={t('coach:progressReport.reportOptions')}>
        <div className="progress-report__range">
          <div>
            <strong>{t('coach:progressReport.range')}</strong>
            <span>{t('coach:progressReport.rangeHint')}</span>
          </div>
          <Segmented
            value={options.weeks}
            options={([4, 8, 12] as const).map(weeks => ({
              value: weeks,
              label: t('coach:progressReport.weeks', { count: weeks }),
            }))}
            onChange={(weeks) => setOptions(current => ({ ...current, weeks: weeks as 4 | 8 | 12 }))}
          />
        </div>
        <div className="progress-report__privacy-options">
          <label>
            <Switch
              checked={options.includeCoachNotes}
              onChange={(includeCoachNotes) => setOptions(current => ({ ...current, includeCoachNotes }))}
            />
            <span>
              <strong>{t('coach:progressReport.includeNotes')}</strong>
              <small>{t('coach:progressReport.includeNotesHint')}</small>
            </span>
          </label>
          <label>
            <Switch
              checked={options.includePhotos}
              onChange={(includePhotos) => setOptions(current => ({ ...current, includePhotos }))}
            />
            <span>
              <strong>{t('coach:progressReport.includePhotos')}</strong>
              <small>{t('coach:progressReport.includePhotosHint')}</small>
            </span>
          </label>
          <label className="progress-report__language-option">
            <span>
              <strong>{t('coach:progressReport.pdfLanguage')}</strong>
              <small>{t('coach:progressReport.pdfLanguageHint')}</small>
            </span>
            <Segmented
              value={options.language}
              options={[
                { value: 'ar', label: 'العربية' },
                { value: 'en', label: 'English' },
              ]}
              onChange={(language) => setOptions(current => ({ ...current, language: language as 'ar' | 'en' }))}
            />
          </label>
        </div>
        <div className="progress-report__privacy-note">
          <span className="material-symbols-outlined">shield_lock</span>
          <span>{t('coach:progressReport.privacyNotice')}</span>
        </div>
      </section>

      {isLoading || !report ? (
        <div className="progress-report__loading"><Skeleton active paragraph={{ rows: 12 }} /></div>
      ) : (
        <div className={`progress-report__body${isFetching ? ' progress-report__body--refreshing' : ''}`}>
          <section className="progress-report__summary-grid">
            <SummaryCard icon="monitor_weight" label={t('coach:progressReport.weightChange')} value={report.summary.weightChangeKg == null ? '—' : `${report.summary.weightChangeKg > 0 ? '+' : ''}${report.summary.weightChangeKg} kg`} />
            <SummaryCard icon="exercise" label={t('coach:progressReport.loggedSessions')} value={`${report.summary.completedWorkouts}/${report.summary.loggedWorkouts}`} />
            <SummaryCard icon="nutrition" label={t('coach:progressReport.calorieAdherence')} value={formatPercent(report.summary.averageCalorieAdherencePercent)} />
            <SummaryCard icon="fact_check" label={t('coach:progressReport.checkIns')} value={String(report.summary.checkInCount)} />
          </section>

          <section className="progress-report__panel progress-report__chart-panel">
            <SectionHeading icon="show_chart" title={t('coach:progressReport.weightTrend')} description={t('coach:progressReport.weightTrendHint')} />
            {report.weeklyProgress.some(point => point.weightKg != null) ? (
              <div className="progress-report__chart" dir="ltr">
                <ResponsiveContainer width="100%" height="100%" minWidth={0} minHeight={240}>
                  <LineChart data={report.weeklyProgress} margin={{ top: 10, right: 12, left: 0, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="4 4" stroke="#e4e7ef" />
                    <XAxis dataKey="weekOf" tickFormatter={(value) => formatDate(value)} tick={{ fontSize: 11 }} />
                    <YAxis domain={['dataMin - 2', 'dataMax + 2']} tick={{ fontSize: 11 }} unit=" kg" />
                    <Tooltip labelFormatter={(value) => formatDate(String(value))} formatter={(value) => [`${value} kg`, t('coach:progressReport.weight')]} />
                    <Line type="monotone" dataKey="weightKg" stroke="#172554" strokeWidth={3} dot={{ r: 4, fill: '#fdc003', stroke: '#172554' }} connectNulls />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            ) : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t('coach:progressReport.noWeightData')} />}
          </section>

          <section className="progress-report__panel">
            <SectionHeading icon="calendar_month" title={t('coach:progressReport.weeklyProgress')} description={t('coach:progressReport.weeklyProgressHint')} />
            <div className="progress-report__weeks">
              {[...report.weeklyProgress].reverse().map(week => (
                <article className="progress-report__week" key={week.weekOf}>
                  <header>
                    <strong>{formatDate(week.weekOf)}</strong>
                    <span className={week.checkInSubmitted ? 'is-complete' : ''}>
                      <span className="material-symbols-outlined">{week.checkInSubmitted ? 'check_circle' : 'radio_button_unchecked'}</span>
                      {t(`coach:progressReport.${week.checkInSubmitted ? 'checkInDone' : 'checkInMissing'}`)}
                    </span>
                  </header>
                  <div className="progress-report__week-weight">
                    <span>{t('coach:progressReport.weight')}</span>
                    <strong>{week.weightKg == null ? '—' : `${week.weightKg} kg`}</strong>
                  </div>
                  <MetricProgress label={t('coach:progressReport.calories')} value={week.calorieAdherencePercent} />
                  <MetricProgress label={t('coach:progressReport.protein')} value={week.proteinAdherencePercent} />
                  <MetricProgress label={t('coach:progressReport.steps')} value={week.stepsAdherencePercent} />
                  <div className="progress-report__week-workouts">
                    <span>{t('coach:progressReport.loggedSessions')}</span>
                    <strong>{week.completedWorkouts}/{week.loggedWorkouts}</strong>
                  </div>
                </article>
              ))}
            </div>
          </section>

          <section className="progress-report__panel">
            <SectionHeading icon="assignment_turned_in" title={t('coach:progressReport.checkInHistory')} description={t('coach:progressReport.checkInHistoryHint')} />
            {report.checkIns.length === 0 ? (
              <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t('coach:progressReport.noCheckIns')} />
            ) : (
              <div className="progress-report__checkins">
                {report.checkIns.map(checkIn => (
                  <article key={checkIn.id}>
                    <div>
                      <strong>{formatDate(checkIn.weekOf)}</strong>
                      <span>{checkIn.weightKg} kg</span>
                    </div>
                    <dl>
                      <div><dt>{t('coach:progressReport.waist')}</dt><dd>{checkIn.waistCm == null ? '—' : `${checkIn.waistCm} cm`}</dd></div>
                      <div><dt>{t('coach:progressReport.chest')}</dt><dd>{checkIn.chestCm == null ? '—' : `${checkIn.chestCm} cm`}</dd></div>
                      <div><dt>{t('coach:progressReport.thigh')}</dt><dd>{checkIn.thighCm == null ? '—' : `${checkIn.thighCm} cm`}</dd></div>
                      <div><dt>{t('coach:progressReport.energy')}</dt><dd>{checkIn.energyLevel}/10</dd></div>
                      <div><dt>{t('coach:progressReport.sleep')}</dt><dd>{checkIn.sleepQuality}/10</dd></div>
                      <div><dt>{t('coach:progressReport.gut')}</dt><dd>{checkIn.gutHealth}/10</dd></div>
                      <div><dt>{t('coach:progressReport.stress')}</dt><dd>{checkIn.trainingStress}/10</dd></div>
                    </dl>
                    {checkIn.reviewNotes && <p>{checkIn.reviewNotes}</p>}
                  </article>
                ))}
              </div>
            )}
          </section>

          {options.includeCoachNotes && (
            <section className="progress-report__panel">
              <SectionHeading icon="clinical_notes" title={t('coach:progressReport.coachNotes')} description={t('coach:progressReport.coachNotesHint')} />
              {report.coachNotes.length === 0 ? <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t('coach:progressReport.noCoachNotes')} /> : (
                <div className="progress-report__notes">
                  {report.coachNotes.map(note => (
                    <blockquote key={note.id}><p>{note.text}</p><footer>{note.coachName} · {formatDate(note.createdAt.slice(0, 10))}</footer></blockquote>
                  ))}
                </div>
              )}
            </section>
          )}

        </div>
      )}
    </div>
  );
};

const SummaryCard = ({ icon, label, value }: { icon: string; label: string; value: string }) => (
  <article className="progress-report__summary-card">
    <span className="material-symbols-outlined">{icon}</span>
    <div><strong>{value}</strong><small>{label}</small></div>
  </article>
);

const SectionHeading = ({ icon, title, description }: { icon: string; title: string; description: string }) => (
  <header className="progress-report__section-heading">
    <span className="material-symbols-outlined">{icon}</span>
    <div><h2>{title}</h2><p>{description}</p></div>
  </header>
);

const MetricProgress = ({ label, value }: { label: string; value: number | null }) => (
  <div className="progress-report__metric-progress">
    <div><span>{label}</span><strong>{value == null ? '—' : `${Math.round(value)}%`}</strong></div>
    <Progress percent={value == null ? 0 : Math.round(value)} showInfo={false} strokeColor="#fdc003" trailColor="#edf0f6" />
  </div>
);

export default AthleteProgressReport;
