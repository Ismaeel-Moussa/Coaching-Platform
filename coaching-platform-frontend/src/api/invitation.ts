import axiosInstance from './axiosInstance';
import type {
  InvitationDto,
  CreateInvitationForm,
  PagedInvitationsResult,
} from '../types/Invitation';

export const getInvitations = async (
  page = 1,
  pageSize = 10,
): Promise<PagedInvitationsResult> => {
  const response = await axiosInstance.get<PagedInvitationsResult>('/invitations', {
    params: { page, pageSize },
  });
  return response.data;
};

export const createInvitation = async (form: CreateInvitationForm): Promise<InvitationDto> => {
  const response = await axiosInstance.post<InvitationDto>('/invitations', form);
  return response.data;
};

export const resendInvitation = async (id: number): Promise<InvitationDto> => {
  const response = await axiosInstance.post<InvitationDto>(`/invitations/resend/${id}`);
  return response.data;
};

export const revokeInvitation = async (id: number): Promise<void> => {
  await axiosInstance.delete(`/invitations/${id}`);
};
