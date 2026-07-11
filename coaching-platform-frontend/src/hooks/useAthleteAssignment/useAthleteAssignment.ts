import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
import i18n from '../../i18n/i18n';
import {
  setMacroTargets,
  getAthleteSupplements,
  addAthleteSupplement,
  updateAthleteSupplement,
  deleteAthleteSupplement,
  type SetMacroTargetsForm,
  type AddSupplementForm,
  type UpdateSupplementForm,
} from '../../api/athleteAssignment';

export const useSetMacroTargets = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: SetMacroTargetsForm) => setMacroTargets(athleteId, form),
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.targetsUpdated'));
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile', athleteId] });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || i18n.t('common:alerts.targetsUpdateFailed');
      antMessage.error(msg);
    },
  });
};

export const useGetAthleteSupplements = (athleteId: number) => {
  return useQuery({
    queryKey: ['athlete-supplements', athleteId],
    queryFn: () => getAthleteSupplements(athleteId),
    enabled: !!athleteId && !isNaN(athleteId),
  });
};

export const useAddAthleteSupplement = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: AddSupplementForm) => addAthleteSupplement(athleteId, form),
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.suppScheduleAdded'));
      queryClient.invalidateQueries({ queryKey: ['athlete-supplements', athleteId] });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || i18n.t('common:alerts.suppScheduleAddFailed');
      antMessage.error(msg);
    },
  });
};

export const useUpdateAthleteSupplement = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: number; form: UpdateSupplementForm }) =>
      updateAthleteSupplement(id, form),
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.suppScheduleUpdated'));
      queryClient.invalidateQueries({ queryKey: ['athlete-supplements', athleteId] });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || i18n.t('common:alerts.suppScheduleUpdateFailed');
      antMessage.error(msg);
    },
  });
};

export const useDeleteAthleteSupplement = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => deleteAthleteSupplement(id),
    onMutate: async (deletedId) => {
      await queryClient.cancelQueries({ queryKey: ['athlete-supplements', athleteId] });
      const previousSupplements = queryClient.getQueryData<any[]>(['athlete-supplements', athleteId]);

      if (previousSupplements) {
        queryClient.setQueryData(
          ['athlete-supplements', athleteId],
          previousSupplements.filter((s) => s.id !== deletedId),
        );
      }

      return { previousSupplements };
    },
    onError: (error: any, deletedId, context) => {
      if (context?.previousSupplements) {
        queryClient.setQueryData(['athlete-supplements', athleteId], context.previousSupplements);
      }
      const msg = error.response?.data?.message || i18n.t('common:alerts.suppScheduleDeleteFailed');
      antMessage.error(msg);
    },
    onSuccess: () => {
      antMessage.success(i18n.t('common:alerts.suppScheduleDeleted'));
      queryClient.invalidateQueries({ queryKey: ['athlete-supplements', athleteId] });
    },
  });
};
