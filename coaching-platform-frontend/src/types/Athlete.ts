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

import type { CoachFeedbackNoteDto } from './CoachHub';

export interface AthleteDashboardDto {
  athlete: AthleteInfoDto;
  today: MacroSummaryDto;
  todaysWorkoutStatus: TodaysWorkoutStatus;
  recentFeedbackNotes?: CoachFeedbackNoteDto[];
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

import type { TodaysWorkoutDto } from './Workout';
import type { DailyDiaryDto, MealType } from './Diary';
import type { SupplementDto } from './Supplement';

export type NutritionPlanDayType = 'Training' | 'Rest' | 'AllDays' | 'Unspecified';
export type NutritionPlanBlockStatus = 'Completed' | 'Missed' | 'Pending' | 'NotTracked';

export interface NutritionPlanBlockAdherenceDto {
  mealBlockId: number;
  orderIndex: number;
  label: string;
  labelAr: string | null;
  targetCalories: number | null;
  status: NutritionPlanBlockStatus;
  mealOptionId: number | null;
  optionLabel: string | null;
  optionLabelAr: string | null;
  loggedMealType: MealType | null;
  servings: number | null;
  loggedAt: string | null;
}

export interface NutritionPlanAdherenceDto {
  assignmentId: number;
  planName: string;
  planNameAr: string | null;
  dayType: NutritionPlanDayType;
  isPartialDay: boolean;
  completedBlocks: number;
  totalBlocks: number;
  completionPercent: number;
  blocks: NutritionPlanBlockAdherenceDto[];
}

export interface DailyLogHistoryDto {
  date: string;
  workout: TodaysWorkoutDto | null;
  nutrition: DailyDiaryDto | null;
  nutritionPlanAdherences: NutritionPlanAdherenceDto[];
  supplements: SupplementDto[];
}
