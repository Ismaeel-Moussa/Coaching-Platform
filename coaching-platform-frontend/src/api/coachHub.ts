import axiosInstance from './axiosInstance';
import type {
  CoachDashboardDto,
  LiveFeedItemDto,
  ComplianceItemDto,
  RosterItemDto,
  AthleteDeepProfileDto,
  CoachFeedbackNoteDto,
  WeightHistoryPointDto,
  SaveFeedbackNoteForm,
  PagedResult,
  CoachActionItemDto,
  CoachActionPriority,
  CoachActionType,
  AthleteProgressReportDto,
  ProgressReportOptions,
  ProgressReportPreviewOptions,
} from '../types/CoachHub';

export const getCoachDashboard = async (): Promise<CoachDashboardDto> => {
  const response = await axiosInstance.get<CoachDashboardDto>('/coach-hub/dashboard');
  return response.data;
};

export interface CoachActionItemsQuery {
  page?: number;
  pageSize?: number;
  type?: CoachActionType;
  priority?: CoachActionPriority;
  search?: string;
}

export const getCoachActionItems = async (
  query: CoachActionItemsQuery,
): Promise<PagedResult<CoachActionItemDto>> => {
  const response = await axiosInstance.get<PagedResult<CoachActionItemDto>>('/coach-hub/action-items', {
    params: query,
  });
  return response.data;
};

export const getLiveFeed = async (
  page: number = 1,
  pageSize: number = 20
): Promise<PagedResult<LiveFeedItemDto>> => {
  const response = await axiosInstance.get<PagedResult<LiveFeedItemDto>>('/coach-hub/live-feed', {
    params: { page, pageSize },
  });
  return response.data;
};

export const getCompliance = async (): Promise<ComplianceItemDto[]> => {
  const response = await axiosInstance.get<ComplianceItemDto[]>('/coach-hub/compliance');
  return response.data;
};

export const getRoster = async (
  page: number = 1,
  pageSize: number = 20,
  filter?: string | null
): Promise<PagedResult<RosterItemDto>> => {
  const response = await axiosInstance.get<PagedResult<RosterItemDto>>('/coach-hub/roster', {
    params: { page, pageSize, filter },
  });
  return response.data;
};

export const getAthleteProfile = async (id: number): Promise<AthleteDeepProfileDto> => {
  const response = await axiosInstance.get<AthleteDeepProfileDto>(`/coach-hub/athletes/${id}`);
  return response.data;
};

export const saveFeedbackNote = async (
  id: number,
  form: SaveFeedbackNoteForm
): Promise<CoachFeedbackNoteDto> => {
  const response = await axiosInstance.post<CoachFeedbackNoteDto>(
    `/coach-hub/athletes/${id}/notes`,
    form
  );
  return response.data;
};

export const getWeightHistory = async (id: number): Promise<WeightHistoryPointDto[]> => {
  const response = await axiosInstance.get<WeightHistoryPointDto[]>(
    `/coach-hub/athletes/${id}/weight-history`
  );
  return response.data;
};

export const getAthleteProgressReport = async (
  id: number,
  options: ProgressReportPreviewOptions,
): Promise<AthleteProgressReportDto> => {
  const response = await axiosInstance.get<AthleteProgressReportDto>(
    `/coach-hub/athletes/${id}/progress-report`,
    { params: options },
  );
  return response.data;
};

export const downloadAthleteProgressReportPdf = async (
  id: number,
  options: ProgressReportOptions,
): Promise<Blob> => {
  const response = await axiosInstance.get(
    `/coach-hub/athletes/${id}/progress-report/pdf`,
    { params: options, responseType: 'blob' },
  );
  return response.data;
};
