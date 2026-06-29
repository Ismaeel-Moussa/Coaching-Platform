import React, { useState } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { Tooltip, Drawer } from 'antd';
import { useQueryClient } from '@tanstack/react-query';
import './CoachLayout.scss';

const coachNavItems = [
  { path: '/coach/dashboard', icon: 'analytics', label: 'Dashboard' },
  { path: '/coach/roster', icon: 'group', label: 'Client Roster' },
  { path: '/coach/exercise-library', icon: 'fitness_center', label: 'Exercise Library' },
  { path: '/coach/food-admin', icon: 'restaurant_menu', label: 'Food & Recipes' },
  { path: '/coach/template-builder', icon: 'view_week', label: 'Template Builder' },
  { path: '/coach/invitations', icon: 'mail', label: 'Invitations' },
  { path: '/coach/profile', icon: 'person', label: 'Profile' },
];

const STORAGE_KEY = 'coach-sidebar-collapsed';

const CoachLayout: React.FC = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;

  const [collapsed, setCollapsed] = useState<boolean>(
    () => localStorage.getItem(STORAGE_KEY) === 'true',
  );
  const [drawerOpen, setDrawerOpen] = useState(false);

  const toggleCollapsed = () => {
    setCollapsed((prev) => {
      const next = !prev;
      localStorage.setItem(STORAGE_KEY, String(next));
      return next;
    });
  };

  const handleLogout = () => {
    queryClient.clear();
    localStorage.clear();
    navigate('/sign-in', { replace: true });
  };

  return (
    <div className={`coach-layout ${collapsed ? 'coach-layout--collapsed' : ''}`}>
      {/* ── Desktop Sidebar (hidden on mobile <= 768px) ── */}
      <aside className="coach-layout__sidebar">
        {/* Logo */}
        <div className="coach-layout__sidebar-logo">
          <div className="coach-layout__logo-brand">
            <div className="coach-layout__logo-icon">JN</div>
            <div className="coach-layout__logo-info">
              <span className="coach-layout__logo-text">JOKER NUTRITION</span>
              <span className="coach-layout__logo-sub">Coach Hub</span>
            </div>
          </div>
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
              <NavLink to="/coach/profile" className="coach-layout__user">
                <div className="coach-layout__avatar">
                  {user.profilePictureUrl ? (
                    <img src={user.profilePictureUrl} alt="avatar" className="coach-layout__avatar-img" />
                  ) : (
                    `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
                  )}
                </div>
                <div className="coach-layout__user-info">
                  <span className="coach-layout__user-name">{user.firstName} {user.lastName}</span>
                  <span className="coach-layout__user-role">{user.role}</span>
                </div>
              </NavLink>
            </Tooltip>
          )}
          <Tooltip title={collapsed ? 'Sign out' : ''} placement="right">
            <button className="coach-layout__logout" onClick={handleLogout} aria-label="Sign out">
              <span className="material-symbols-outlined">logout</span>
            </button>
          </Tooltip>
        </div>
      </aside>

      {/* ── Mobile Layout Elements (visible on mobile <= 768px) ── */}
      <header className="coach-layout__mobile-header">
        <button
          className="coach-layout__mobile-hamburger"
          onClick={() => setDrawerOpen(true)}
          aria-label="Open navigation menu"
        >
          <span className="material-symbols-outlined">menu</span>
        </button>
        <div className="coach-layout__logo-brand">
          <div className="coach-layout__logo-icon">JN</div>
          <div className="coach-layout__logo-info">
            <span className="coach-layout__logo-text">JOKER NUTRITION</span>
            <span className="coach-layout__logo-sub">Coach Hub</span>
          </div>
        </div>
        {user && (
          <NavLink to="/coach/profile" className="coach-layout__mobile-avatar-btn" aria-label="Open profile">
            <div className="coach-layout__avatar">
              {user.profilePictureUrl ? (
                <img src={user.profilePictureUrl} alt="avatar" className="coach-layout__avatar-img" />
              ) : (
                `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
              )}
            </div>
          </NavLink>
        )}
      </header>

      {/* Slide-out Navigation Drawer for Mobile */}
      <Drawer
        title="Coach Portal"
        placement="left"
        onClose={() => setDrawerOpen(false)}
        open={drawerOpen}
        className="coach-layout__mobile-drawer"
        width={260}
        styles={{
          content: { backgroundColor: 'var(--color-navy)' },
          header: { backgroundColor: 'var(--color-navy)', borderBottom: '1px solid rgba(255, 255, 255, 0.08)' },
          body: { padding: '16px 0 24px', backgroundColor: 'var(--color-navy)' }
        }}
      >
        <div className="coach-layout__drawer-user">
          {user && (
            <>
              <div className="coach-layout__avatar coach-layout__avatar--large">
                {user.profilePictureUrl ? (
                  <img src={user.profilePictureUrl} alt="avatar" className="coach-layout__avatar-img" />
                ) : (
                  `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
                )}
              </div>
              <div className="coach-layout__drawer-user-info">
                <h4>{user.firstName} {user.lastName}</h4>
                <p>{user.role}</p>
              </div>
            </>
          )}
        </div>
        <hr className="coach-layout__drawer-divider" />
        <nav className="coach-layout__drawer-nav">
          {coachNavItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) => `coach-layout__drawer-item ${isActive ? 'coach-layout__drawer-item--active' : ''}`}
              onClick={() => setDrawerOpen(false)}
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              <span>{item.label}</span>
            </NavLink>
          ))}
          <button 
            className="coach-layout__drawer-item coach-layout__drawer-item--logout"
            onClick={() => {
              setDrawerOpen(false);
              handleLogout();
            }}
          >
            <span className="material-symbols-outlined">logout</span>
            <span>Sign Out</span>
          </button>
        </nav>
      </Drawer>

      {/* Main content */}
      <main className="coach-layout__main">
        <Outlet />
      </main>
    </div>
  );
};

export default CoachLayout;
