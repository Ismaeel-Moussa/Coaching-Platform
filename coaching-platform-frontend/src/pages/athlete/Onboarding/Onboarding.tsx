import { useEffect, useState, useMemo, useRef } from 'react';
import { Alert, Button, Checkbox, Form, Input, InputNumber, Select, Skeleton, Tag, Steps, Space } from 'antd';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import {
  useMyOnboardingAssessment,
  useSaveOnboardingDraft,
  useSubmitOnboardingAssessment,
  useUploadOnboardingPhotos,
  useDeleteOnboardingPhoto,
} from '../../../hooks/useOnboarding/useOnboarding';
import PhotoUploadZone from '../../../components/PhotoUploadZone/PhotoUploadZone';
import type { OnboardingAssessmentForm } from '../../../types/Onboarding';
import './Onboarding.scss';

const { TextArea } = Input;

const EMPTY_FORM: OnboardingAssessmentForm = {
  primaryGoal: null,
  weightKg: null,
  heightCm: null,
  activityLevel: null,
  trainingExperience: null,
  trainingDaysPerWeek: null,
  availableEquipment: [],
  preferredTrainingDays: [],
  injuriesOrLimitations: null,
  currentPain: null,
  averageSleepHours: null,
  sleepQuality: null,
  foodAllergies: null,
  foodIntolerances: null,
  preferredFoods: null,
  foodsToAvoid: null,
  typicalMealsPerDay: null,
  typicalMealSchedule: null,
  currentSupplements: null,
  additionalNotes: null,
};

const Onboarding = () => {
  const { t } = useTranslation(['athlete', 'common']);
  const navigate = useNavigate();
  const [form] = Form.useForm<OnboardingAssessmentForm>();
  const { data, isLoading, isError, refetch } = useMyOnboardingAssessment();
  const saveDraft = useSaveOnboardingDraft();
  const submitAssessment = useSubmitOnboardingAssessment();
  const uploadPhotos = useUploadOnboardingPhotos();
  const deletePhoto = useDeleteOnboardingPhoto();

  const [currentStep, setCurrentStep] = useState(0);
  const saveTimeoutRef = useRef<any>(null);

  // Whitelist/safely map DTO fields to Form state
  useEffect(() => {
    if (data) {
      const formKeys: (keyof OnboardingAssessmentForm)[] = [
        'primaryGoal',
        'weightKg',
        'heightCm',
        'activityLevel',
        'trainingExperience',
        'trainingDaysPerWeek',
        'availableEquipment',
        'preferredTrainingDays',
        'injuriesOrLimitations',
        'currentPain',
        'averageSleepHours',
        'sleepQuality',
        'foodAllergies',
        'foodIntolerances',
        'preferredFoods',
        'foodsToAvoid',
        'typicalMealsPerDay',
        'typicalMealSchedule',
        'currentSupplements',
        'additionalNotes',
      ];

      const values = { ...EMPTY_FORM };
      formKeys.forEach((key) => {
        const val = data[key];
        if (val !== undefined && val !== null) {
          (values as any)[key] = val;
        }
      });
      form.setFieldsValue(values);
    }
  }, [data, form]);

  // Clean up timeout on unmount
  useEffect(() => {
    return () => {
      if (saveTimeoutRef.current) clearTimeout(saveTimeoutRef.current);
    };
  }, []);

  // Memoize options to avoid unnecessary renders
  const activityOptions = useMemo(() => ['Sedentary', 'Light', 'Moderate', 'High', 'VeryHigh'].map((value) => ({
    value,
    label: t(`athlete:onboarding.options.activity.${value}`),
  })), [t]);

  const experienceOptions = useMemo(() => ['Beginner', 'Intermediate', 'Advanced'].map((value) => ({
    value,
    label: t(`athlete:onboarding.options.experience.${value}`),
  })), [t]);

  const sleepOptions = useMemo(() => ['Poor', 'Fair', 'Good', 'Excellent'].map((value) => ({
    value,
    label: t(`athlete:onboarding.options.sleep.${value}`),
  })), [t]);

  const dayOptions = useMemo(() => ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'].map((value) => ({
    value,
    label: t(`athlete:onboarding.options.days.${value}`),
  })), [t]);

  const equipmentOptions = useMemo(() => ['Gym', 'Dumbbells', 'Bands', 'Bodyweight', 'HomeMachines', 'Other'].map((value) => ({
    value,
    label: t(`athlete:onboarding.options.equipment.${value}`),
  })), [t]);

  const goalOptions = useMemo(() => ['FatLoss', 'MuscleGain', 'Strength', 'Fitness', 'Health'].map((value) => ({
    value,
    label: t(`athlete:onboarding.options.goals.${value}`),
  })), [t]);

  const isReadOnly = data?.status === 'Submitted' || data?.status === 'Reviewed';

  // Debounced auto-save on form values change
  const handleValuesChange = () => {
    if (isReadOnly) return;

    if (saveTimeoutRef.current) clearTimeout(saveTimeoutRef.current);

    saveTimeoutRef.current = setTimeout(() => {
      const values = { ...EMPTY_FORM, ...form.getFieldsValue(true) };
      saveDraft.mutate(values);
    }, 2000); // 2s debounce
  };

  const handleSelectPhoto = async (angle: 'Front' | 'Side' | 'Back', file: File) => {
    try {
      const files = { [angle]: file };
      await uploadPhotos.mutateAsync({ files });
    } catch (err) {
      console.error('Failed to upload onboarding photo:', err);
    }
  };

  const handleDeletePhoto = async (angle: 'Front' | 'Side' | 'Back') => {
    try {
      await deletePhoto.mutateAsync(angle);
    } catch (err) {
      console.error('Failed to delete onboarding photo:', err);
    }
  };

  const handleNext = async () => {
    let fieldsToValidate: (keyof OnboardingAssessmentForm)[] = [];
    if (currentStep === 0) {
      fieldsToValidate = ['primaryGoal', 'weightKg', 'heightCm', 'activityLevel'];
    } else if (currentStep === 1) {
      fieldsToValidate = ['trainingExperience', 'trainingDaysPerWeek'];
    } else if (currentStep === 2) {
      fieldsToValidate = ['averageSleepHours', 'sleepQuality'];
    } else if (currentStep === 3) {
      fieldsToValidate = ['typicalMealsPerDay'];
    }

    try {
      await form.validateFields(fieldsToValidate);
      setCurrentStep((prev) => prev + 1);

      // Perform draft save when moving to next step
      const values = { ...EMPTY_FORM, ...form.getFieldsValue(true) };
      saveDraft.mutate(values);
    } catch (err) {
      // Validation failed, Ant Design will show error messages automatically
    }
  };

  const handleBack = () => {
    setCurrentStep((prev) => prev - 1);
  };

  const handleSubmit = async (values: OnboardingAssessmentForm) => {
    try {
      await submitAssessment.mutateAsync({ ...EMPTY_FORM, ...values });
      navigate('/athlete/dashboard', { replace: true });
    } catch (err) {
      console.error('Failed to submit assessment:', err);
    }
  };

  if (isLoading) return <div className="onboarding-page"><Skeleton active paragraph={{ rows: 12 }} /></div>;
  if (isError) return (
    <div className="onboarding-page onboarding-page--error">
      <Alert type="error" showIcon message={t('athlete:onboarding.messages.loadFailed')} />
      <Button onClick={() => refetch()}>{t('common:actions.retry')}</Button>
    </div>
  );

  const statusColor = data?.status === 'Reviewed' ? 'green' : data?.status === 'Submitted' ? 'blue' : 'gold';

  const existingFrontPhoto = data?.photos?.find((p) => p.angle === 'Front');
  const existingSidePhoto = data?.photos?.find((p) => p.angle === 'Side');
  const existingBackPhoto = data?.photos?.find((p) => p.angle === 'Back');

  return (
    <main className="onboarding-page">
      <header className="onboarding-page__hero">
        <div className="onboarding-page__hero-copy">
          <span className="onboarding-page__eyebrow">{t('athlete:onboarding.eyebrow')}</span>
          <h1>{t('athlete:onboarding.title')}</h1>
          <p>{t('athlete:onboarding.subtitle')}</p>
        </div>
        <Tag color={statusColor} className="onboarding-page__status">
          {t(`athlete:onboarding.status.${data?.status ?? 'NotStarted'}`)}
        </Tag>
      </header>

      {data?.status === 'Reviewed' && data.coachReviewNotes && (
        <Alert
          className="onboarding-page__coach-note"
          type="success"
          showIcon
          message={t('athlete:onboarding.coachNote')}
          description={data.coachReviewNotes}
        />
      )}

      {isReadOnly && (
        <Alert
          className="onboarding-page__coach-note"
          type="info"
          showIcon
          message={t('athlete:onboarding.messages.readOnly')}
        />
      )}

      {/* Progress Steps Indicator */}
      <div className="weekly-check-in__steps-wrapper" style={{ marginBottom: '24px' }}>
        <Steps
          current={currentStep}
          onChange={(step) => {
            if (step === currentStep) return;
            if (isReadOnly || step < currentStep) {
              setCurrentStep(step);
            } else {
              handleNext();
            }
          }}
          items={[
            { title: t('athlete:onboarding.sections.basics') },
            { title: t('athlete:onboarding.sections.training') },
            { title: t('athlete:onboarding.sections.safety') },
            { title: t('athlete:onboarding.sections.nutrition') },
            { title: t('athlete:onboarding.sections.photos') },
          ]}
        />
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        onValuesChange={handleValuesChange}
        requiredMark="optional"
        className="onboarding-form"
        disabled={isReadOnly}
      >
        {/* Step 1: Baseline & Basics */}
        <div style={{ display: currentStep === 0 ? 'block' : 'none' }}>
          <section className="onboarding-section animate-fade-in">
            <div className="onboarding-section__heading">
              <span className="material-symbols-outlined">track_changes</span>
              <div>
                <h2>{t('athlete:onboarding.sections.basics')}</h2>
                <p>{t('athlete:onboarding.sections.basicsHint')}</p>
              </div>
            </div>
            <div className="onboarding-form__grid">
              <Form.Item name="primaryGoal" label={t('athlete:onboarding.fields.primaryGoal')} rules={[{ required: true }]}>
                <Select options={goalOptions} />
              </Form.Item>
              <Form.Item label={t('athlete:onboarding.fields.weightKg')} required>
                <Space.Compact style={{ width: '100%' }}>
                  <Form.Item name="weightKg" noStyle rules={[{ required: true }]}>
                    <InputNumber min={25} max={400} step={0.1} style={{ width: '100%' }} />
                  </Form.Item>
                  <Button disabled style={{ color: 'var(--color-text-secondary)', backgroundColor: 'var(--surface-container-low)', borderColor: 'var(--color-border)' }}>kg</Button>
                </Space.Compact>
              </Form.Item>
              <Form.Item label={t('athlete:onboarding.fields.heightCm')} required>
                <Space.Compact style={{ width: '100%' }}>
                  <Form.Item name="heightCm" noStyle rules={[{ required: true }]}>
                    <InputNumber min={100} max={250} step={0.1} style={{ width: '100%' }} />
                  </Form.Item>
                  <Button disabled style={{ color: 'var(--color-text-secondary)', backgroundColor: 'var(--surface-container-low)', borderColor: 'var(--color-border)' }}>cm</Button>
                </Space.Compact>
              </Form.Item>
              <Form.Item name="activityLevel" label={t('athlete:onboarding.fields.activityLevel')} rules={[{ required: true }]}>
                <Select options={activityOptions} />
              </Form.Item>
            </div>
          </section>
        </div>

        {/* Step 2: Training Profile */}
        <div style={{ display: currentStep === 1 ? 'block' : 'none' }}>
          <section className="onboarding-section animate-fade-in">
            <div className="onboarding-section__heading">
              <span className="material-symbols-outlined">fitness_center</span>
              <div>
                <h2>{t('athlete:onboarding.sections.training')}</h2>
                <p>{t('athlete:onboarding.sections.trainingHint')}</p>
              </div>
            </div>
            <div className="onboarding-form__grid">
              <Form.Item name="trainingExperience" label={t('athlete:onboarding.fields.trainingExperience')} rules={[{ required: true }]}>
                <Select options={experienceOptions} />
              </Form.Item>
              <Form.Item name="trainingDaysPerWeek" label={t('athlete:onboarding.fields.trainingDays')} rules={[{ required: true }]}>
                <InputNumber min={1} max={7} />
              </Form.Item>
              <Form.Item className="onboarding-form__full" name="preferredTrainingDays" label={t('athlete:onboarding.fields.preferredDays')}>
                <Checkbox.Group options={dayOptions} />
              </Form.Item>
              <Form.Item className="onboarding-form__full" name="availableEquipment" label={t('athlete:onboarding.fields.equipment')}>
                <Checkbox.Group options={equipmentOptions} />
              </Form.Item>
            </div>
          </section>
        </div>

        {/* Step 3: Health & Safety */}
        <div style={{ display: currentStep === 2 ? 'block' : 'none' }}>
          <section className="onboarding-section onboarding-section--safety animate-fade-in">
            <div className="onboarding-section__heading">
              <span className="material-symbols-outlined">health_and_safety</span>
              <div>
                <h2>{t('athlete:onboarding.sections.safety')}</h2>
                <p>{t('athlete:onboarding.sections.safetyHint')}</p>
              </div>
            </div>
            <div className="onboarding-form__grid">
              <Form.Item name="injuriesOrLimitations" label={t('athlete:onboarding.fields.injuries')}>
                <TextArea rows={3} placeholder={t('athlete:onboarding.placeholders.none')} />
              </Form.Item>
              <Form.Item name="currentPain" label={t('athlete:onboarding.fields.currentPain')}>
                <TextArea rows={3} placeholder={t('athlete:onboarding.placeholders.none')} />
              </Form.Item>
              <Form.Item name="averageSleepHours" label={t('athlete:onboarding.fields.sleepHours')} rules={[{ required: true }]}>
                <InputNumber min={0} max={24} step={0.5} />
              </Form.Item>
              <Form.Item name="sleepQuality" label={t('athlete:onboarding.fields.sleepQuality')} rules={[{ required: true }]}>
                <Select options={sleepOptions} />
              </Form.Item>
            </div>
          </section>
        </div>

        {/* Step 4: Nutrition Habits */}
        <div style={{ display: currentStep === 3 ? 'block' : 'none' }}>
          <section className="onboarding-section animate-fade-in">
            <div className="onboarding-section__heading">
              <span className="material-symbols-outlined">restaurant</span>
              <div>
                <h2>{t('athlete:onboarding.sections.nutrition')}</h2>
                <p>{t('athlete:onboarding.sections.nutritionHint')}</p>
              </div>
            </div>
            <div className="onboarding-form__grid">
              <Form.Item name="foodAllergies" label={t('athlete:onboarding.fields.allergies')}>
                <TextArea rows={3} placeholder={t('athlete:onboarding.placeholders.none')} />
              </Form.Item>
              <Form.Item name="foodIntolerances" label={t('athlete:onboarding.fields.intolerances')}>
                <TextArea rows={3} placeholder={t('athlete:onboarding.placeholders.none')} />
              </Form.Item>
              <Form.Item name="preferredFoods" label={t('athlete:onboarding.fields.preferredFoods')}>
                <TextArea rows={3} />
              </Form.Item>
              <Form.Item name="foodsToAvoid" label={t('athlete:onboarding.fields.foodsToAvoid')}>
                <TextArea rows={3} placeholder={t('athlete:onboarding.placeholders.none')} />
              </Form.Item>
              <Form.Item name="typicalMealsPerDay" label={t('athlete:onboarding.fields.mealsPerDay')} rules={[{ required: true }]}>
                <InputNumber min={1} max={10} />
              </Form.Item>
              <Form.Item name="typicalMealSchedule" label={t('athlete:onboarding.fields.mealSchedule')}>
                <Input />
              </Form.Item>
              <Form.Item name="currentSupplements" label={t('athlete:onboarding.fields.supplements')}>
                <TextArea rows={3} placeholder={t('athlete:onboarding.placeholders.none')} />
              </Form.Item>
              <Form.Item name="additionalNotes" label={t('athlete:onboarding.fields.notes')}>
                <TextArea rows={3} />
              </Form.Item>
            </div>
          </section>
        </div>

        {/* Step 5: Progress Photos (Optional) */}
        <div style={{ display: currentStep === 4 ? 'block' : 'none' }}>
          <section className="onboarding-section animate-fade-in">
            <div className="onboarding-section__heading">
              <span className="material-symbols-outlined">add_a_photo</span>
              <div>
                <h2>{t('athlete:onboarding.sections.photos')}</h2>
                <p>{t('athlete:onboarding.sections.photosHint')}</p>
              </div>
            </div>
            
            <div className="onboarding-photo-uploader">
              <PhotoUploadZone
                angle="Front"
                file={null}
                onFileSelect={(file) => handleSelectPhoto('Front', file)}
                onDelete={() => handleDeletePhoto('Front')}
                existingUrl={existingFrontPhoto?.signedDownloadUrl}
                uploading={uploadPhotos.isPending && uploadPhotos.variables?.files.Front !== undefined}
              />
              <PhotoUploadZone
                angle="Side"
                file={null}
                onFileSelect={(file) => handleSelectPhoto('Side', file)}
                onDelete={() => handleDeletePhoto('Side')}
                existingUrl={existingSidePhoto?.signedDownloadUrl}
                uploading={uploadPhotos.isPending && uploadPhotos.variables?.files.Side !== undefined}
              />
              <PhotoUploadZone
                angle="Back"
                file={null}
                onFileSelect={(file) => handleSelectPhoto('Back', file)}
                onDelete={() => handleDeletePhoto('Back')}
                existingUrl={existingBackPhoto?.signedDownloadUrl}
                uploading={uploadPhotos.isPending && uploadPhotos.variables?.files.Back !== undefined}
              />
            </div>
          </section>
        </div>

        {/* Navigation & Action buttons */}
        <footer className="onboarding-form__actions">
          <div>
            <strong>{t('athlete:onboarding.readyTitle')}</strong>
            <span>{t('athlete:onboarding.readyHint')}</span>
          </div>
          <div className="onboarding-form__buttons">
            {isReadOnly ? (
              <Button key="dashboard-btn" size="large" htmlType="button" onClick={() => navigate('/athlete/dashboard')}>
                {t('common:nav.dashboard')}
              </Button>
            ) : (
              <>
                {currentStep > 0 && (
                  <Button key="back-btn" size="large" htmlType="button" onClick={handleBack}>
                    {t('common:actions.back')}
                  </Button>
                )}
                
                {currentStep < 4 ? (
                  <Button key="next-btn" size="large" type="primary" htmlType="button" onClick={handleNext}>
                    {t('common:actions.next')}
                  </Button>
                ) : (
                  <Button
                    key="submit-btn"
                    size="large"
                    type="primary"
                    htmlType="submit"
                    loading={submitAssessment.isPending}
                  >
                    {t('athlete:onboarding.submit')}
                  </Button>
                )}
              </>
            )}
          </div>
        </footer>
      </Form>
    </main>
  );
};

export default Onboarding;
