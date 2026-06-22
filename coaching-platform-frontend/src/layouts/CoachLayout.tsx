import React, { useState } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { Tooltip } from 'antd';
import './CoachLayout.scss';

const coachNavItems = [
  { path: '/coach/dashboard', icon: 'analytics', label: 'Dashboard' },
  { path: '/coach/roster', icon: 'group', label: 'Client Roster' },
  { path: '/coach/exercise-library', icon: 'fitness_center', label: 'Exercise Library' },
  { path: '/coach/food-admin', icon: 'restaurant_menu', label: 'Food & Recipes' },
  { path: '/coach/template-builder', icon: 'view_week', label: 'Template Builder' },
  { path: '/coach/invitations', icon: 'mail', label: 'Invitations' },
];

const STORAGE_KEY = 'coach-sidebar-collapsed';

const CoachLayout: React.FC = () => {
  const navigate = useNavigate();
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;

  const [collapsed, setCollapsed] = useState<boolean>(
    () => localStorage.getItem(STORAGE_KEY) === 'true',
  );

  const toggleCollapsed = () => {
    setCollapsed((prev) => {
      const next = !prev;
      localStorage.setItem(STORAGE_KEY, String(next));
      return next;
    });
  };

  const handleLogout = () => {
    localStorage.clear();
    navigate('/sign-in', { replace: true });
  };

  return (
    <div className={`coach-layout ${collapsed ? 'coach-layout--collapsed' : ''}`}>
      {/* Sidebar */}
      <aside className="coach-layout__sidebar">
        {/* Logo */}
        <div className="coach-layout__sidebar-logo">
          <div className="coach-layout__logo-icon">JN</div>
          <div className="coach-layout__logo-info">
            <span className="coach-layout__logo-text">JOKER NUTRITION</span>
            <span className="coach-layout__logo-sub">Coach Hub</span>
          </div>
        </div>

        {/* Nav */}
        <nav className="coach-layout__nav">
          {coachNavItems.map((item) => (
            <Tooltip
              key={item.path}
              title={collapsed ? item.label : ''}
              placement="right"
              mouseEnterDelay={0.1}
            >
              <NavLink
                to={item.path}
                className={({ isActive }) =>
                  `coach-layout__nav-item ${isActive ? 'coach-layout__nav-item--active' : ''}`
                }
              >
                <span className="material-symbols-outlined">{item.icon}</span>
                <span className="coach-layout__nav-label">{item.label}</span>
              </NavLink>
            </Tooltip>
          ))}
        </nav>

        {/* Footer */}
        <div className="coach-layout__sidebar-footer">
          {user && (
            <Tooltip title={collapsed ? `${user.firstName} ${user.lastName}` : ''} placement="right">
              <div className="coach-layout__user">
                <div className="coach-layout__avatar">
                  {user.firstName?.[0]}{user.lastName?.[0]}
                </div>
                <div className="coach-layout__user-info">
                  <span className="coach-layout__user-name">{user.firstName} {user.lastName}</span>
                  <span className="coach-layout__user-role">{user.role}</span>
                </div>
              </div>
            </Tooltip>
          )}
          <Tooltip title={collapsed ? 'Sign out' : ''} placement="right">
            <button className="coach-layout__logout" onClick={handleLogout} aria-label="Sign out">
              <span className="material-symbols-outlined">logout</span>
            </button>
          </Tooltip>
        </div>

        {/* Collapse toggle */}
        <button
          id="coach-sidebar-toggle"
          className="coach-layout__toggle"
          onClick={toggleCollapsed}
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          <span className="material-symbols-outlined">
            {collapsed ? 'chevron_right' : 'chevron_left'}
          </span>
        </button>
      </aside>

      {/* Main content */}
      <main className="coach-layout__main">
        <Outlet />
      </main>
    </div>
  );
};

export default CoachLayout;
