import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import OnboardingGuard from './OnboardingGuard';
import { useMyOnboardingAssessment } from '../hooks/useOnboarding/useOnboarding';

vi.mock('../hooks/useOnboarding/useOnboarding', () => ({
  useMyOnboardingAssessment: vi.fn(),
}));

const mockedAssessment = vi.mocked(useMyOnboardingAssessment);

const renderGuard = (initialPath = '/athlete/dashboard') => render(
  <MemoryRouter initialEntries={[initialPath]}>
    <Routes>
      <Route path="/athlete/onboarding" element={<div>Onboarding page</div>} />
      <Route
        path="/athlete/dashboard"
        element={<OnboardingGuard><div>Athlete dashboard</div></OnboardingGuard>}
      />
    </Routes>
  </MemoryRouter>,
);

describe('OnboardingGuard', () => {
  beforeEach(() => {
    mockedAssessment.mockReset();
  });

  it('redirects athletes who must complete onboarding', () => {
    mockedAssessment.mockReturnValue({
      data: { requiresCompletion: true },
      isLoading: false,
      isError: false,
    } as ReturnType<typeof useMyOnboardingAssessment>);

    renderGuard();

    expect(screen.getByText('Onboarding page')).toBeInTheDocument();
  });

  it('allows athletes with completed onboarding to continue', () => {
    mockedAssessment.mockReturnValue({
      data: { requiresCompletion: false },
      isLoading: false,
      isError: false,
    } as ReturnType<typeof useMyOnboardingAssessment>);

    renderGuard();

    expect(screen.getByText('Athlete dashboard')).toBeInTheDocument();
  });

  it('does not lock athletes out when the assessment request fails', () => {
    mockedAssessment.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    } as ReturnType<typeof useMyOnboardingAssessment>);

    renderGuard();

    expect(screen.getByText('Athlete dashboard')).toBeInTheDocument();
  });
});
