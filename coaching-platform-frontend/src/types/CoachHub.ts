// ── Coach Hub Types ─────────────────────────────────────────────────────────

export type WorkoutStatus = 'InProgress' | 'Completed' | 'Missed';
export type RosterStatus = 'Active' | 'ComplianceAlert' | 'NoRecentCheckIn';

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
  recentFeed: LiveFeedItemDto[];
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
