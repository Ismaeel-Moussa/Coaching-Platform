// ── Exercise Types ─────────────────────────────────────────────────────────────

export type MuscleGroup = 'Chest' | 'Back' | 'Shoulders' | 'Arms' | 'Legs' | 'Cardio' | 'Core';

export interface ExerciseAdminDto {
  id: number;
  name: string;
  primaryMuscle: MuscleGroup;
  equipmentRequired: string | null;
  instructions: string | null;
  youTubeVideoId: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateExerciseForm {
  name: string;
  primaryMuscle: MuscleGroup;
  equipmentRequired?: string;
  instructions?: string;
  youTubeVideoId?: string;
}

export interface UpdateExerciseForm {
  name?: string;
  primaryMuscle?: MuscleGroup;
  equipmentRequired?: string;
  instructions?: string;
  youTubeVideoId?: string;
}

export interface PagedExerciseResult {
  items: ExerciseAdminDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}
