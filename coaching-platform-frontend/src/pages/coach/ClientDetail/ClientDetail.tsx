import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Skeleton, Input, Button, Avatar, Card, Breadcrumb, Empty, Pagination, Modal, Tag, Progress, Divider } from 'antd';
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
import { useGetCheckInHistory, useAddCoachNotes } from '../../../hooks/useCheckIn/useCheckIn';
import { formatDateDisplay } from '../../../utils/date';
import type { CoachFeedbackNoteDto } from '../../../types/CoachHub';
import './ClientDetail.scss';

const { TextArea } = Input;

const CheckInCard: React.FC<{ checkIn: any; onPhotoClick: (url: string) => void }> = ({
  checkIn,
  onPhotoClick,
}) => {
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
          Week of {formatDateDisplay(checkIn.weekOf)}
        </div>
        <div className="checkin-card-item__submit-time mono">
          Submitted: {new Date(checkIn.submittedAt).toLocaleString()}
        </div>
      </div>

      <div className="checkin-card-item__body">
        {/* Biometrics */}
        <div className="checkin-card-item__section">
          <h4 className="checkin-card-item__section-title">Measurements</h4>
          <div className="checkin-card-item__biometrics-grid mono">
            <div className="checkin-card-item__metric">
              <span className="label">Weight</span>
              <span className="value">{checkIn.weightKg} kg</span>
            </div>
            <div className="checkin-card-item__metric">
              <span className="label">Waist</span>
              <span className="value">{checkIn.waistCm ? `${checkIn.waistCm} cm` : 'N/A'}</span>
            </div>
            <div className="checkin-card-item__metric">
              <span className="label">Chest</span>
              <span className="value">{checkIn.chestCm ? `${checkIn.chestCm} cm` : 'N/A'}</span>
            </div>
            <div className="checkin-card-item__metric">
              <span className="label">Thigh</span>
              <span className="value">{checkIn.thighCm ? `${checkIn.thighCm} cm` : 'N/A'}</span>
            </div>
          </div>
        </div>

        {/* Well-being sliders */}
        <div className="checkin-card-item__section">
          <h4 className="checkin-card-item__section-title">Subjective Well-being</h4>
          <div className="checkin-card-item__subjective-list">
            {[
              { label: 'Sleep Quality', val: checkIn.sleepQuality },
              { label: 'Energy Level', val: checkIn.energyLevel },
              { label: 'Gut Health', val: checkIn.gutHealth },
              { label: 'Training Stress', val: checkIn.trainingStress },
            ].map((marker) => (
              <div className="checkin-card-item__subjective-row" key={marker.label}>
                <span className="label">{marker.label}</span>
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
          <h4 className="checkin-card-item__section-title">Progress Photos</h4>
          <div className="checkin-card-item__photos-row">
            {checkIn.photos && checkIn.photos.length > 0 ? (
              checkIn.photos.map((photo: any) => (
                <div
                  className="checkin-card-item__photo-thumb"
                  key={photo.id}
                  onClick={() => onPhotoClick(photo.signedDownloadUrl)}
                >
                  <img src={photo.signedDownloadUrl} alt={`${photo.angle} view`} />
                  <span className="angle-label">{photo.angle}</span>
                </div>
              ))
            ) : (
              <div className="checkin-card-item__no-photos">
                <span className="material-symbols-outlined">hide_image</span>
                <span>No photos</span>
              </div>
            )}
          </div>
        </div>
      </div>

      <Divider style={{ margin: '16px 0' }} />

      {/* Coach Notes */}
      <div className="checkin-card-item__feedback-section">
        <h4 className="checkin-card-item__section-title">Check-In Review & Adjustment Notes</h4>
        <div className="checkin-card-item__notes-form">
          <TextArea
            rows={2}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Provide feedback, nutrition tweaks, or program adjustments for this check-in..."
            maxLength={2000}
            disabled={addCoachNotesMutation.isPending}
          />
          <div className="checkin-card-item__notes-actions">
            {checkIn.coachReviewedAt && (
              <span className="reviewed-at-text mono">
                Reviewed: {new Date(checkIn.coachReviewedAt).toLocaleDateString()}
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
              Save Review
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

const ClientDetail: React.FC = () => {
  const { athleteId } = useParams<{ athleteId: string }>();
  const navigate = useNavigate();
  const id = athleteId ? parseInt(athleteId, 10) : 0;

  const { data: profile, isLoading, error } = useGetAthleteProfile(id);
  const saveNoteMutation = useSaveFeedbackNote(id);

  const [noteText, setNoteText] = useState<string>('');
  const [notesList, setNotesList] = useState<CoachFeedbackNoteDto[]>([]);

  // Check-In History Pagination & Lightbox Photo State
  const [historyPage, setHistoryPage] = useState<number>(1);
  const { data: checkInHistory, isLoading: isHistoryLoading } = useGetCheckInHistory(historyPage, 5, id);
  const [lightboxPhoto, setLightboxPhoto] = useState<string | null>(null);

  // Keep notes synchronized when profile loads
  useEffect(() => {
    if (profile?.feedbackNotes) {
      setNotesList(profile.feedbackNotes);
    }
  }, [profile]);

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
        <h2>Client Profile Not Found</h2>
        <p>Could not load profile or you are unauthorized to view this client.</p>
        <Button type="primary" onClick={() => navigate('/coach/roster')}>
          Back to Roster
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
            { title: <a onClick={() => navigate('/coach/roster')}>Client Roster</a> },
            { title: isLoading ? 'Loading...' : profile?.fullName },
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
                    Goal: {profile.targetGoal}
                  </span>
                  <span className="client-detail__badge-item">
                    <span className="material-symbols-outlined">scale</span>
                    Weight: {profile.weightKg} kg
                  </span>
                  <span className="client-detail__badge-item">
                    <span className="material-symbols-outlined">height</span>
                    Height: {profile.heightCm} cm
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
                  <span className="client-detail__streak-label">Current Streak</span>
                </div>
              </div>
              <div className="client-detail__streak-divider" />
              <div className="client-detail__streak-metric">
                <span className="client-detail__streak-icon">🏆</span>
                <div>
                  <span className="client-detail__streak-value mono">{profile.longestStreak}</span>
                  <span className="client-detail__streak-label">Longest Streak</span>
                </div>
              </div>
            </div>
          </div>

          {/* Grid of Targets & Charts */}
          <div className="client-detail__grid">
            
            {/* Left: Targets & Weight History */}
            <div className="client-detail__left-col">
              
              {/* Targets Card */}
              <div className="client-detail__card">
                <div className="client-detail__card-header">
                  <span className="material-symbols-outlined text-gold">adjust</span>
                  <h3>Assigned Daily Targets</h3>
                </div>
                {profile.currentTargets ? (
                  <div className="client-detail__targets-grid">
                    <div className="client-detail__target-item">
                      <span className="client-detail__target-label">Calories</span>
                      <span className="client-detail__target-val mono">
                        {Math.round(profile.currentTargets.targetCalories)} <span className="unit">kcal</span>
                      </span>
                    </div>
                    <div className="client-detail__target-item">
                      <span className="client-detail__target-label">Protein</span>
                      <span className="client-detail__target-val mono">
                        {Math.round(profile.currentTargets.targetProtein)} <span className="unit">g</span>
                      </span>
                    </div>
                    <div className="client-detail__target-item">
                      <span className="client-detail__target-label">Carbohydrates</span>
                      <span className="client-detail__target-val mono">
                        {Math.round(profile.currentTargets.targetCarbs)} <span className="unit">g</span>
                      </span>
                    </div>
                    <div className="client-detail__target-item">
                      <span className="client-detail__target-label">Fat</span>
                      <span className="client-detail__target-val mono">
                        {Math.round(profile.currentTargets.targetFat)} <span className="unit">g</span>
                      </span>
                    </div>
                    <div className="client-detail__target-item">
                      <span className="client-detail__target-label">Water Intake</span>
                      <span className="client-detail__target-val mono">
                        {profile.currentTargets.waterLitersTarget} <span className="unit">L</span>
                      </span>
                    </div>
                    <div className="client-detail__target-item">
                      <span className="client-detail__target-label">Daily Steps</span>
                      <span className="client-detail__target-val mono">
                        {profile.currentTargets.stepsTarget.toLocaleString()} <span className="unit">steps</span>
                      </span>
                    </div>
                  </div>
                ) : (
                  <Empty description="No targets configured yet." style={{ padding: '20px 0' }} />
                )}
              </div>

              {/* Weight Trend Chart */}
              <div className="client-detail__card">
                <div className="client-detail__card-header">
                  <span className="material-symbols-outlined text-gold">show_chart</span>
                  <h3>Weight Progress Chart</h3>
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
                          unit=" kg"
                          domain={['auto', 'auto']}
                        />
                        <Tooltip
                          labelFormatter={(label) => `Week: ${formatDateDisplay(label)}`}
                          formatter={(value) => [`${value} kg`, 'Weight']}
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
                    <Empty description="No weight measurements logged yet." style={{ padding: '40px 0' }} />
                  )}
                </div>
              </div>
            </div>

            {/* Right: Feedback Notes */}
            <div className="client-detail__right-col">
              <div className="client-detail__card client-detail__card--notes">
                <div className="client-detail__card-header">
                  <span className="material-symbols-outlined text-gold">feedback</span>
                  <h3>Coach Feedback & Notes</h3>
                </div>

                {/* Add Note Input */}
                <div className="client-detail__add-note">
                  <TextArea
                    placeholder="Provide weekly feedback, target updates, or workout recommendations..."
                    rows={4}
                    value={noteText}
                    onChange={(e) => setNoteText(e.target.value)}
                    maxLength={2000}
                    disabled={saveNoteMutation.isPending}
                  />
                  <div className="client-detail__note-actions">
                    <span className="client-detail__note-count">
                      {noteText.length}/2000 characters
                    </span>
                    <Button
                      type="primary"
                      onClick={handleSaveNote}
                      loading={saveNoteMutation.isPending}
                      disabled={!noteText.trim()}
                      className="client-detail__submit-note-btn"
                    >
                      Save Note
                    </Button>
                  </div>
                </div>

                {/* Notes List */}
                <div className="client-detail__notes-list">
                  {notesList.length > 0 ? (
                    notesList.map((note) => (
                      <div className="client-detail__note-item" key={note.id}>
                        <div className="client-detail__note-header">
                          <span className="client-detail__note-author">Coach {note.coachName}</span>
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
                      <p>No feedback notes logged yet. Be the first to leave a comment!</p>
                    </div>
                  )}
                </div>
              </div>
            </div>

          </div>

          {/* Check-In History Full Width Section */}
          <div className="client-detail__history-section">
            <div className="client-detail__card">
              <div className="client-detail__card-header">
                <span className="material-symbols-outlined text-gold">assignment_turned_in</span>
                <h3>Weekly Check-In History</h3>
              </div>
              {isHistoryLoading ? (
                <div style={{ padding: '20px' }}>
                  <Skeleton active paragraph={{ rows: 6 }} />
                </div>
              ) : checkInHistory?.items && checkInHistory.items.length > 0 ? (
                <div className="client-detail__history-list">
                  {checkInHistory.items.map((checkIn) => (
                    <CheckInCard
                      key={checkIn.id}
                      checkIn={checkIn}
                      onPhotoClick={(url) => setLightboxPhoto(url)}
                    />
                  ))}
                  <div className="client-detail__history-pagination">
                    <Pagination
                      current={historyPage}
                      pageSize={5}
                      total={checkInHistory.totalCount}
                      onChange={(page) => setHistoryPage(page)}
                      showSizeChanger={false}
                    />
                  </div>
                </div>
              ) : (
                <Empty description="No weekly check-ins submitted yet." style={{ padding: '40px 0' }} />
              )}
            </div>
          </div>

        </div>
      ) : null}

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

export default ClientDetail;
