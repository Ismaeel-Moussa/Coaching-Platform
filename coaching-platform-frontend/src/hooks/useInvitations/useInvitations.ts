import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import {
  getInvitations,
  createInvitation,
  resendInvitation,
  revokeInvitation,
} from '../../api/invitation';
import type { CreateInvitationForm, InvitationDto, PagedInvitationsResult } from '../../types/Invitation';

// ── useGetInvitations ─────────────────────────────────────────────────────────
export const useGetInvitations = (page = 1, pageSize = 10) =>
  useQuery<PagedInvitationsResult, AxiosError>({
    queryKey: ['invitations', page, pageSize],
    queryFn: () => getInvitations(page, pageSize),
  });

// ── useCreateInvitation ───────────────────────────────────────────────────────
export const useCreateInvitation = () => {
  const queryClient = useQueryClient();

  return useMutation<InvitationDto, AxiosError, CreateInvitationForm>({
    mutationFn: createInvitation,
    onSuccess: (data) => {
      antMessage.success(`Invitation sent to ${data.email}`);
      queryClient.invalidateQueries({ queryKey: ['invitations'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to send invitation.';
      antMessage.error(msg);
    },
  });
};

// ── useResendInvitation ───────────────────────────────────────────────────────
export const useResendInvitation = () => {
  const queryClient = useQueryClient();

  return useMutation<InvitationDto, AxiosError, number>({
    mutationFn: resendInvitation,
    onSuccess: (data) => {
      antMessage.success(`Invitation resent to ${data.email}`);
      queryClient.invalidateQueries({ queryKey: ['invitations'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to resend invitation.';
      antMessage.error(msg);
    },
  });
};

// ── useRevokeInvitation ───────────────────────────────────────────────────────
export const useRevokeInvitation = () => {
  const queryClient = useQueryClient();

  return useMutation<void, AxiosError, number>({
    mutationFn: revokeInvitation,
    onSuccess: () => {
      antMessage.success('Invitation revoked.');
      queryClient.invalidateQueries({ queryKey: ['invitations'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to revoke invitation.';
      antMessage.error(msg);
    },
  });
};
