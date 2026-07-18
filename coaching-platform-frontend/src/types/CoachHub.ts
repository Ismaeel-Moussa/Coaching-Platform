// ── Coach Hub Types ─────────────────────────────────────────────────────────

export type WorkoutStatus = 'InProgress' | 'Completed' | 'Missed';
export type RosterStatus = 'Active' | 'ComplianceAlert' | 'NoRecentCheckIn';
export type OnboardingDisplayStatus = 'NotStarted' | 'Draft' | 'Submitted' | 'Reviewed' | 'ChangesRequested';

export interface LiveFeedItemDto {
  athleteId: number;
  athleteName: string;
  athleteAvatarUrl: string | null;
  workoutDayLabel: string;
  status: WorkoutStatus;
  completedAt: string | null;
  date: string;
}

export interface CoachDashboardDto {
  activeAthleteCount: number;
  avgWorkoutCompletionPercent: number;
  pendingCheckInsCount: number;
  pendingOnboardingAssessmentsCount: number;
  athletesNeedingSetupCount: number;
  actionItems: CoachActionItemDto[];
  recentFeed: LiveFeedItemDto[];
}

export type CoachActionType = 'AssessmentReview' | 'SetupRequired' | 'CheckInPending' | 'ComplianceAlert';
export type CoachActionPriority = 'High' | 'Medium';

export interface CoachActionItemDto {
  athleteId: number;
  athleteName: string;
  athleteAvatarUrl: string | null;
  type: CoachActionType;
  priority: CoachActionPriority;
  progressCurrent: number | null;
  progressTotal: number | null;
  metricValue: number | null;
}

export interface AthleteSetupReadinessDto {
  isComplete: boolean;
  completedRequiredSteps: number;
  totalRequiredSteps: number;
  assessmentReviewed: boolean;
  workoutAssigned: boolean;
  nutritionPlanAssigned: boolean;
  nutritionTargetsConfigured: boolean;
  activityTargetsConfigured: boolean;
}

export interface ComplianceItemDto {
  athleteId: number;
  athleteName: string;
  athleteAvatarUrl: string | null;
  targetCalories: number;
  consumedCalories: number;
  targetProtein: number;
  consumedProtein: number;
  targetCarbs: number;
  consumedCarbs: number;
  targetFat: number;
  consumedFat: number;
  isOverCalorieTarget: boolean;
  compliancePercent: number;
}

export interface RosterItemDto {
  athleteId: number;
  athleteName: string;
  athleteAvatarUrl: string | null;
  activeProgramName: string | null;
  macroCompliancePercent: number;
  lastCheckInDate: string | null;
  onboardingStatus: OnboardingDisplayStatus;
  onboardingSubmittedAt: string | null;
  setupReadiness?: AthleteSetupReadinessDto;
  status: RosterStatus;
}

export interface MacroTargetDto {
  id: number;
  targetCalories: number;
  targetProtein: number;
  targetCarbs: number;
  targetFat: number;
  waterLitersTarget: number;
  stepsTarget: number;
  setAt: string;
  setByCoachName: string;
}

export interface WeightHistoryPointDto {
  weekOf: string;
  weightKg: number;
}

export interface CoachFeedbackNoteDto {
  id: number;
  noteText: string;
  coachName: string;
  createdAt: string;
  type?: 'General' | 'CheckIn';
  weekOf?: string;
}

export interface AthleteDeepProfileDto {
  id: number;
  fullName: string;
  avatarUrl: string | null;
  targetGoal: string;
  weightKg: number;
  heightCm: number;
  currentStreak: number;
  longestStreak: number;
  currentTargets: MacroTargetDto | null;
  setupReadiness?: AthleteSetupReadinessDto;
  weightHistory: WeightHistoryPointDto[];
  feedbackNotes: CoachFeedbackNoteDto[];
}

export interface SaveFeedbackNoteForm {
  noteText: string;
}

export interface ProgressReportSummaryDto {
  startingWeightKg: number | null;
  currentWeightKg: number | null;
  weightChangeKg: number | null;
  loggedWorkouts: number;
  completedWorkouts: number;
  workoutCompletionPercent: number | null;
  nutritionTrackedDays: number;
  averageCalorieAdherencePercent: number | null;
  averageProteinAdherencePercent: number | null;
  averageStepsAdherencePercent: number | null;
  checkInCount: number;
}

export interface ProgressReportWeekDto {
  weekOf: string;
  weightKg: number | null;
  loggedWorkouts: number;
  completedWorkouts: number;
  workoutCompletionPercent: number | null;
  calorieAdherencePercent: number | null;
  proteinAdherencePercent: number | null;
  stepsAdherencePercent: number | null;
  checkInSubmitted: boolean;
}

export interface ProgressReportCheckInDto {
  id: number;
  weekOf: string;
  weightKg: number;
  waistCm: number | null;
  chestCm: number | null;
  thighCm: number | null;
  sleepQuality: number;
  energyLevel: number;
  gutHealth: number;
  trainingStress: number;
  reviewNotes: string | null;
  reviewedAt: string | null;
}

export interface ProgressReportNoteDto {
  id: number;
  text: string;
  coachName: string;
  createdAt: string;
}

export interface ProgressReportPhotoDto {
  id: number;
  weekOf: string;
  angle: string;
  url: string;
}

export interface AthleteProgressReportDto {
  athleteId: number;
  athleteName: string;
  avatarUrl: string | null;
  targetGoal: string | null;
  heightCm: number | null;
  periodStart: string;
  periodEnd: string;
  weeks: number;
  generatedAt: string;
  summary: ProgressReportSummaryDto;
  weeklyProgress: ProgressReportWeekDto[];
  checkIns: ProgressReportCheckInDto[];
  coachNotes: ProgressReportNoteDto[];
  progressPhotos: ProgressReportPhotoDto[];
}

export interface ProgressReportOptions {
  weeks: 4 | 8 | 12;
  includeCoachNotes: boolean;
  includePhotos: boolean;
  language: 'ar' | 'en';
}

export type ProgressReportPreviewOptions = Pick<ProgressReportOptions, 'weeks' | 'includeCoachNotes' | 'includePhotos'>;

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
