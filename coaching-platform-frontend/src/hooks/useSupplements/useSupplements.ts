import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
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
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['supplements'] });
    },
    onError: () => antMessage.error('Failed to update supplement. Please try again.'),
  });
};
