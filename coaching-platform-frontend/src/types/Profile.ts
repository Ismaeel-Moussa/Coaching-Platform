import type { UserRole } from './auth';

export interface UserProfileDto {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  profilePictureUrl: string | null;

  // Coach details
  bio?: string | null;

  // Athlete details
  weightKg?: number | null;
  heightCm?: number | null;
  targetGoal?: string | null;
  currentStreak?: number | null;
  longestStreak?: number | null;
  assignedCoachName?: string | null;
}

export interface UpdateProfileForm {
  firstName: string;
  lastName: string;
  profilePictureUrl?: string | null;
  bio?: string | null;
  weightKg?: number | null;
  heightCm?: number | null;
  targetGoal?: string | null;
}

export interface ChangePasswordForm {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
