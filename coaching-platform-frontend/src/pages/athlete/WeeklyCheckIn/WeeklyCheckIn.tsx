import React, { useState, useEffect } from 'react';
import { Steps, Button, Card, Divider, Alert, Space, Result } from 'antd';
import { useNavigate } from 'react-router-dom';
import BiometricInputRow from '../../../components/BiometricInputRow/BiometricInputRow';
import SubjectiveSlider from '../../../components/SubjectiveSlider/SubjectiveSlider';
import PhotoUploadZone from '../../../components/PhotoUploadZone/PhotoUploadZone';
import { useSubmitCheckIn, useUploadPhotos, useGetCheckInHistory } from '../../../hooks/useCheckIn/useCheckIn';
import { formatDateDisplay } from '../../../utils/date';
import './WeeklyCheckIn.scss';

const WeeklyCheckIn: React.FC = () => {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState<number>(0);
  const [checkInId, setCheckInId] = useState<number | null>(null);

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

  // Upload Progress per angle (mock UI progress while Axios uploads, or Axios progress)
  const [uploadProgress, setUploadProgress] = useState<number>(0);
  const [uploading, setUploading] = useState<boolean>(false);

  // Fetch Check-In History to see if they already submitted this week
  const { data: historyData, isLoading: historyLoading } = useGetCheckInHistory(1, 1);
  const latestCheckIn = historyData?.items?.[0];

  // Prefill check-in if there is a recent one to make life easier
  useEffect(() => {
    if (latestCheckIn) {
      // If submitted in the current ISO week (or just use latest values as a baseline)
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

  // Mutations
  const submitCheckInMutation = useSubmitCheckIn();
  const uploadPhotosMutation = useUploadPhotos(checkInId || 0);

  const handleStep1Submit = () => {
    if (!weightKg) {
      return;
    }

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
        onSuccess: (data) => {
          setCheckInId(data.id);
          setCurrentStep(1);
        },
      }
    );
  };

  const handleStep2Submit = () => {
    // If no photos selected, just go to Step 3
    if (!frontFile && !sideFile && !backFile) {
      setCurrentStep(2);
      return;
    }

    setUploading(true);
    setUploadProgress(10);

    uploadPhotosMutation.mutate(
      {
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
          setCurrentStep(2);
        },
        onError: () => {
          setUploading(false);
        },
      }
    );
  };

  return (
    <div id="weekly-check-in-page" className="weekly-check-in animate-fade-in">
      <div className="weekly-check-in__header">
        <h1 className="weekly-check-in__title">Weekly Check-In</h1>
        <p className="weekly-check-in__subtitle">
          Submit your weekly stats and progress photos so your coach can adjust your targets.
        </p>
      </div>

      <div className="weekly-check-in__steps-wrapper">
        <Steps
          current={currentStep}
          items={[
            { title: 'Biometrics & Subjective' },
            { title: 'Progress Photos' },
            { title: 'Confirmation' },
          ]}
          className="weekly-check-in__steps"
        />
      </div>

      <div className="weekly-check-in__content">
        {currentStep === 0 && (
          <Card className="weekly-check-in__card" loading={historyLoading}>
            <h2 className="weekly-check-in__section-title">Step 1: Measurements & Well-being</h2>
            <p className="weekly-check-in__section-desc">
              Please enter your current body measurements. Weight is required.
            </p>

            <div className="weekly-check-in__form-grid">
              <div className="weekly-check-in__form-column">
                <h3 className="weekly-check-in__subsection-title">
                  <span className="material-symbols-outlined text-gold">scale</span> Biometrics
                </h3>
                <div className="weekly-check-in__biometrics-list">
                  <BiometricInputRow
                    label="Current Weight *"
                    value={weightKg}
                    onChange={setWeightKg}
                    unit="kg"
                    placeholder="e.g. 78.5"
                    min={30}
                    max={250}
                  />
                  <BiometricInputRow
                    label="Waist Circumference"
                    value={waistCm}
                    onChange={setWaistCm}
                    unit="cm"
                    placeholder="e.g. 82.0"
                    min={40}
                    max={200}
                  />
                  <BiometricInputRow
                    label="Chest Circumference"
                    value={chestCm}
                    onChange={setChestCm}
                    unit="cm"
                    placeholder="e.g. 104.5"
                    min={40}
                    max={200}
                  />
                  <BiometricInputRow
                    label="Thigh Circumference"
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
                  <span className="material-symbols-outlined text-gold">favorite</span> Subjective Markers
                </h3>
                <div className="weekly-check-in__sliders-list">
                  <SubjectiveSlider
                    label="Sleep Quality"
                    value={sleepQuality}
                    onChange={setSleepQuality}
                  />
                  <SubjectiveSlider
                    label="Energy Level"
                    value={energyLevel}
                    onChange={setEnergyLevel}
                  />
                  <SubjectiveSlider
                    label="Gut Health"
                    value={gutHealth}
                    onChange={setGutHealth}
                  />
                  <SubjectiveSlider
                    label="Training Stress"
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
                loading={submitCheckInMutation.isPending}
                disabled={!weightKg}
                className="weekly-check-in__next-btn"
                size="large"
              >
                Next: Add Photos <span className="material-symbols-outlined">arrow_forward</span>
              </Button>
            </div>
          </Card>
        )}

        {currentStep === 1 && (
          <Card className="weekly-check-in__card">
            <h2 className="weekly-check-in__section-title">Step 2: Progress Photos (Optional)</h2>
            <p className="weekly-check-in__section-desc">
              Upload Front, Side, and Back photos in consistent lighting to track visual composition changes.
            </p>

            <div className="weekly-check-in__photos-grid">
              <PhotoUploadZone
                angle="Front"
                file={frontFile}
                onFileSelect={setFrontFile}
                onDelete={() => setFrontFile(null)}
                uploading={uploading}
                uploadProgress={uploadProgress}
              />
              <PhotoUploadZone
                angle="Side"
                file={sideFile}
                onFileSelect={setSideFile}
                onDelete={() => setSideFile(null)}
                uploading={uploading}
                uploadProgress={uploadProgress}
              />
              <PhotoUploadZone
                angle="Back"
                file={backFile}
                onFileSelect={setBackFile}
                onDelete={() => setBackFile(null)}
                uploading={uploading}
                uploadProgress={uploadProgress}
              />
            </div>

            <Divider />

            <div className="weekly-check-in__actions weekly-check-in__actions--split">
              <Button
                type="text"
                onClick={() => setCurrentStep(2)}
                disabled={uploading}
                className="weekly-check-in__skip-btn"
              >
                Skip Photo Upload
              </Button>
              <Button
                type="primary"
                onClick={handleStep2Submit}
                loading={uploading}
                className="weekly-check-in__next-btn"
                size="large"
              >
                Upload & Continue <span className="material-symbols-outlined">arrow_forward</span>
              </Button>
            </div>
          </Card>
        )}

        {currentStep === 2 && (
          <Card className="weekly-check-in__card weekly-check-in__card--confirm">
            <Result
              status="success"
              title="Check-In Received!"
              subTitle="Your coach has been notified and will review your stats and photos shortly. Check back for custom feedback and adjustments."
              extra={[
                <Button
                  type="primary"
                  key="dashboard"
                  onClick={() => navigate('/athlete/dashboard')}
                  size="large"
                  className="weekly-check-in__done-btn"
                >
                  Return to Dashboard
                </Button>,
                <Button
                  key="history"
                  onClick={() => {
                    setCurrentStep(0);
                    setFrontFile(null);
                    setSideFile(null);
                    setBackFile(null);
                  }}
                  size="large"
                >
                  Update/Resubmit Check-in
                </Button>,
              ]}
            />
          </Card>
        )}
      </div>
    </div>
  );
};

export default WeeklyCheckIn;
