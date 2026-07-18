import type { AxiosProgressEvent, AxiosRequestConfig } from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import axiosInstance from './axiosInstance';
import { addCoachNotes, getCheckInPhotos, submitCheckIn, uploadPhotos } from './checkIn';
import {
  reopenAthleteOnboardingAssessment,
  reviewAthleteOnboardingAssessment,
  saveMyOnboardingDraft,
  submitMyOnboardingAssessment,
  uploadOnboardingPhotos,
} from './onboarding';
import { logFood, logNutritionPlanOption } from './diary';
import { assignNutritionPlan } from './nutritionPlan';
import { completeWorkout, logSet } from './workout';
import { saveFeedbackNote } from './coachHub';
import { MealType } from '../types/Diary';
import type { OnboardingAssessmentForm } from '../types/Onboarding';

vi.mock('./axiosInstance', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}));

const mockedAxios = vi.mocked(axiosInstance);

const onboardingForm = {
  primaryGoal: 'Build muscle',
  weightKg: 80,
  heightCm: 180,
  activityLevel: 'Active',
  trainingExperience: 'Intermediate',
  trainingDaysPerWeek: 4,
  availableEquipment: ['Gym'],
  preferredTrainingDays: ['Monday', 'Tuesday'],
  injuriesOrLimitations: null,
  currentPain: null,
  averageSleepHours: 8,
  sleepQuality: 'Good',
  foodAllergies: null,
  foodIntolerances: null,
  preferredFoods: 'Chicken and rice',
  foodsToAvoid: null,
  typicalMealsPerDay: 4,
  typicalMealSchedule: 'Regular',
  currentSupplements: null,
  additionalNotes: null,
} satisfies OnboardingAssessmentForm;

describe('workflow API contracts', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('submits an athlete check-in and saves the coach review to that check-in', async () => {
    const checkIn = {
      weightKg: 80,
      waistCm: 82,
      sleepQuality: 8,
      energyLevel: 7,
      gutHealth: 9,
      trainingStress: 6,
    };
    mockedAxios.post.mockResolvedValueOnce({ data: { id: 41 } });
    mockedAxios.put.mockResolvedValueOnce({ data: { id: 41, coachNotes: 'Keep going.' } });

    await submitCheckIn(checkIn);
    await addCoachNotes(41, { notes: 'Keep going.' });

    expect(mockedAxios.post).toHaveBeenCalledWith('/checkins', checkIn);
    expect(mockedAxios.put).toHaveBeenCalledWith('/checkins/41/coach-notes', { notes: 'Keep going.' });
  });

  it('uploads each progress-photo angle as multipart data and reports progress', async () => {
    const front = new File(['front'], 'front.jpg', { type: 'image/jpeg' });
    const side = new File(['side'], 'side.png', { type: 'image/png' });
    const onProgress = vi.fn();
    mockedAxios.post.mockResolvedValueOnce({ data: { id: 41 } });

    await uploadPhotos(41, { Front: front, Side: side }, onProgress);

    expect(mockedAxios.post).toHaveBeenCalledWith(
      '/checkins/41/photos',
      expect.any(FormData),
      expect.objectContaining({ onUploadProgress: expect.any(Function) }),
    );
    const [, body, config] = mockedAxios.post.mock.calls[0] as [string, FormData, AxiosRequestConfig];
    expect(body.get('Front')).toBe(front);
    expect(body.get('Side')).toBe(side);
    expect(body.has('Back')).toBe(false);

    config.onUploadProgress?.({ loaded: 3, total: 4 } as AxiosProgressEvent);
    expect(onProgress).toHaveBeenCalledWith(75);
  });

  it('keeps signed private-photo URLs returned by the check-in endpoint', async () => {
    const photos = [{
      id: 1,
      angle: 'Front' as const,
      signedDownloadUrl: 'https://private.blob.example/front.jpg?sig=temporary',
      uploadedAt: '2026-07-18T12:00:00Z',
    }];
    mockedAxios.get.mockResolvedValueOnce({ data: photos });

    const result = await getCheckInPhotos(41);

    expect(mockedAxios.get).toHaveBeenCalledWith('/checkins/41/photos');
    expect(result[0]?.signedDownloadUrl).toContain('sig=temporary');
  });

  it('supports athlete onboarding draft, submission, coach review, and reopening', async () => {
    mockedAxios.put.mockResolvedValue({ data: { id: 5 } });
    mockedAxios.post.mockResolvedValue({ data: { id: 5 } });

    await saveMyOnboardingDraft(onboardingForm);
    await submitMyOnboardingAssessment(onboardingForm);
    await reviewAthleteOnboardingAssessment(9, { coachReviewNotes: 'Approved.' });
    await reopenAthleteOnboardingAssessment(9, { reason: 'Please update your injury details.' });

    expect(mockedAxios.put).toHaveBeenNthCalledWith(1, '/onboarding/me', onboardingForm);
    expect(mockedAxios.post).toHaveBeenCalledWith('/onboarding/me/submit', onboardingForm);
    expect(mockedAxios.put).toHaveBeenNthCalledWith(
      2,
      '/onboarding/athletes/9/review',
      { coachReviewNotes: 'Approved.' },
    );
    expect(mockedAxios.put).toHaveBeenNthCalledWith(
      3,
      '/onboarding/athletes/9/reopen',
      { reason: 'Please update your injury details.' },
    );
  });

  it('uploads onboarding photos to the onboarding-specific endpoint', async () => {
    const back = new File(['back'], 'back.jpg', { type: 'image/jpeg' });
    mockedAxios.post.mockResolvedValueOnce({ data: { id: 5 } });

    await uploadOnboardingPhotos({ Back: back });

    expect(mockedAxios.post).toHaveBeenCalledWith(
      '/onboarding/me/photos',
      expect.any(FormData),
      expect.objectContaining({ onUploadProgress: expect.any(Function) }),
    );
    const body = mockedAxios.post.mock.calls[0]?.[1] as FormData;
    expect(body.get('Back')).toBe(back);
  });

  it('logs individual food and a selected nutrition-plan option into the diary', async () => {
    const foodLog = {
      date: '2026-07-18',
      mealType: MealType.Breakfast,
      foodId: 14,
      quantityGrams: 120,
    };
    const planOption = {
      assignmentId: 3,
      mealBlockId: 12,
      mealOptionId: 24,
      date: '2026-07-18',
      mealType: MealType.Lunch,
      servings: 1,
      selectedAlternativeItemIds: [33],
    };
    mockedAxios.post.mockResolvedValue({ data: { id: 1 } });

    await logFood(foodLog);
    await logNutritionPlanOption(planOption);

    expect(mockedAxios.post).toHaveBeenNthCalledWith(1, '/diary/log', foodLog);
    expect(mockedAxios.post).toHaveBeenNthCalledWith(2, '/diary/log/nutrition-plan', planOption);
  });

  it('assigns a nutrition plan to the selected athlete with coach notes', async () => {
    mockedAxios.post.mockResolvedValueOnce({ data: { assignedCount: 1 } });

    await assignNutritionPlan(12, [9], 'Use the same 2200 calorie target every day.');

    expect(mockedAxios.post).toHaveBeenCalledWith('/nutrition-plans/12/assign', {
      athleteIds: [9],
      notes: 'Use the same 2200 calorie target every day.',
    });
  });

  it('logs a workout set and completes the same workout log', async () => {
    const set = { workoutLogId: 71, exerciseId: 8, setNumber: 2, weightKg: 90, reps: 10 };
    mockedAxios.post.mockResolvedValue({ data: { id: 1 } });

    await logSet(set);
    await completeWorkout({ workoutLogId: 71 });

    expect(mockedAxios.post).toHaveBeenNthCalledWith(1, '/workouts/log-set', set);
    expect(mockedAxios.post).toHaveBeenNthCalledWith(2, '/workouts/complete', { workoutLogId: 71 });
  });

  it('saves general coach feedback against the correct athlete', async () => {
    mockedAxios.post.mockResolvedValueOnce({ data: { id: 30 } });

    await saveFeedbackNote(9, { noteText: 'Increase water intake this week.' });

    expect(mockedAxios.post).toHaveBeenCalledWith('/coach-hub/athletes/9/notes', {
      noteText: 'Increase water intake this week.',
    });
  });
});
