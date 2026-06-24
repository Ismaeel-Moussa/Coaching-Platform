import React, { useState } from 'react';
import { Outlet, NavLink, useNavigate, useLocation } from 'react-router-dom';
import { Tooltip, Drawer } from 'antd';
import './AthleteLayout.scss';

const athleteNavItems = [
  { path: '/athlete/dashboard', icon: 'dashboard', label: 'Dashboard' },
  { path: '/athlete/meal-logger', icon: 'restaurant', label: 'Nutrition' },
  { path: '/athlete/workouts', icon: 'fitness_center', label: 'Workouts' },
  { path: '/athlete/supplements', icon: 'medication', label: 'Supplements' },
  { path: '/athlete/recipes', icon: 'menu_book', label: 'Recipes' },
  { path: '/athlete/check-in', icon: 'assignment', label: 'Check-In' },
];

const mobileBottomNavItems = [
  { path: '/athlete/dashboard', icon: 'dashboard', label: 'Dashboard' },
  { path: '/athlete/meal-logger', icon: 'restaurant', label: 'Nutrition' },
  { path: '/athlete/workouts', icon: 'fitness_center', label: 'Workouts' },
  { path: '/athlete/supplements', icon: 'medication', label: 'Supplements' },
];

const STORAGE_KEY = 'athlete-sidebar-collapsed';

const AthleteLayout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;

  const [collapsed, setCollapsed] = useState<boolean>(
    () => localStorage.getItem(STORAGE_KEY) === 'true',
  );
  const [moreDrawerOpen, setMoreDrawerOpen] = useState(false);

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

  // Determine if a bottom nav path matches the current location
  const isBottomTabActive = (path: string) => {
    return location.pathname.startsWith(path);
  };

  return (
    <div className={`athlete-layout ${collapsed ? 'athlete-layout--collapsed' : ''}`}>
      {/* ── Desktop Sidebar (hidden on mobile <= 768px) ── */}
      <aside className="athlete-layout__sidebar">
        {/* Logo */}
        <div className="athlete-layout__sidebar-logo">
          <div className="athlete-layout__logo-brand">
            <div className="athlete-layout__logo-icon">JN</div>
            <span className="athlete-layout__logo-text">JOKER NUTRITION</span>
          </div>
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
      </aside>

      {/* ── Mobile Layout Elements (visible on mobile <= 768px) ── */}
      <header className="athlete-layout__mobile-header">
        <div className="athlete-layout__logo-brand">
          <div className="athlete-layout__logo-icon">JN</div>
          <span className="athlete-layout__logo-text">JOKER NUTRITION</span>
        </div>
        {user && (
          <button 
            className="athlete-layout__mobile-avatar-btn" 
            onClick={() => setMoreDrawerOpen(true)}
            aria-label="Open menu"
          >
            <div className="athlete-layout__avatar">
              {user.firstName?.[0]}{user.lastName?.[0]}
            </div>
          </button>
        )}
      </header>

      <nav className="athlete-layout__mobile-nav">
        {mobileBottomNavItems.map((item) => {
          const active = isBottomTabActive(item.path);
          return (
            <NavLink
              key={item.path}
              to={item.path}
              className={`athlete-layout__mobile-nav-item ${active ? 'athlete-layout__mobile-nav-item--active' : ''}`}
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              <span className="athlete-layout__mobile-nav-label">{item.label}</span>
            </NavLink>
          );
        })}
      </nav>

      {/* More Drawer for Mobile */}
      <Drawer
        title="Joker Nutrition"
        placement="bottom"
        onClose={() => setMoreDrawerOpen(false)}
        open={moreDrawerOpen}
        height="auto"
        className="athlete-layout__mobile-drawer"
        styles={{
          content: { backgroundColor: 'var(--color-navy)', borderRadius: '16px 16px 0 0' },
          header: { backgroundColor: 'var(--color-navy)', borderBottom: '1px solid rgba(255, 255, 255, 0.08)' },
          body: { padding: '16px 24px 24px', backgroundColor: 'var(--color-navy)' }
        }}
      >
        <div className="athlete-layout__drawer-user">
          {user && (
            <>
              <div className="athlete-layout__avatar athlete-layout__avatar--large">
                {user.firstName?.[0]}{user.lastName?.[0]}
              </div>
              <div className="athlete-layout__drawer-user-info">
                <h4>{user.firstName} {user.lastName}</h4>
                <p>Athlete Profile</p>
              </div>
            </>
          )}
        </div>
        <hr className="athlete-layout__drawer-divider" />
        <div className="athlete-layout__drawer-menu">
          <NavLink
            to="/athlete/recipes"
            className={({ isActive }) => `athlete-layout__drawer-item ${isActive ? 'athlete-layout__drawer-item--active' : ''}`}
            onClick={() => setMoreDrawerOpen(false)}
          >
            <span className="material-symbols-outlined">menu_book</span>
            <span>Recipe Library</span>
          </NavLink>
          <NavLink
            to="/athlete/check-in"
            className={({ isActive }) => `athlete-layout__drawer-item ${isActive ? 'athlete-layout__drawer-item--active' : ''}`}
            onClick={() => setMoreDrawerOpen(false)}
          >
            <span className="material-symbols-outlined">assignment</span>
            <span>Weekly Check-In</span>
          </NavLink>
          <button 
            className="athlete-layout__drawer-item athlete-layout__drawer-item--logout"
            onClick={() => {
              setMoreDrawerOpen(false);
              handleLogout();
            }}
          >
            <span className="material-symbols-outlined">logout</span>
            <span>Sign Out</span>
          </button>
        </div>
      </Drawer>

      {/* Main content */}
      <main className="athlete-layout__main">
        <Outlet />
      </main>
    </div>
  );
};

export default AthleteLayout;
