import axiosInstance from './axiosInstance';
import type {
  UserManagementDto,
  UserMonitoringSummaryDto,
  CoachDeactivationImpactDto,
  ToggleUserStatusForm,
  UserFilterParams,
  UserLoginAuditLogDto,
  PagedResult,
} from '../types/admin';

export const getUsers = async (
  params: UserFilterParams,
): Promise<PagedResult<UserManagementDto>> => {
  const response = await axiosInstance.get<PagedResult<UserManagementDto>>('/admin/users', {
    params,
  });
  return response.data;
};

export const getMonitoringSummary = async (): Promise<UserMonitoringSummaryDto> => {
  const response = await axiosInstance.get<UserMonitoringSummaryDto>('/admin/monitoring-summary');
  return response.data;
};

export const getCoachDeactivationImpact = async (
  coachId: number,
): Promise<CoachDeactivationImpactDto> => {
  const response = await axiosInstance.get<CoachDeactivationImpactDto>(
    `/admin/coaches/${coachId}/deactivation-impact`,
  );
  return response.data;
};

export const toggleUserStatus = async (
  userId: number,
  form: ToggleUserStatusForm,
): Promise<void> => {
  await axiosInstance.post(`/admin/users/${userId}/toggle-status`, form);
};

export const getUserLoginAuditLogs = async (
  userId: number,
): Promise<UserLoginAuditLogDto[]> => {
  const response = await axiosInstance.get<UserLoginAuditLogDto[]>(
    `/admin/users/${userId}/audit-logs`,
  );
  return response.data;
};

export const exportUserAuditLogsCsv = async (
  params: UserFilterParams,
): Promise<Blob> => {
  const response = await axiosInstance.get('/admin/users/export-audit-csv', {
    params,
    responseType: 'blob',
  });
  return response.data;
};
