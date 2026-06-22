import React, { useState } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { Tooltip } from 'antd';
import './AthleteLayout.scss';

const athleteNavItems = [
  { path: '/athlete/dashboard', icon: 'dashboard', label: 'Dashboard' },
  { path: '/athlete/meal-logger', icon: 'restaurant', label: 'Nutrition' },
  { path: '/athlete/recipes', icon: 'menu_book', label: 'Recipes' },
  { path: '/athlete/workouts', icon: 'fitness_center', label: 'Workouts' },
  { path: '/athlete/supplements', icon: 'medication', label: 'Supplements' },
  { path: '/athlete/check-in', icon: 'assignment', label: 'Check-In' },
];

const STORAGE_KEY = 'athlete-sidebar-collapsed';

const AthleteLayout: React.FC = () => {
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
    <div className={`athlete-layout ${collapsed ? 'athlete-layout--collapsed' : ''}`}>
      {/* Sidebar */}
      <aside className="athlete-layout__sidebar">
        {/* Logo */}
        <div className="athlete-layout__sidebar-logo">
          <div className="athlete-layout__logo-icon">JN</div>
          <span className="athlete-layout__logo-text">JOKER NUTRITION</span>
        </div>

        {/* Nav */}
        <nav className="athlete-layout__nav">
          {athleteNavItems.map((item) => (
            <Tooltip
              key={item.path}
              title={collapsed ? item.label : ''}
              placement="right"
              mouseEnterDelay={0.1}
            >
              <NavLink
                to={item.path}
                className={({ isActive }) =>
                  `athlete-layout__nav-item ${isActive ? 'athlete-layout__nav-item--active' : ''}`
                }
              >
                <span className="material-symbols-outlined">{item.icon}</span>
                <span className="athlete-layout__nav-label">{item.label}</span>
              </NavLink>
            </Tooltip>
          ))}
        </nav>

        {/* Footer */}
        <div className="athlete-layout__sidebar-footer">
          {user && (
            <Tooltip title={collapsed ? `${user.firstName} ${user.lastName}` : ''} placement="right">
              <div className="athlete-layout__user">
                <div className="athlete-layout__avatar">
                  {user.firstName?.[0]}{user.lastName?.[0]}
                </div>
                <div className="athlete-layout__user-info">
                  <span className="athlete-layout__user-name">{user.firstName} {user.lastName}</span>
                  <span className="athlete-layout__user-role">Athlete</span>
                </div>
              </div>
            </Tooltip>
          )}
          <Tooltip title={collapsed ? 'Sign out' : ''} placement="right">
            <button className="athlete-layout__logout" onClick={handleLogout} aria-label="Sign out">
              <span className="material-symbols-outlined">logout</span>
            </button>
          </Tooltip>
        </div>

        {/* Collapse toggle */}
        <button
          id="athlete-sidebar-toggle"
          className="athlete-layout__toggle"
          onClick={toggleCollapsed}
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          <span className="material-symbols-outlined">
            {collapsed ? 'chevron_right' : 'chevron_left'}
          </span>
        </button>
      </aside>

      {/* Main content */}
      <main className="athlete-layout__main">
        <Outlet />
      </main>
    </div>
  );
};

export default AthleteLayout;
