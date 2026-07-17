import { Alert, Button, Empty, Input, Modal, Skeleton, Tag } from 'antd';
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import ProgressPhotoViewer from '../ProgressPhotoViewer/ProgressPhotoViewer';
import {
  useAthleteOnboardingAssessment,
  useReopenOnboardingAssessment,
  useReviewOnboardingAssessment,
} from '../../hooks/useOnboarding/useOnboarding';
import './OnboardingAssessmentReview.scss';

const { TextArea } = Input;

interface Props { athleteId: number }

interface DetailItem { label: string; value: string | number | null | undefined }

const OnboardingAssessmentReview = ({ athleteId }: Props) => {
  const { t, i18n } = useTranslation(['coach', 'athlete', 'common']);
  const { data, isLoading, isError } = useAthleteOnboardingAssessment(athleteId);
  const review = useReviewOnboardingAssessment(athleteId);
  const reopen = useReopenOnboardingAssessment(athleteId);
  const [notes, setNotes] = useState('');
  const [reopenModalOpen, setReopenModalOpen] = useState(false);
  const [reopenReason, setReopenReason] = useState('');

  useEffect(() => setNotes(data?.coachReviewNotes ?? ''), [data?.coachReviewNotes]);

  if (isLoading) return <div className="onboarding-review"><Skeleton active paragraph={{ rows: 10 }} /></div>;
  if (isError) return <Alert type="error" showIcon message={t('coach:clientDetail.errorDesc')} />;
  if (!data || data.status === 'NotStarted') return <div className="onboarding-review"><Empty description={t('coach:onboarding.notStarted')} /></div>;
  if (data.status === 'Draft') return (
    <div className="onboarding-review">
      <Alert
        type="warning"
        showIcon
        message={t('coach:onboarding.draft')}
        description={data.reopenReason ? t('coach:onboarding.reopenedReason', { reason: data.reopenReason }) : undefined}
      />
    </div>
  );

  const translateOption = (group: string, value: string | null) => value
    ? t(`athlete:onboarding.options.${group}.${value}`, { defaultValue: value })
    : '—';
  const translateList = (group: string, values: string[]) => values.length
    ? values.map(value => translateOption(group, value)).join(' · ')
    : '—';
  const date = (value: string | null) => value
    ? new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium' }).format(new Date(value))
    : '—';

  const sections: Array<{ key: string; icon: string; items: DetailItem[] }> = [
    {
      key: 'baseline', icon: 'track_changes', items: [
        { label: t('athlete:onboarding.fields.primaryGoal'), value: translateOption('goals', data.primaryGoal) },
        { label: t('athlete:onboarding.fields.weightKg'), value: data.weightKg ? `${data.weightKg} kg` : null },
        { label: t('athlete:onboarding.fields.heightCm'), value: data.heightCm ? `${data.heightCm} cm` : null },
        { label: t('athlete:onboarding.fields.activityLevel'), value: translateOption('activity', data.activityLevel) },
      ],
    },
    {
      key: 'training', icon: 'fitness_center', items: [
        { label: t('athlete:onboarding.fields.trainingExperience'), value: translateOption('experience', data.trainingExperience) },
        { label: t('athlete:onboarding.fields.trainingDays'), value: data.trainingDaysPerWeek },
        { label: t('athlete:onboarding.fields.preferredDays'), value: translateList('days', data.preferredTrainingDays) },
        { label: t('athlete:onboarding.fields.equipment'), value: translateList('equipment', data.availableEquipment) },
      ],
    },
    {
      key: 'safety', icon: 'health_and_safety', items: [
        { label: t('athlete:onboarding.fields.injuries'), value: data.injuriesOrLimitations },
        { label: t('athlete:onboarding.fields.currentPain'), value: data.currentPain },
        { label: t('athlete:onboarding.fields.sleepHours'), value: data.averageSleepHours },
        { label: t('athlete:onboarding.fields.sleepQuality'), value: translateOption('sleep', data.sleepQuality) },
      ],
    },
    {
      key: 'nutrition', icon: 'restaurant', items: [
        { label: t('athlete:onboarding.fields.allergies'), value: data.foodAllergies },
        { label: t('athlete:onboarding.fields.intolerances'), value: data.foodIntolerances },
        { label: t('athlete:onboarding.fields.preferredFoods'), value: data.preferredFoods },
        { label: t('athlete:onboarding.fields.foodsToAvoid'), value: data.foodsToAvoid },
        { label: t('athlete:onboarding.fields.mealsPerDay'), value: data.typicalMealsPerDay },
        { label: t('athlete:onboarding.fields.mealSchedule'), value: data.typicalMealSchedule },
        { label: t('athlete:onboarding.fields.supplements'), value: data.currentSupplements },
        { label: t('athlete:onboarding.fields.notes'), value: data.additionalNotes },
      ],
    },
  ];

  const safetyFlags = [
    data.hasInjuryFlag && { icon: 'personal_injury', label: t('athlete:onboarding.fields.injuries'), value: data.injuriesOrLimitations },
    data.hasPainFlag && { icon: 'warning', label: t('athlete:onboarding.fields.currentPain'), value: data.currentPain },
    data.hasAllergyFlag && { icon: 'allergy', label: t('athlete:onboarding.fields.allergies'), value: data.foodAllergies },
    data.hasFoodRestrictionFlag && {
      icon: 'no_food',
      label: t('athlete:onboarding.fields.foodsToAvoid'),
      value: [data.foodIntolerances, data.foodsToAvoid].filter(Boolean).join(' · '),
    },
  ].filter(Boolean) as Array<{ icon: string; label: string; value: string | null }>;

  const isReviewed = data.status === 'Reviewed';
  const hasNotesChanged = notes.trim() !== (data.coachReviewNotes ?? '').trim();
  const isSubmitDisabled = isReviewed && !hasNotesChanged;

  return (
    <div className="onboarding-review animate-fade-in">
      <header className="onboarding-review__header">
        <div>
          <span className="onboarding-review__eyebrow">{t('coach:onboarding.tab')}</span>
          <h2>{t('coach:onboarding.title')}</h2>
          <p>{t('coach:onboarding.subtitle')}</p>
        </div>
        <div className="onboarding-review__meta">
          <Tag color={data.status === 'Reviewed' ? 'green' : 'blue'}>{t(`athlete:onboarding.status.${data.status}`)}</Tag>
          {data.submittedAt && <small>{t('coach:onboarding.submittedAt', { date: date(data.submittedAt) })}</small>}
          {data.reviewedAt && <small>{t('coach:onboarding.reviewedAt', { date: date(data.reviewedAt) })}</small>}
        </div>
      </header>

      <section className={`onboarding-review__flags ${safetyFlags.length ? 'onboarding-review__flags--active' : ''}`}>
        <h3><span className="material-symbols-outlined">shield</span>{t('coach:onboarding.safetyFlags')}</h3>
        {safetyFlags.length ? (
          <div className="onboarding-review__flag-grid">
            {safetyFlags.map(flag => (
              <div className="onboarding-review__flag" key={flag.label}>
                <span className="material-symbols-outlined">{flag.icon}</span>
                <div>
                  <strong>{flag.label}</strong>
                  <p>{flag.value}</p>
                </div>
              </div>
            ))}
          </div>
        ) : <p className="onboarding-review__clear"><span className="material-symbols-outlined">check_circle</span>{t('coach:onboarding.noSafetyFlags')}</p>}
      </section>

      <div className="onboarding-review__sections">
        {sections.map(section => (
          <section className="onboarding-review__section" key={section.key}>
            <h3><span className="material-symbols-outlined">{section.icon}</span>{t(`coach:onboarding.groups.${section.key}`)}</h3>
            <dl>{section.items.map(item => <div key={item.label}><dt>{item.label}</dt><dd>{item.value || '—'}</dd></div>)}</dl>
          </section>
        ))}
      </div>

      {/* Progress Photos Display */}
      {data.photos && data.photos.length > 0 && (
        <section className="onboarding-review__photos-section">
          <h3><span className="material-symbols-outlined">add_a_photo</span>{t('athlete:onboarding.fields.photos')}</h3>
          <ProgressPhotoViewer
            photos={data.photos.map((photo) => ({
              angle: photo.angle,
              url: photo.signedDownloadUrl
            }))}
            variant="grid"
          />
        </section>
      )}

      <section className="onboarding-review__decision">
        <div>
          <h3>{t('coach:onboarding.reviewNotes')}</h3>
          <p>{t('coach:onboarding.reviewPlaceholder')}</p>
        </div>
        <TextArea value={notes} onChange={event => setNotes(event.target.value)} rows={4} maxLength={3000} showCount />
        <div className="onboarding-review__decision-actions">
          <Button danger size="large" onClick={() => setReopenModalOpen(true)}>
            <span className="material-symbols-outlined">restart_alt</span>{t('coach:onboarding.reopenAction')}
          </Button>
          <Button
            type="primary"
            size="large"
            loading={review.isPending}
            disabled={isSubmitDisabled}
            onClick={() => review.mutate({ coachReviewNotes: notes.trim() || null })}
          >
            <span className="material-symbols-outlined">task_alt</span>{t('coach:onboarding.markReviewed')}
          </Button>
        </div>
      </section>

      <Modal
        open={reopenModalOpen}
        title={t('coach:onboarding.reopenTitle')}
        okText={t('coach:onboarding.confirmReopen')}
        cancelText={t('common:actions.cancel')}
        okButtonProps={{ danger: true, disabled: reopenReason.trim().length < 10, loading: reopen.isPending }}
        onCancel={() => { setReopenModalOpen(false); setReopenReason(''); }}
        onOk={() => reopen.mutate(
          { reason: reopenReason.trim() },
          { onSuccess: () => { setReopenModalOpen(false); setReopenReason(''); } },
        )}
      >
        <p>{t('coach:onboarding.reopenDescription')}</p>
        <TextArea
          value={reopenReason}
          onChange={(event) => setReopenReason(event.target.value)}
          placeholder={t('coach:onboarding.reopenPlaceholder')}
          rows={4}
          maxLength={1000}
          showCount
        />
        <small className="onboarding-review__reopen-help">{t('coach:onboarding.reopenMinimum')}</small>
      </Modal>
    </div>
  );
};

export default OnboardingAssessmentReview;
