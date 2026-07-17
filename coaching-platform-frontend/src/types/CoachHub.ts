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

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
