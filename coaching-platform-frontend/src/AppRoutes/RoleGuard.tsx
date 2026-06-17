import React from 'react';
import { Navigate } from 'react-router-dom';
import { getCurrentUserRoles } from '../utils/getCurrentUserRoles';

interface RoleGuardProps {
  allowedRoles: string[];
  children: React.ReactNode;
}

const RoleGuard: React.FC<RoleGuardProps> = ({ allowedRoles, children }) => {
  const roles = getCurrentUserRoles();
  const hasAccess = allowedRoles.some((r) => roles.includes(r));

  if (!hasAccess) {
    // Redirect to the appropriate hub based on actual role
    if (roles.includes('Athlete')) return <Navigate to="/athlete/dashboard" replace />;
    return <Navigate to="/coach/dashboard" replace />;
  }

  return <>{children}</>;
};

export default RoleGuard;
