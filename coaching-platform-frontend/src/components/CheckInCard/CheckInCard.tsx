import React, { useState } from 'react';
import { Divider, Progress, Input, Button } from 'antd';
import { useTranslation } from 'react-i18next';
import { useAddCoachNotes } from '../../hooks/useCheckIn/useCheckIn';
import { formatDateDisplay } from '../../utils/date';
import type { CheckInDto } from '../../types/CheckIn';
import ProgressPhotoViewer from '../ProgressPhotoViewer/ProgressPhotoViewer';
import './CheckInCard.scss';

const { TextArea } = Input;

interface CheckInCardProps {
  checkIn: CheckInDto;
  isCoach?: boolean;
  onPhotoClick?: (url: string) => void;
}

const getSubjectiveLabel = (label: string, t: any) => {
  switch (label) {
    case 'Sleep Quality': return t('athlete:checkIn.sleepLabel');
    case 'Energy Level': return t('athlete:checkIn.energyLabel');
    case 'Gut Health': return t('athlete:checkIn.gutLabel');
    case 'Training Stress': return t('athlete:checkIn.stressLabel');
    default: return label;
  }
};

const CheckInCard: React.FC<CheckInCardProps> = React.memo(({
  checkIn,
  isCoach = false,
  onPhotoClick,
}) => {
  const { t, i18n } = useTranslation(['common', 'athlete', 'coach']);
  const [notes, setNotes] = useState<string>(checkIn.coachNotes || '');
  const addCoachNotesMutation = useAddCoachNotes(checkIn.id);

  const handleSaveCheckInNotes = () => {
    if (!notes.trim()) return;
    addCoachNotesMutation.mutate({ notes: notes.trim() });
  };

  const getSliderTrackColor = (val: number) => {
    if (val <= 3) return 'var(--color-red)';
    if (val <= 6) return 'var(--color-gold)';
    return 'var(--color-success)';
  };

  return (
    <div className="checkin-card-item">
      <div className="checkin-card-item__header">
        <div className="checkin-card-item__week-title">
          {t('coach:clientDetail.weekOf', { date: formatDateDisplay(checkIn.weekOf) })}
        </div>
        <div className="checkin-card-item__submit-time mono">
          {t('coach:clientDetail.submittedAt', { date: new Date(checkIn.submittedAt).toLocaleString(i18n.language) })}
        </div>
      </div>

      <div className="checkin-card-item__body">
        {/* Measurements */}
        <div className="checkin-card-item__section">
          <h4 className="checkin-card-item__section-title">{t('coach:clientDetail.measurements')}</h4>
          <div className="checkin-card-item__biometrics-grid mono">
            <div className="checkin-card-item__metric">
              <span className="label">{t('common:labels.weight')}</span>
              <span className="value">{checkIn.weightKg} {t('common:units.kg')}</span>
            </div>
            <div className="checkin-card-item__metric">
              <span className="label">{t('common:labels.waist')}</span>
              <span className="value">{checkIn.waistCm ? `${checkIn.waistCm} ${t('common:units.cm')}` : '—'}</span>
            </div>
            <div className="checkin-card-item__metric">
              <span className="label">{t('common:labels.chest')}</span>
              <span className="value">{checkIn.chestCm ? `${checkIn.chestCm} ${t('common:units.cm')}` : '—'}</span>
            </div>
            <div className="checkin-card-item__metric">
              <span className="label">{t('common:labels.thigh')}</span>
              <span className="value">{checkIn.thighCm ? `${checkIn.thighCm} ${t('common:units.cm')}` : '—'}</span>
            </div>
          </div>
        </div>

        {/* Well-being sliders */}
        <div className="checkin-card-item__section">
          <h4 className="checkin-card-item__section-title">{t('coach:clientDetail.subjective')}</h4>
          <div className="checkin-card-item__subjective-list">
            {[
              { label: 'Sleep Quality', val: checkIn.sleepQuality },
              { label: 'Energy Level', val: checkIn.energyLevel },
              { label: 'Gut Health', val: checkIn.gutHealth },
              { label: 'Training Stress', val: checkIn.trainingStress },
            ].map((marker) => (
              <div className="checkin-card-item__subjective-row" key={marker.label}>
                <span className="label">{getSubjectiveLabel(marker.label, t)}</span>
                <div className="progress-container">
                  <Progress
                    percent={marker.val * 10}
                    strokeColor={getSliderTrackColor(marker.val)}
                    format={() => `${marker.val}/10`}
                    size="small"
                  />
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Photos */}
        <div className="checkin-card-item__section checkin-card-item__section--photos">
          <h4 className="checkin-card-item__section-title">{t('coach:clientDetail.photos')}</h4>
          <ProgressPhotoViewer
            photos={checkIn.photos?.map((photo: any) => ({
              angle: photo.angle,
              url: photo.signedDownloadUrl
            })) || []}
            variant="thumb"
          />
        </div>
      </div>

      <Divider style={{ margin: '16px 0' }} />

      {/* Coach Notes/Review */}
      <div className="checkin-card-item__feedback-section">
        <h4 className="checkin-card-item__section-title">{t('coach:clientDetail.reviewTitle')}</h4>
        {isCoach ? (
          <div className="checkin-card-item__notes-form">
            <TextArea
              rows={2}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder={t('coach:clientDetail.reviewPlaceholder')}
              maxLength={2000}
              disabled={addCoachNotesMutation.isPending}
            />
            <div className="checkin-card-item__notes-actions">
              {checkIn.coachReviewedAt && (
                <span className="reviewed-at-text mono">
                  {t('coach:clientDetail.reviewedAt', { date: new Date(checkIn.coachReviewedAt).toLocaleDateString(i18n.language) })}
                </span>
              )}
              <Button
                type="primary"
                size="small"
                onClick={handleSaveCheckInNotes}
                loading={addCoachNotesMutation.isPending}
                disabled={notes.trim() === (checkIn.coachNotes || '').trim() || !notes.trim()}
                className="save-checkin-notes-btn"
              >
                {t('coach:clientDetail.saveReview')}
              </Button>
            </div>
          </div>
        ) : (
          <div className="checkin-card-item__notes-readonly">
            {checkIn.coachNotes ? (
              <>
                <p className="feedback-text">{checkIn.coachNotes}</p>
                {checkIn.coachReviewedAt && (
                  <div className="reviewed-at-text mono">
                    {t('coach:clientDetail.reviewedAt', { date: new Date(checkIn.coachReviewedAt).toLocaleDateString(i18n.language) })}
                  </div>
                )}
              </>
            ) : (
              <p className="no-feedback text-secondary italic">
                {t('athlete:feedback.empty', 'No feedback notes or reviews from your coach yet.')}
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
});

export default CheckInCard;
