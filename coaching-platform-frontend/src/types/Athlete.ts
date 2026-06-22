// ── Athlete Types ──────────────────────────────────────────────────────────────

export type TodaysWorkoutStatus = 'NoProgram' | 'InProgress' | 'Completed' | 'Missed';

export interface AthleteInfoDto {
  id: number;
  firstName: string;
  lastName: string;
  currentStreak: number;
  longestStreak: number;
  targetGoal: string;
  profilePictureUrl: string | null;
}

export interface MacroSummaryDto {
  date: string;
  caloriesConsumed: number;
  proteinConsumed: number;
  carbsConsumed: number;
  fatConsumed: number;
  targetCalories: number;
  targetProtein: number;
  targetCarbs: number;
  targetFat: number;
  caloriesRemaining: number;
  proteinRemaining: number;
  carbsRemaining: number;
  fatRemaining: number;
  waterLitersConsumed: number;
  waterLitersTarget: number;
  stepsWalked: number;
  stepsTarget: number;
}

export interface AthleteDashboardDto {
  athlete: AthleteInfoDto;
  today: MacroSummaryDto;
  todaysWorkoutStatus: TodaysWorkoutStatus;
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
