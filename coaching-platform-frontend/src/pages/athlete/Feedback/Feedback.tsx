import React, { useState } from 'react';
import { Card, Tag, Empty, Button, Skeleton, Result, Modal, Divider, Descriptions, Col, Row, Progress } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useGetFeedbackHistory } from '../../../hooks/useAthlete/useAthlete';
import { useGetCheckInHistory, useGetCheckInById } from '../../../hooks/useCheckIn/useCheckIn';
import { formatDateDisplay } from '../../../utils/date';
import ProgressPhotoViewer from '../../../components/ProgressPhotoViewer/ProgressPhotoViewer';
import './Feedback.scss';

const Feedback: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: feedbackList, isLoading, error, refetch } = useGetFeedbackHistory();

  // Modal and details state
  const [selectedCheckInId, setSelectedCheckInId] = useState<number | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { data: checkInDetails, isLoading: detailsLoading } = useGetCheckInById(
    selectedCheckInId!,
    isModalOpen && selectedCheckInId !== null
  );

  const getFeedbackIcon = (type?: string) => {
    switch (type) {
      case 'CheckIn':
        return 'assignment';
      case 'General':
      default:
        return 'chat';
    }
  };

  const getFeedbackTag = (type?: string) => {
    switch (type) {
      case 'CheckIn':
        return <Tag color="cyan" className="feedback-page__tag">{t('athlete:feedback.tags.checkIn', 'Weekly Check-In')}</Tag>;
      case 'General':
      default:
        return <Tag color="gold" className="feedback-page__tag">{t('athlete:feedback.tags.general', 'Coach Note')}</Tag>;
    }
  };

  const formatTimestamp = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    } catch {
      return dateStr;
    }
  };

  if (error) {
    return (
      <div className="feedback-page feedback-page--error animate-fade-in">
        <Result
          status="error"
          title={t('athlete:feedback.errorTitle', 'Failed to load feedback')}
          subTitle={t('athlete:feedback.errorDesc', 'Something went wrong while fetching your feedback history.')}
          extra={
            <Button type="primary" onClick={() => refetch()}>
              {t('common:actions.retry', 'Retry')}
            </Button>
          }
        />
      </div>
    );
  }

  return (
    <div id="feedback-history-page" className="feedback-page animate-fade-in">
      <div className="feedback-page__header">
        <div>
          <h1 className="feedback-page__title">{t('athlete:feedback.title', 'Coach Feedback & Reviews')}</h1>
          <p className="feedback-page__subtitle">
            {t('athlete:feedback.subtitle', 'Track your notes, instructions, and weekly check-in reviews.')}
          </p>
        </div>
        <Button 
          onClick={() => navigate('/athlete/dashboard')}
          icon={<span className="material-symbols-outlined" style={{ verticalAlign: 'middle', fontSize: '18px' }}>arrow_back</span>}
          className="feedback-page__back-btn"
        >
          {t('athlete:checkIn.backBtn', 'Back')}
        </Button>
      </div>

      <div className="feedback-page__content">
        {isLoading ? (
          <div className="feedback-page__loading">
            {[1, 2, 3].map((i) => (
              <Card key={i} className="feedback-page__card" style={{ marginBottom: 16 }}>
                <Skeleton active avatar paragraph={{ rows: 2 }} />
              </Card>
            ))}
          </div>
        ) : feedbackList && feedbackList.length > 0 ? (
          <div className="feedback-page__list">
            {feedbackList.map((item) => {
              const isCheckIn = item.type === 'CheckIn';
              return (
                <Card 
                  key={item.id} 
                  className={`feedback-page__card ${isCheckIn ? 'feedback-page__card--checkin' : 'feedback-page__card--general'}`}
                >
                  <div className="feedback-page__card-header">
                    <div className="feedback-page__coach-info">
                      <div className={`feedback-page__icon-badge ${isCheckIn ? 'feedback-page__icon-badge--checkin' : 'feedback-page__icon-badge--general'}`}>
                        <span className="material-symbols-outlined">{getFeedbackIcon(item.type)}</span>
                      </div>
                      <div>
                        <h3 className="feedback-page__coach-name">{item.coachName}</h3>
                        <span className="feedback-page__date mono">{formatTimestamp(item.createdAt)}</span>
                        {isCheckIn && item.weekOf && (
                          <span className="feedback-page__week-date mono">
                            <span className="material-symbols-outlined">event</span>
                            {t('athlete:checkIn.weekOf', 'Week of')}: {formatDateDisplay(item.weekOf)}
                          </span>
                        )}
                      </div>
                    </div>
                    <div className="feedback-page__actions-wrapper">
                      {getFeedbackTag(item.type)}
                      {isCheckIn && (
                        <Button 
                          type="dashed" 
                          size="small" 
                          icon={<span className="material-symbols-outlined" style={{ fontSize: '14px', verticalAlign: 'middle' }}>visibility</span>}
                          onClick={() => {
                            const checkInId = Math.abs(item.id);
                            setSelectedCheckInId(checkInId);
                            setIsModalOpen(true);
                          }}
                          className="feedback-page__view-submission-btn"
                        >
                          {t('athlete:feedback.viewSubmissionBtn', 'View Submission')}
                        </Button>
                      )}
                    </div>
                  </div>
                  <div className="feedback-page__card-body">
                    <p className="feedback-page__text">{item.noteText}</p>
                  </div>
                </Card>
              );
            })}
          </div>
        ) : (
          <Card className="feedback-page__empty-card">
            <Empty
              image={<span className="material-symbols-outlined feedback-page__empty-icon">forum</span>}
              imageStyle={{ height: 60 }}
              description={
                <span className="feedback-page__empty-text">
                  {t('athlete:feedback.empty', 'No feedback notes or reviews from your coach yet.')}
                </span>
              }
            />
          </Card>
        )}
      </div>

      <Modal
        title={
          <div className="feedback-page__modal-title">
            <span className="material-symbols-outlined text-gold">assignment_turned_in</span>
            {t('athlete:feedback.submissionDetails', 'Submission Details')} {checkInDetails?.weekOf ? `- ${t('athlete:checkIn.weekOf', 'Week of')} ${formatDateDisplay(checkInDetails.weekOf)}` : ''}
          </div>
        }
        open={isModalOpen}
        onCancel={() => {
          setIsModalOpen(false);
          setSelectedCheckInId(null);
        }}
        footer={[
          <Button key="close" type="primary" onClick={() => {
            setIsModalOpen(false);
            setSelectedCheckInId(null);
          }}>
            {t('common:actions.done', 'Close')}
          </Button>
        ]}
        width={720}
        className="feedback-page__modal"
      >
        {detailsLoading ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : checkInDetails ? (
          <div className="check-in-preview">
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={12}>
                <Card title={t('athlete:checkIn.biometricsTitle', 'Biometrics')} size="small" className="check-in-preview__section-card">
                  <Descriptions column={1} size="small" bordered={false}>
                    <Descriptions.Item label={t('athlete:checkIn.weight', 'Weight')}>{checkInDetails.weightKg} kg</Descriptions.Item>
                    <Descriptions.Item label={t('athlete:checkIn.waist', 'Waist')}>{checkInDetails.waistCm ? `${checkInDetails.waistCm} cm` : t('athlete:checkIn.notProvided', 'Not provided')}</Descriptions.Item>
                    <Descriptions.Item label={t('athlete:checkIn.chest', 'Chest')}>{checkInDetails.chestCm ? `${checkInDetails.chestCm} cm` : t('athlete:checkIn.notProvided', 'Not provided')}</Descriptions.Item>
                    <Descriptions.Item label={t('athlete:checkIn.thigh', 'Thigh')}>{checkInDetails.thighCm ? `${checkInDetails.thighCm} cm` : t('athlete:checkIn.notProvided', 'Not provided')}</Descriptions.Item>
                  </Descriptions>
                </Card>
              </Col>
              <Col xs={24} sm={12}>
                <Card title={t('athlete:checkIn.subjectiveTitle', 'Subjective Markers')} size="small" className="check-in-preview__section-card">
                  <div className="check-in-preview__subjectives">
                    <div className="check-in-preview__subjective-item">
                      <span className="check-in-preview__subjective-label">
                        <span className="material-symbols-outlined check-in-preview__sub-icon">bedtime</span>
                        {t('athlete:checkIn.sleepLabel', 'Sleep Quality')}
                      </span>
                      <Progress 
                        percent={checkInDetails.sleepQuality * 10} 
                        format={() => `${checkInDetails.sleepQuality}/10`} 
                        strokeColor="#52c41a"
                        status="normal"
                        className="check-in-preview__progress"
                      />
                    </div>
                    <div className="check-in-preview__subjective-item">
                      <span className="check-in-preview__subjective-label">
                        <span className="material-symbols-outlined check-in-preview__sub-icon">bolt</span>
                        {t('athlete:checkIn.energyLabel', 'Energy Level')}
                      </span>
                      <Progress 
                        percent={checkInDetails.energyLevel * 10} 
                        format={() => `${checkInDetails.energyLevel}/10`} 
                        strokeColor="#faad14"
                        status="normal"
                        className="check-in-preview__progress"
                      />
                    </div>
                    <div className="check-in-preview__subjective-item">
                      <span className="check-in-preview__subjective-label">
                        <span className="material-symbols-outlined check-in-preview__sub-icon">restaurant</span>
                        {t('athlete:checkIn.gutLabel', 'Gut Health')}
                      </span>
                      <Progress 
                        percent={checkInDetails.gutHealth * 10} 
                        format={() => `${checkInDetails.gutHealth}/10`} 
                        strokeColor="#13c2c2"
                        status="normal"
                        className="check-in-preview__progress"
                      />
                    </div>
                    <div className="check-in-preview__subjective-item">
                      <span className="check-in-preview__subjective-label">
                        <span className="material-symbols-outlined check-in-preview__sub-icon">fitness_center</span>
                        {t('athlete:checkIn.stressLabel', 'Training Stress')}
                      </span>
                      <Progress 
                        percent={checkInDetails.trainingStress * 10} 
                        format={() => `${checkInDetails.trainingStress}/10`} 
                        strokeColor="#ff4d4f"
                        status="normal"
                        className="check-in-preview__progress"
                      />
                    </div>
                  </div>
                </Card>
              </Col>
            </Row>

            <Divider style={{ margin: '16px 0' }} />

            <div className="check-in-preview__photos">
              <h3 className="check-in-preview__photos-title">
                <span className="material-symbols-outlined">photo_camera</span>
                {t('athlete:checkIn.photosSummary', 'Submitted Progress Photos')}
              </h3>
              <ProgressPhotoViewer
                photos={['Front', 'Side', 'Back'].map((angle) => ({
                  angle,
                  url: checkInDetails.photos?.find((p: any) => p.angle === angle)?.signedDownloadUrl
                }))}
                variant="grid"
              />
            </div>
          </div>
        ) : null}
      </Modal>
    </div>
  );
};

export default Feedback;
