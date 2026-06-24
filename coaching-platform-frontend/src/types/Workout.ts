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
