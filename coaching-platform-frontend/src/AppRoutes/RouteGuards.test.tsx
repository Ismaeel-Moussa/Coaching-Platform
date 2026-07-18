import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ProtectedRoute from './ProtectedRoute';
import RoleGuard from './RoleGuard';
import { getCurrentUserRoles } from '../utils/getCurrentUserRoles';

vi.mock('../utils/getCurrentUserRoles', () => ({
  getCurrentUserRoles: vi.fn(),
}));

const mockedGetCurrentUserRoles = vi.mocked(getCurrentUserRoles);

const renderProtectedRoute = () => render(
  <MemoryRouter initialEntries={['/private']}>
    <Routes>
      <Route path="/sign-in" element={<div>Sign in page</div>} />
      <Route
        path="/private"
        element={<ProtectedRoute><div>Private page</div></ProtectedRoute>}
      />
    </Routes>
  </MemoryRouter>,
);

const renderRoleGuard = (allowedRoles: string[]) => render(
  <MemoryRouter initialEntries={['/restricted']}>
    <Routes>
      <Route path="/athlete/dashboard" element={<div>Athlete dashboard</div>} />
      <Route path="/coach/dashboard" element={<div>Coach dashboard</div>} />
      <Route
        path="/restricted"
        element={<RoleGuard allowedRoles={allowedRoles}><div>Restricted page</div></RoleGuard>}
      />
    </Routes>
  </MemoryRouter>,
);

describe('route guards', () => {
  beforeEach(() => {
    localStorage.clear();
    mockedGetCurrentUserRoles.mockReset();
  });

  it('redirects unauthenticated visitors to sign in', () => {
    renderProtectedRoute();
    expect(screen.getByText('Sign in page')).toBeInTheDocument();
    expect(screen.queryByText('Private page')).not.toBeInTheDocument();
  });

  it('renders a protected page for an authenticated user', () => {
    localStorage.setItem('token', 'valid-token');
    renderProtectedRoute();
    expect(screen.getByText('Private page')).toBeInTheDocument();
  });

  it('allows users with an accepted role', () => {
    mockedGetCurrentUserRoles.mockReturnValue(['Coach']);
    renderRoleGuard(['Coach', 'Admin']);
    expect(screen.getByText('Restricted page')).toBeInTheDocument();
  });

  it('redirects an athlete away from coach-only pages', () => {
    mockedGetCurrentUserRoles.mockReturnValue(['Athlete']);
    renderRoleGuard(['Coach', 'Admin']);
    expect(screen.getByText('Athlete dashboard')).toBeInTheDocument();
  });

  it('redirects other unauthorized roles to the coach dashboard', () => {
    mockedGetCurrentUserRoles.mockReturnValue(['Coach']);
    renderRoleGuard(['Admin']);
    expect(screen.getByText('Coach dashboard')).toBeInTheDocument();
  });
});
