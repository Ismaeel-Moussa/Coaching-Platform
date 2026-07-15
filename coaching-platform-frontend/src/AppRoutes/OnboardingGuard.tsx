import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useMyOnboardingAssessment } from '../hooks/useOnboarding/useOnboarding';

interface OnboardingGuardProps { children: ReactNode }

const OnboardingGuard = ({ children }: OnboardingGuardProps) => {
  const location = useLocation();
  const { data, isLoading, isError } = useMyOnboardingAssessment();
  const isOnboardingPage = location.pathname === '/athlete/onboarding';

  if (isLoading) {
    return <div className="page-loader"><div className="page-loader__spinner" /></div>;
  }

  if (!isError && data?.requiresCompletion && !isOnboardingPage) {
    return <Navigate to="/athlete/onboarding" replace state={{ from: location.pathname }} />;
  }

  return children;
};

export default OnboardingGuard;
