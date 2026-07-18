import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { AthleteProgressReportDto } from '../../../types/CoachHub';
import AthleteProgressReport from './AthleteProgressReport';

const { captureResponsiveContainer, getReport, downloadReport } = vi.hoisted(() => ({
  captureResponsiveContainer: vi.fn(),
  getReport: vi.fn(),
  downloadReport: vi.fn(),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: 'en' },
  }),
}));

vi.mock('../../../hooks/useCoachHub/useCoachHub', () => ({
  useGetAthleteProgressReport: getReport,
  useDownloadAthleteProgressReport: () => ({
    mutateAsync: downloadReport,
    isPending: false,
  }),
}));

vi.mock('../../../components/ProgressPhotoViewer/ProgressPhotoViewer', () => ({
  default: ({ photos }: { photos: Array<{ url?: string | null }> }) => (
    <div data-testid="progress-photo-viewer" data-photo-url={photos[0]?.url ?? ''} />
  ),
}));

vi.mock('recharts', () => ({
  ResponsiveContainer: (props: { children: ReactNode; minWidth?: number; minHeight?: number; initialDimension?: unknown }) => {
    captureResponsiveContainer(props);
    return <div data-testid="responsive-chart">{props.children}</div>;
  },
  LineChart: ({ children }: { children: ReactNode }) => <div>{children}</div>,
  CartesianGrid: () => null,
  Line: () => null,
  Tooltip: () => null,
  XAxis: () => null,
  YAxis: () => null,
}));

vi.mock('antd', () => {
  const Empty = Object.assign(() => <div>Empty</div>, { PRESENTED_IMAGE_SIMPLE: 'simple' });
  return {
    Alert: ({ message }: { message: ReactNode }) => <div>{message}</div>,
    Avatar: ({ children }: { children?: ReactNode }) => <div>{children}</div>,
    Breadcrumb: () => null,
    Button: ({ children, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) => <button {...props}>{children}</button>,
    Empty,
    Progress: () => null,
    Segmented: () => null,
    Skeleton: () => <div>Loading</div>,
    Switch: () => <input type="checkbox" />,
    message: { success: vi.fn(), error: vi.fn() },
  };
});

const signedPhotoUrl = 'https://private.blob.example/front.jpg?sig=temporary';

const report: AthleteProgressReportDto = {
  athleteId: 9,
  athleteName: 'Test Athlete',
  avatarUrl: null,
  targetGoal: 'Build muscle',
  heightCm: 180,
  periodStart: '2026-06-01',
  periodEnd: '2026-07-18',
  weeks: 8,
  generatedAt: '2026-07-18T12:00:00Z',
  summary: {
    startingWeightKg: 80,
    currentWeightKg: 81,
    weightChangeKg: 1,
    loggedWorkouts: 8,
    completedWorkouts: 7,
    workoutCompletionPercent: 87.5,
    nutritionTrackedDays: 30,
    averageCalorieAdherencePercent: 95,
    averageProteinAdherencePercent: 90,
    averageStepsAdherencePercent: 85,
    checkInCount: 1,
  },
  weeklyProgress: [{
    weekOf: '2026-07-13',
    weightKg: 81,
    loggedWorkouts: 4,
    completedWorkouts: 4,
    workoutCompletionPercent: 100,
    calorieAdherencePercent: 95,
    proteinAdherencePercent: 90,
    stepsAdherencePercent: 85,
    checkInSubmitted: true,
  }],
  checkIns: [],
  coachNotes: [],
  progressPhotos: [{ id: 1, weekOf: '2026-07-13', angle: 'Front', url: signedPhotoUrl }],
};

const renderPage = () => render(
  <MemoryRouter initialEntries={['/coach/athletes/9/progress-report']}>
    <Routes>
      <Route path="/coach/athletes/:athleteId/progress-report" element={<AthleteProgressReport />} />
    </Routes>
  </MemoryRouter>,
);

describe('AthleteProgressReport', () => {
  beforeEach(() => {
    captureResponsiveContainer.mockReset();
    getReport.mockReturnValue({ data: report, isLoading: false, isFetching: false, error: null });
  });

  it('gives the weight chart safe dimensions before browser layout is measured', () => {
    renderPage();

    expect(screen.getByTestId('responsive-chart')).toBeInTheDocument();
    expect(captureResponsiveContainer).toHaveBeenCalledWith(expect.objectContaining({
      minWidth: 0,
      minHeight: 240,
      initialDimension: { width: 800, height: 300 },
    }));
  });

  it('passes signed progress-photo URLs to the report photo viewer', () => {
    renderPage();

    expect(screen.getByTestId('progress-photo-viewer')).toHaveAttribute('data-photo-url', signedPhotoUrl);
  });
});
