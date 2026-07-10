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

  return useMutation<void, AxiosError, number, { previousQueries: { queryKey: any; data: any }[] }>({
    mutationFn: revokeInvitation,
    onMutate: async (revokedId) => {
      await queryClient.cancelQueries({ queryKey: ['invitations'] });
      const queries = queryClient.getQueriesData<any>({ queryKey: ['invitations'] });
      const previousQueries = queries.map(([queryKey, data]) => ({ queryKey, data }));

      queries.forEach(([queryKey, data]) => {
        if (data && Array.isArray(data.items)) {
          queryClient.setQueryData(queryKey, {
            ...data,
            items: data.items.filter((item: any) => item.id !== revokedId),
            totalCount: Math.max(0, data.totalCount - 1),
          });
        }
      });

      return { previousQueries };
    },
    onError: (error, revokedId, context) => {
      context?.previousQueries?.forEach(({ queryKey, data }) => {
        queryClient.setQueryData(queryKey, data);
      });
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Failed to revoke invitation.';
      antMessage.error(msg);
    },
    onSuccess: () => {
      antMessage.success('Invitation revoked.');
      queryClient.invalidateQueries({ queryKey: ['invitations'] });
    },
  });
};
