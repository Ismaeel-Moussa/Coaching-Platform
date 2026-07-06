export type PhotoAngle = 'Front' | 'Side' | 'Back';

export interface CheckInPhotoDto {
  id: number;
  angle: PhotoAngle;
  signedDownloadUrl: string;
  uploadedAt: string;
}

export interface CheckInDto {
  id: number;
  athleteId: number;
  athleteFullName: string;
  weekOf: string;
  submittedAt: string;
  weightKg: number;
  waistCm: number | null;
  chestCm: number | null;
  thighCm: number | null;
  sleepQuality: number;
  energyLevel: number;
  gutHealth: number;
  trainingStress: number;
  coachNotes: string | null;
  coachReviewedAt: string | null;
  photos: CheckInPhotoDto[];
}

export interface PendingCheckInDto {
  athleteId: number;
  athleteFullName: string;
  profilePictureUrl: string | null;
  lastCheckInWeekOf: string | null;
  daysSinceLastCheckIn: number;
}

export interface SubmitCheckInForm {
  weightKg: number;
  waistCm?: number | null;
  chestCm?: number | null;
  thighCm?: number | null;
  sleepQuality: number;
  energyLevel: number;
  gutHealth: number;
  trainingStress: number;
}

export interface AddCoachNotesForm {
  notes: string;
}
