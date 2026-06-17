export type InvitationRole = 'Athlete' | 'Coach' | 'Admin';

// Enum matches backend C# enum integer values
export const InvitationStatus = {
  Pending: 0,
  Accepted: 1,
  Expired: 2,
  Revoked: 3,
} as const;

export type InvitationStatusValue = (typeof InvitationStatus)[keyof typeof InvitationStatus];

export const InvitationStatusLabel: Record<InvitationStatusValue, string> = {
  [InvitationStatus.Pending]: 'Pending',
  [InvitationStatus.Accepted]: 'Accepted',
  [InvitationStatus.Expired]: 'Expired',
  [InvitationStatus.Revoked]: 'Revoked',
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
