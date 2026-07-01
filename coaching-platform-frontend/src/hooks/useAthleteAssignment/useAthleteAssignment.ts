import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { message as antMessage } from 'antd';
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
      antMessage.success('Daily targets updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['coach-athlete-profile', athleteId] });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || 'Failed to update daily targets.';
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
      antMessage.success('Supplement schedule added successfully!');
      queryClient.invalidateQueries({ queryKey: ['athlete-supplements', athleteId] });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || 'Failed to add supplement schedule.';
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
      antMessage.success('Supplement schedule updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['athlete-supplements', athleteId] });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || 'Failed to update supplement schedule.';
      antMessage.error(msg);
    },
  });
};

export const useDeleteAthleteSupplement = (athleteId: number) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => deleteAthleteSupplement(id),
    onSuccess: () => {
      antMessage.success('Supplement schedule deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['athlete-supplements', athleteId] });
    },
    onError: (error: any) => {
      const msg = error.response?.data?.message || 'Failed to delete supplement schedule.';
      antMessage.error(msg);
    },
  });
};
