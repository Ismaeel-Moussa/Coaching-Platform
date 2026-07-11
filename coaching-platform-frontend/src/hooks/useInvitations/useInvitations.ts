import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import i18n from '../../i18n/i18n';
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
      antMessage.success(i18n.t('common:alerts.invitationSent', { email: data.email }));
      queryClient.invalidateQueries({ queryKey: ['invitations'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
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
      antMessage.success(i18n.t('common:alerts.invitationResent', { email: data.email }));
      queryClient.invalidateQueries({ queryKey: ['invitations'] });
    },
    onError: (error) => {
      const msg =
        (error.response?.data as { message?: string })?.message ??
        i18n.t('common:alerts.genericError');
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
        i18n.t('common:alerts.genericError');
      antMessage.error(msg);
    },
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.invitationRevoked'));
      queryClient.invalidateQueries({ queryKey: ['invitations'] });
    },
  });
};
