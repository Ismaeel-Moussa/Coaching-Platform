import React, { useState, useEffect } from 'react';
import { Steps, Button, Card, Divider, Alert, Space, Result, Progress } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import BiometricInputRow from '../../../components/BiometricInputRow/BiometricInputRow';
import SubjectiveSlider from '../../../components/SubjectiveSlider/SubjectiveSlider';
import PhotoUploadZone from '../../../components/PhotoUploadZone/PhotoUploadZone';
import { useSubmitCheckIn, useUploadPhotos, useGetCheckInHistory } from '../../../hooks/useCheckIn/useCheckIn';
import { deletePhoto } from '../../../api/checkIn';
import { formatDateDisplay } from '../../../utils/date';
import './WeeklyCheckIn.scss';

const PhotoPreview: React.FC<{ file: File | null; existingUrl?: string | null; label: string }> = ({ file, existingUrl, label }) => {
  const { t } = useTranslation();
  const [url, setUrl] = useState<string | null>(null);

  useEffect(() => {
    if (file) {
      const objectUrl = URL.createObjectURL(file);
      setUrl(objectUrl);
      return () => URL.revokeObjectURL(objectUrl);
    }
    setUrl(null);
  }, [file]);

  const displayUrl = url || existingUrl;

  return (
    <div className="weekly-check-in__review-photo-item">
      <h4>{label}</h4>
      {displayUrl ? (
        <img src={displayUrl} alt={`${label} Preview`} className="weekly-check-in__review-photo-img" />
      ) : (
        <div className="weekly-check-in__review-photo-empty">{t('athlete:checkIn.noPhotoSelected')}</div>
      )}
    </div>
  );
};

const WeeklyCheckIn: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState<number>(0);
  const [checkInId, setCheckInId] = useState<number | null>(null);
  const [hasExplicitlyChosenToResubmit, setHasExplicitlyChosenToResubmit] = useState<boolean>(false);
  const [isSubmissionSuccessful, setIsSubmissionSuccessful] = useState<boolean>(false);
  const [isAlreadySubmittedThisWeek, setIsAlreadySubmittedThisWeek] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  // Step 1 Form States
  const [weightKg, setWeightKg] = useState<number | null>(null);
  const [waistCm, setWaistCm] = useState<number | null>(null);
  const [chestCm, setChestCm] = useState<number | null>(null);
  const [thighCm, setThighCm] = useState<number | null>(null);

  const [sleepQuality, setSleepQuality] = useState<number>(5);
  const [energyLevel, setEnergyLevel] = useState<number>(5);
  const [gutHealth, setGutHealth] = useState<number>(5);
  const [trainingStress, setTrainingStress] = useState<number>(5);

  // Step 2 Photo Files States
  const [frontFile, setFrontFile] = useState<File | null>(null);
  const [sideFile, setSideFile] = useState<File | null>(null);
  const [backFile, setBackFile] = useState<File | null>(null);

  // Photo deletion tracking for resubmission
  const [deletedAngles, setDeletedAngles] = useState<string[]>([]);

  const handleSelectFront = (file: File) => {
    setFrontFile(file);
    setDeletedAngles(prev => prev.filter(a => a !== 'Front'));
  };
  const handleDeleteFront = () => {
    if (frontFile) {
      setFrontFile(null);
    } else {
      setDeletedAngles(prev => [...prev, 'Front']);
    }
  };

  const handleSelectSide = (file: File) => {
    setSideFile(file);
    setDeletedAngles(prev => prev.filter(a => a !== 'Side'));
  };
  const handleDeleteSide = () => {
    if (sideFile) {
      setSideFile(null);
    } else {
      setDeletedAngles(prev => [...prev, 'Side']);
    }
  };

  const handleSelectBack = (file: File) => {
    setBackFile(file);
    setDeletedAngles(prev => prev.filter(a => a !== 'Back'));
  };
  const handleDeleteBack = () => {
    if (backFile) {
      setBackFile(null);
    } else {
      setDeletedAngles(prev => [...prev, 'Back']);
    }
  };

  // Upload Progress per angle (mock UI progress while Axios uploads, or Axios progress)
  const [uploadProgress, setUploadProgress] = useState<number>(0);
  const [uploading, setUploading] = useState<boolean>(false);

  // Fetch Check-In History to see if they already submitted this week
  const { data: historyData, isLoading: historyLoading } = useGetCheckInHistory(1, 1);
  const latestCheckIn = historyData?.items?.[0];

  const existingFrontPhoto = latestCheckIn?.photos?.find((p: any) => p.angle === 'Front');
  const existingSidePhoto = latestCheckIn?.photos?.find((p: any) => p.angle === 'Side');
  const existingBackPhoto = latestCheckIn?.photos?.find((p: any) => p.angle === 'Back');

  // Prefill check-in if there is a recent one to make life easier
  useEffect(() => {
    if (latestCheckIn) {
      setWeightKg(latestCheckIn.weightKg);
      setWaistCm(latestCheckIn.waistCm);
      setChestCm(latestCheckIn.chestCm);
      setThighCm(latestCheckIn.thighCm);
      setSleepQuality(latestCheckIn.sleepQuality);
      setEnergyLevel(latestCheckIn.energyLevel);
      setGutHealth(latestCheckIn.gutHealth);
      setTrainingStress(latestCheckIn.trainingStress);
    }
  }, [latestCheckIn]);

  // Jump to Confirmation if already submitted for current ISO week
  useEffect(() => {
    if (latestCheckIn && !hasExplicitlyChosenToResubmit) {
      // Calculate Monday of current ISO week
      const today = new Date();
      const day = today.getDay();
      const diff = today.getDate() - day + (day === 0 ? -6 : 1);
      const currentMonday = new Date(today.setDate(diff));
      currentMonday.setHours(0, 0, 0, 0);

      const checkInDate = new Date(latestCheckIn.weekOf);
      const isThisWeek =
        checkInDate.getFullYear() === currentMonday.getFullYear() &&
        checkInDate.getMonth() === currentMonday.getMonth() &&
        checkInDate.getDate() === currentMonday.getDate();

      if (isThisWeek) {
        setCheckInId(latestCheckIn.id);
        setCurrentStep(2);
        setIsAlreadySubmittedThisWeek(true);
      }
    }
  }, [latestCheckIn, hasExplicitlyChosenToResubmit]);

  // Mutations
  const submitCheckInMutation = useSubmitCheckIn();
  const uploadPhotosMutation = useUploadPhotos();

  const handleStep1Submit = () => {
    if (!weightKg) {
      return;
    }
    setCurrentStep(1);
  };

  const handleStep2Submit = () => {
    setCurrentStep(2);
  };

  const handleFinalSubmit = () => {
    if (!weightKg) {
      return;
    }

    setIsSubmitting(true);
    setUploadProgress(0);

    submitCheckInMutation.mutate(
      {
        weightKg,
        waistCm,
        chestCm,
        thighCm,
        sleepQuality,
        energyLevel,
        gutHealth,
        trainingStress,
      },
      {
        onSuccess: async (data) => {
          const newCheckInId = data.id;
          setCheckInId(newCheckInId);

          try {
            // Process photo deletions on the server first
            for (const angle of deletedAngles) {
              const hasPhotoInLatest = latestCheckIn?.photos?.some(p => p.angle === angle);
              if (hasPhotoInLatest) {
                await deletePhoto(newCheckInId, angle as any);
              }
            }

            // Upload any newly selected photos
            const hasPhotos = frontFile || sideFile || backFile;
            if (hasPhotos) {
              setUploading(true);
              setUploadProgress(10);

              uploadPhotosMutation.mutate(
                {
                  checkInId: newCheckInId,
                  files: {
                    Front: frontFile || undefined,
                    Side: sideFile || undefined,
                    Back: backFile || undefined,
                  },
                  onProgress: (progress) => {
                    setUploadProgress(progress);
                  },
                },
                {
                  onSuccess: () => {
                    setUploading(false);
                    setIsSubmitting(false);
                    setIsSubmissionSuccessful(true);
                    setHasExplicitlyChosenToResubmit(false);
                    setDeletedAngles([]);
                  },
                  onError: () => {
                    setUploading(false);
                    setIsSubmitting(false);
                    setIsSubmissionSuccessful(true);
                    setHasExplicitlyChosenToResubmit(false);
                    setDeletedAngles([]);
                  },
                }
              );
            } else {
              setIsSubmitting(false);
              setIsSubmissionSuccessful(true);
              setHasExplicitlyChosenToResubmit(false);
              setDeletedAngles([]);
            }
          } catch (err) {
            console.error('Error updating photos:', err);
            setIsSubmitting(false);
            setIsSubmissionSuccessful(true);
            setHasExplicitlyChosenToResubmit(false);
            setDeletedAngles([]);
          }
        },
        onError: () => {
          setIsSubmitting(false);
        },
      }
    );
  };

  return (
    <div id="weekly-check-in-page" className="weekly-check-in animate-fade-in">
      <div className="weekly-check-in__header">
        <h1 className="weekly-check-in__title">{t('athlete:checkIn.title')}</h1>
        <p className="weekly-check-in__subtitle">
          {t('athlete:checkIn.subtitle')}
        </p>
      </div>

      <div className="weekly-check-in__steps-wrapper">
        <Steps
          current={currentStep}
          items={[
            { title: t('athlete:checkIn.step1') },
            { title: t('athlete:checkIn.step2') },
            { title: isAlreadySubmittedThisWeek || isSubmissionSuccessful ? t('athlete:checkIn.step3Confirm') : t('athlete:checkIn.step3Review') },
          ]}
          className="weekly-check-in__steps"
        />
      </div>

      <div className="weekly-check-in__content">
        <div style={{ display: currentStep === 0 ? 'block' : 'none' }}>
          <Card className="weekly-check-in__card" loading={historyLoading}>
            <h2 className="weekly-check-in__section-title">{t('athlete:checkIn.step1Title')}</h2>
            <p className="weekly-check-in__section-desc">
              {t('athlete:checkIn.step1Desc')}
            </p>

            <div className="weekly-check-in__form-grid">
              <div className="weekly-check-in__form-column">
                <h3 className="weekly-check-in__subsection-title">
                  <span className="material-symbols-outlined text-gold">scale</span> {t('athlete:checkIn.biometricsTitle')}
                </h3>
                <div className="weekly-check-in__biometrics-list">
                  <BiometricInputRow
                    label={t('athlete:checkIn.weightLabel')}
                    value={weightKg}
                    onChange={setWeightKg}
                    unit="kg"
                    placeholder="e.g. 78.5"
                    min={30}
                    max={250}
                    required
                  />
                  <BiometricInputRow
                    label={t('athlete:checkIn.waistLabel')}
                    value={waistCm}
                    onChange={setWaistCm}
                    unit="cm"
                    placeholder="e.g. 82.0"
                    min={40}
                    max={200}
                  />
                  <BiometricInputRow
                    label={t('athlete:checkIn.chestLabel')}
                    value={chestCm}
                    onChange={setChestCm}
                    unit="cm"
                    placeholder="e.g. 104.5"
                    min={40}
                    max={200}
                  />
                  <BiometricInputRow
                    label={t('athlete:checkIn.thighLabel')}
                    value={thighCm}
                    onChange={setThighCm}
                    unit="cm"
                    placeholder="e.g. 58.0"
                    min={30}
                    max={120}
                  />
                </div>
              </div>

              <div className="weekly-check-in__form-column">
                <h3 className="weekly-check-in__subsection-title">
                  <span className="material-symbols-outlined text-gold">favorite</span> {t('athlete:checkIn.subjectiveTitle')}
                </h3>
                <div className="weekly-check-in__sliders-list">
                  <SubjectiveSlider
                    label={t('athlete:checkIn.sleepLabel')}
                    value={sleepQuality}
                    onChange={setSleepQuality}
                  />
                  <SubjectiveSlider
                    label={t('athlete:checkIn.energyLabel')}
                    value={energyLevel}
                    onChange={setEnergyLevel}
                  />
                  <SubjectiveSlider
                    label={t('athlete:checkIn.gutLabel')}
                    value={gutHealth}
                    onChange={setGutHealth}
                  />
                  <SubjectiveSlider
                    label={t('athlete:checkIn.stressLabel')}
                    value={trainingStress}
                    onChange={setTrainingStress}
                  />
                </div>
              </div>
            </div>

            <Divider />

            <div className="weekly-check-in__actions">
              <Button
                type="primary"
                onClick={handleStep1Submit}
                disabled={!weightKg}
                className="weekly-check-in__next-btn"
                size="large"
              >
                {t('athlete:checkIn.nextPhotosBtn')} <span className="material-symbols-outlined">arrow_forward</span>
              </Button>
            </div>
          </Card>
        </div>

        <div style={{ display: currentStep === 1 ? 'block' : 'none' }}>
          <Card className="weekly-check-in__card">
            <h2 className="weekly-check-in__section-title">{t('athlete:checkIn.step2Title')}</h2>
            <p className="weekly-check-in__section-desc">
              {t('athlete:checkIn.step2Desc')}
            </p>

            <div className="weekly-check-in__photos-grid">
              <PhotoUploadZone
                angle="Front"
                file={frontFile}
                onFileSelect={handleSelectFront}
                onDelete={handleDeleteFront}
                existingUrl={!deletedAngles.includes('Front') ? existingFrontPhoto?.signedDownloadUrl : null}
                uploading={false}
              />
              <PhotoUploadZone
                angle="Side"
                file={sideFile}
                onFileSelect={handleSelectSide}
                onDelete={handleDeleteSide}
                existingUrl={!deletedAngles.includes('Side') ? existingSidePhoto?.signedDownloadUrl : null}
                uploading={false}
              />
              <PhotoUploadZone
                angle="Back"
                file={backFile}
                onFileSelect={handleSelectBack}
                onDelete={handleDeleteBack}
                existingUrl={!deletedAngles.includes('Back') ? existingBackPhoto?.signedDownloadUrl : null}
                uploading={false}
              />
            </div>

            <Divider />

            <div className="weekly-check-in__actions weekly-check-in__actions--split">
              <Button
                onClick={() => setCurrentStep(0)}
                className="weekly-check-in__back-btn"
                size="large"
              >
                <span className="material-symbols-outlined">arrow_back</span> {t('athlete:checkIn.backBtn')}
              </Button>
              <Button
                type="primary"
                onClick={handleStep2Submit}
                className="weekly-check-in__next-btn"
                size="large"
              >
                {t('athlete:checkIn.nextReviewBtn')} <span className="material-symbols-outlined">arrow_forward</span>
              </Button>
            </div>
          </Card>
        </div>

        <div style={{ display: currentStep === 2 ? 'block' : 'none' }}>
          {isAlreadySubmittedThisWeek || isSubmissionSuccessful ? (
            <Card className="weekly-check-in__card weekly-check-in__card--confirm">
              <Result
                status="success"
                title={t('athlete:checkIn.successTitle')}
                subTitle={t('athlete:checkIn.successDesc')}
                extra={
                  <div className="weekly-check-in__confirm-actions">
                    <Button
                      type="primary"
                      onClick={() => navigate('/athlete/dashboard')}
                      size="large"
                      className="weekly-check-in__done-btn"
                    >
                      {t('athlete:checkIn.returnDashboard')}
                    </Button>
                    <Button
                      onClick={() => {
                        setHasExplicitlyChosenToResubmit(true);
                        setCurrentStep(0);
                        setFrontFile(null);
                        setSideFile(null);
                        setBackFile(null);
                        setDeletedAngles([]);
                        setIsSubmissionSuccessful(false);
                        setIsAlreadySubmittedThisWeek(false);
                      }}
                      size="large"
                      className="weekly-check-in__resubmit-btn"
                    >
                      {t('athlete:checkIn.resubmitBtn')}
                    </Button>
                  </div>
                }
              />
            </Card>
          ) : (
            <Card className="weekly-check-in__card">
              <h2 className="weekly-check-in__section-title">{t('athlete:checkIn.step3Title')}</h2>
              <p className="weekly-check-in__section-desc">
                {t('athlete:checkIn.step3Desc')}
              </p>

              <div className="weekly-check-in__review-sections">
                <div className="weekly-check-in__review-section">
                  <h3 className="weekly-check-in__subsection-title">
                    <span className="material-symbols-outlined text-gold">scale</span> {t('athlete:checkIn.biometricsSummary')}
                  </h3>
                  <div className="weekly-check-in__review-grid">
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.weight')}:</strong> <span>{weightKg} kg</span>
                    </div>
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.waist')}:</strong> <span>{waistCm ? `${waistCm} cm` : t('athlete:checkIn.notProvided')}</span>
                    </div>
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.chest')}:</strong> <span>{chestCm ? `${chestCm} cm` : t('athlete:checkIn.notProvided')}</span>
                    </div>
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.thigh')}:</strong> <span>{thighCm ? `${thighCm} cm` : t('athlete:checkIn.notProvided')}</span>
                    </div>
                  </div>
                </div>

                <Divider />

                <div className="weekly-check-in__review-section">
                  <h3 className="weekly-check-in__subsection-title">
                    <span className="material-symbols-outlined text-gold">favorite</span> {t('athlete:checkIn.subjectiveSummary')}
                  </h3>
                  <div className="weekly-check-in__review-grid">
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.sleepLabel')}:</strong> <span>{sleepQuality}/10</span>
                    </div>
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.energyLabel')}:</strong> <span>{energyLevel}/10</span>
                    </div>
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.gutLabel')}:</strong> <span>{gutHealth}/10</span>
                    </div>
                    <div className="weekly-check-in__review-item">
                      <strong>{t('athlete:checkIn.stressLabel')}:</strong> <span>{trainingStress}/10</span>
                    </div>
                  </div>
                </div>

                <Divider />

                <div className="weekly-check-in__review-section">
                  <h3 className="weekly-check-in__subsection-title">
                    <span className="material-symbols-outlined text-gold">photo_camera</span> {t('athlete:checkIn.photosSummary')}
                  </h3>
                  <div className="weekly-check-in__review-photos">
                    <PhotoPreview 
                      file={frontFile} 
                      existingUrl={!deletedAngles.includes('Front') ? existingFrontPhoto?.signedDownloadUrl : null} 
                      label={t('athlete:checkIn.frontView')} 
                    />
                    <PhotoPreview 
                      file={sideFile} 
                      existingUrl={!deletedAngles.includes('Side') ? existingSidePhoto?.signedDownloadUrl : null} 
                      label={t('athlete:checkIn.sideView')} 
                    />
                    <PhotoPreview 
                      file={backFile} 
                      existingUrl={!deletedAngles.includes('Back') ? existingBackPhoto?.signedDownloadUrl : null} 
                      label={t('athlete:checkIn.backView')} 
                    />
                  </div>
                </div>
              </div>

              {uploading && (
                <div style={{ marginTop: 24, padding: '0 16px' }}>
                  <Progress percent={uploadProgress} status="active" strokeColor="var(--color-gold)" />
                  <div style={{ textAlign: 'center', marginTop: 8, color: 'var(--color-text-secondary)', fontSize: 14 }}>
                    {t('athlete:checkIn.uploadingProgress', { progress: uploadProgress })}
                  </div>
                </div>
              )}

              <Divider />

              <div className="weekly-check-in__actions weekly-check-in__actions--split">
                <Button
                  onClick={() => setCurrentStep(1)}
                  disabled={isSubmitting}
                  className="weekly-check-in__back-btn"
                  size="large"
                >
                  <span className="material-symbols-outlined">arrow_back</span> {t('athlete:checkIn.backBtn')}
                </Button>
                <Button
                  type="primary"
                  onClick={handleFinalSubmit}
                  loading={isSubmitting}
                  className="weekly-check-in__next-btn"
                  size="large"
                >
                  {t('athlete:checkIn.submitBtn')} <span className="material-symbols-outlined">done</span>
                </Button>
              </div>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
};

export default WeeklyCheckIn;
