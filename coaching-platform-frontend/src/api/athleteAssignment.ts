import axiosInstance from './axiosInstance';
import type { SupplementDto } from '../types/Supplement';
import type { MacroTargetDto } from '../types/Athlete';

export interface SetMacroTargetsForm {
  targetCalories: number;
  targetProtein: number;
  targetCarbs: number;
  targetFat: number;
  waterLitersTarget: number;
  stepsTarget: number;
}

export interface AddSupplementForm {
  name: string;
  type: 'Essential' | 'Optional';
  dosage: string | null;
  notes: string | null;
}

export interface UpdateSupplementForm {
  name: string;
  type: 'Essential' | 'Optional';
  dosage: string | null;
  notes: string | null;
}

// ─── Macro targets APIs ──────────────────────────────────────────────
export const setMacroTargets = async (athleteId: number, form: SetMacroTargetsForm): Promise<void> => {
  await axiosInstance.put(`/coach-hub/athletes/${athleteId}/targets`, form);
};

// ─── Supplements APIs ───────────────────────────────────────────────
export const getAthleteSupplements = async (athleteId: number): Promise<SupplementDto[]> => {
  const response = await axiosInstance.get<SupplementDto[]>(`/supplements/athlete/${athleteId}`);
  return response.data;
};

export const addAthleteSupplement = async (athleteId: number, form: AddSupplementForm): Promise<SupplementDto> => {
  // We can use the existing POST /api/supplements/schedule endpoint by passing AthleteId in the form
  const response = await axiosInstance.post<SupplementDto>('/supplements/schedule', {
    athleteId,
    ...form,
  });
  return response.data;
};

export const updateAthleteSupplement = async (supplementId: number, form: UpdateSupplementForm): Promise<SupplementDto> => {
  const response = await axiosInstance.put<SupplementDto>(`/supplements/${supplementId}`, form);
  return response.data;
};

export const deleteAthleteSupplement = async (supplementId: number): Promise<void> => {
  await axiosInstance.delete(`/supplements/${supplementId}`);
};
