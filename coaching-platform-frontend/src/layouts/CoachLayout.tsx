import React from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import './CoachLayout.scss';

const coachNavItems = [
  { path: '/coach/dashboard', icon: 'analytics', label: 'Dashboard' },
  { path: '/coach/roster', icon: 'group', label: 'Client Roster' },
  { path: '/coach/exercise-library', icon: 'fitness_center', label: 'Exercise Library' },
  { path: '/coach/food-admin', icon: 'restaurant_menu', label: 'Food & Recipes' },
  { path: '/coach/template-builder', icon: 'view_week', label: 'Template Builder' },
  { path: '/coach/invitations', icon: 'mail', label: 'Invitations' },
];

const CoachLayout: React.FC = () => {
  const navigate = useNavigate();
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;

  const handleLogout = () => {
    localStorage.clear();
    navigate('/sign-in', { replace: true });
  };

  return (
    <div className="coach-layout">
      {/* Sidebar */}
      <aside className="coach-layout__sidebar">
        <div className="coach-layout__sidebar-logo">
          <div className="coach-layout__logo-icon">JN</div>
          <div className="coach-layout__logo-info">
            <span className="coach-layout__logo-text">JOKER NUTRITION</span>
            <span className="coach-layout__logo-sub">Coach Hub</span>
          </div>
        </div>

        <nav className="coach-layout__nav">
          {coachNavItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `coach-layout__nav-item ${isActive ? 'coach-layout__nav-item--active' : ''}`
              }
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="coach-layout__sidebar-footer">
          {user && (
            <div className="coach-layout__user">
              <div className="coach-layout__avatar">
                {user.firstName?.[0]}{user.lastName?.[0]}
              </div>
              <div className="coach-layout__user-info">
                <span className="coach-layout__user-name">{user.firstName} {user.lastName}</span>
                <span className="coach-layout__user-role">{user.role}</span>
              </div>
            </div>
          )}
          <button className="coach-layout__logout" onClick={handleLogout} title="Sign out">
            <span className="material-symbols-outlined">logout</span>
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="coach-layout__main">
        <Outlet />
      </main>
    </div>
  );
};

export default CoachLayout;
