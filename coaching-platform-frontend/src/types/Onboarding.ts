export type OnboardingAssessmentStatus = 'NotStarted' | 'Draft' | 'Submitted' | 'Reviewed';

export interface OnboardingAssessmentForm {
  primaryGoal: string | null;
  weightKg: number | null;
  heightCm: number | null;
  activityLevel: string | null;
  trainingExperience: string | null;
  trainingDaysPerWeek: number | null;
  availableEquipment: string[];
  preferredTrainingDays: string[];
  injuriesOrLimitations: string | null;
  currentPain: string | null;
  averageSleepHours: number | null;
  sleepQuality: string | null;
  foodAllergies: string | null;
  foodIntolerances: string | null;
  preferredFoods: string | null;
  foodsToAvoid: string | null;
  typicalMealsPerDay: number | null;
  typicalMealSchedule: string | null;
  currentSupplements: string | null;
  additionalNotes: string | null;
}

export interface OnboardingAssessmentDto extends OnboardingAssessmentForm {
  id: number | null;
  athleteId: number;
  athleteName: string;
  status: OnboardingAssessmentStatus;
  coachReviewNotes: string | null;
  submittedAt: string | null;
  reviewedAt: string | null;
  updatedAt: string | null;
  hasInjuryFlag: boolean;
  hasPainFlag: boolean;
  hasAllergyFlag: boolean;
  hasFoodRestrictionFlag: boolean;
  requiresCompletion: boolean;
  photos: OnboardingPhotoDto[];
}

export interface OnboardingPhotoDto {
  id: number;
  angle: 'Front' | 'Side' | 'Back';
  signedDownloadUrl: string;
  uploadedAt: string;
}

export interface ReviewOnboardingAssessmentForm {
  coachReviewNotes: string | null;
}

