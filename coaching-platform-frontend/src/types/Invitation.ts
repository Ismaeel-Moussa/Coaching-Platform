export type InvitationRole = 'Athlete' | 'Coach' | 'Admin';

// Enum matches backend C# enum values (handles both string from API and number from legacy/mock)
export const InvitationStatus = {
  Pending: 'Pending',
  Accepted: 'Accepted',
  Expired: 'Expired',
  Revoked: 'Revoked',
} as const;

export type InvitationStatusValue = 
  | typeof InvitationStatus[keyof typeof InvitationStatus]
  | 0 | 1 | 2 | 3;

export const InvitationStatusLabel: Record<InvitationStatusValue, string> = {
  [InvitationStatus.Pending]: 'Pending',
  [InvitationStatus.Accepted]: 'Accepted',
  [InvitationStatus.Expired]: 'Expired',
  [InvitationStatus.Revoked]: 'Revoked',
  0: 'Pending',
  1: 'Accepted',
  2: 'Expired',
  3: 'Revoked',
};

export interface InvitationDto {
  id: number;
  email: string;
  token: string;
  role: InvitationRole;
  status: InvitationStatusValue;
  expiresAt: string;
  createdAt: string;
  inviteUrl: string;
}

export interface CreateInvitationForm {
  email: string;
  role: InvitationRole;
  expiryHours?: number;
}

export interface PagedInvitationsResult {
  items: InvitationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}
