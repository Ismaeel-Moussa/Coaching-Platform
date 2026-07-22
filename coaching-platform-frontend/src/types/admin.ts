export interface UserManagementDto {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  profilePictureUrl?: string | null;
  role: 'Admin' | 'Coach' | 'Athlete' | string;
  isActive: boolean;
  deactivationReason?: string | null;
  createdAt: string;
  lastLoginAt?: string | null;
  lastLoginIp?: string | null;
  assignedCoachId?: number | null;
  assignedCoachName?: string | null;
  assignedAthleteCount: number;
}

export interface UserMonitoringSummaryDto {
  totalUsersCount: number;
  activeCoachesCount: number;
  activeAthletesCount: number;
  deactivatedUsersCount: number;
  activeLast24hCount: number;
}

export interface CoachDeactivationImpactDto {
  coachId: number;
  coachName: string;
  assignedAthletesCount: number;
  activeNutritionPlansCount: number;
  activeWorkoutPlansCount: number;
}

export interface ToggleUserStatusForm {
  isActive: boolean;
  reason?: string | null;
  reassignCoachId?: number | null;
}

export interface UserFilterParams {
  search?: string;
  role?: string;
  isActive?: boolean;
  inactivityFilter?: 'all' | 'never' | '30days' | '24h' | string;
  pageNumber?: number;
  pageSize?: number;
}

export interface UserLoginAuditLogDto {
  id: number;
  action: string;
  performedByName?: string | null;
  ipAddress?: string | null;
  createdAt: string;
  details?: string | null;
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
