// ── Workout Types ─────────────────────────────────────────────────────────────

export enum WorkoutStatus {
  InProgress = 'InProgress',
  Completed = 'Completed',
  Missed = 'Missed',
  NoProgram = 'NoProgram',
}

export enum ExerciseSection {
  WarmUp = 'WarmUp',
  Main = 'Main',
  CoolDown = 'CoolDown',
}

export interface ExerciseDto {
  id: number;
  name: string;
  instructions: string;
  primaryMuscle: string;
  equipmentRequired: string;
  youTubeVideoId: string | null;
}

export interface TemplateExerciseDto {
  id: number;
  exercise: ExerciseDto;
  section: ExerciseSection;
  orderIndex: number;
  targetSets: number;
  targetReps: string;
  restSeconds: number | null;
  isSupersetWith: boolean;
  progressiveOverloadTargetKg: number | null;
}

export interface WorkoutDayDto {
  dayNumber: number;
  dayLabel: string;
  isRestDay: boolean;
  warmUp: TemplateExerciseDto[];
  main: TemplateExerciseDto[];
  coolDown: TemplateExerciseDto[];
}

export interface SetLogDto {
  id: number;
  exerciseId: number;
  exerciseName: string;
  setNumber: number;
  weightKg: number;
  reps: number;
  isCompleted: boolean;
}

export interface TodaysWorkoutDto {
  workoutLogId: number;
  status: WorkoutStatus;
  completedAt?: string | null;
  day: WorkoutDayDto | null;
  loggedSets: SetLogDto[];
}

export interface WorkoutProgramDto {
  templateId: number;
  templateName: string;
  description: string;
  startDate: string;
  days: WorkoutDayDto[];
}

export interface WorkoutSessionDto {
  date: string;
  sets: SetLogDto[];
}

export interface WorkoutHistoryDto {
  exerciseId: number;
  exerciseName: string;
  sessions: WorkoutSessionDto[];
}

// ── Request Forms ─────────────────────────────────────────────────────────────

export interface LogSetForm {
  workoutLogId: number;
  exerciseId: number;
  setNumber: number;
  weightKg: number;
  reps: number;
}

export interface CompleteWorkoutForm {
  workoutLogId: number;
}

// ── Day 5 Workout Template Builder Types ─────────────────────────────────────

export interface WorkoutTemplateSummaryDto {
  id: number;
  name: string;
  description: string | null;
  coachName: string;
  dayCount: number;
  isActive: boolean;
  createdAt: string;
}

export interface WorkoutTemplateExerciseDto {
  id?: number;
  exerciseId: number;
  exerciseName?: string;
  exercise?: ExerciseDto;
  youTubeVideoId?: string | null;
  section: 'WarmUp' | 'Main' | 'CoolDown';
  orderIndex: number;
  targetSets: number;
  targetReps: string;
  restSeconds?: number | null;
  progressiveOverloadTargetKg?: number | null;
}

export interface WorkoutTemplateDayDto {
  id?: number;
  dayNumber: number;
  dayLabel: string;
  isRestDay: boolean;
  exercises: WorkoutTemplateExerciseDto[];
}

export interface WorkoutTemplateDto {
  id: number;
  name: string;
  description: string | null;
  coachName: string;
  isActive: boolean;
  createdAt: string;
  days: WorkoutTemplateDayDto[];
}

export interface CreateWorkoutTemplateForm {
  name: string;
  description?: string;
  days: {
    dayNumber: number;
    dayLabel: string;
    isRestDay: boolean;
    exercises: {
      exerciseId: number;
      section: 'WarmUp' | 'Main' | 'CoolDown';
      orderIndex: number;
      targetSets: number;
      targetReps: string;
      restSeconds?: number | null;
      progressiveOverloadTargetKg?: number | null;
    }[];
  }[];
}

export interface AssignTemplateForm {
  athleteIds: number[];
}

