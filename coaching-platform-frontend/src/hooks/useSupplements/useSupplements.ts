import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import i18n from '../../i18n/i18n';
import { getSupplements, toggleSupplement } from '../../api/supplement';
import type { ToggleSupplementForm } from '../../types/Supplement';

// ── Queries ───────────────────────────────────────────────────────────────────

export const useGetSupplements = () =>
  useQuery({
    queryKey: ['supplements'],
    queryFn: getSupplements,
  });

// ── Mutations ─────────────────────────────────────────────────────────────────

export const useToggleSupplement = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: ToggleSupplementForm) => toggleSupplement(form),
    onMutate: async (variables) => {
      await queryClient.cancelQueries({ queryKey: ['supplements'] });
      const previousSupplements = queryClient.getQueryData<any[]>(['supplements']);

      if (previousSupplements) {
        queryClient.setQueryData(
          ['supplements'],
          previousSupplements.map((s) => {
            if (s.id === variables.supplementScheduleId) {
              return {
                ...s,
                isTaken: !s.isTaken,
                takenAt: !s.isTaken ? new Date().toISOString() : null,
              };
            }
            return s;
          }),
        );
      }

      return { previousSupplements };
    },
    onError: (err, variables, context) => {
      if (context?.previousSupplements) {
        queryClient.setQueryData(['supplements'], context.previousSupplements);
      }
      antMessage.error(i18n.t('common:alerts.supplementToggleFailed'));
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supplements'] });
    },
  });
};
