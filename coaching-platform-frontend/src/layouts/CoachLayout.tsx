import React, { useState } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { Tooltip, Drawer } from 'antd';
import { useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { useLanguage } from '../contexts/LanguageContext';
import { useNotifications } from '../contexts/NotificationContext';
import HealthBanner from '../components/HealthBanner/HealthBanner';
import ErrorBoundary from '../components/ErrorBoundary/ErrorBoundary';
import LanguageSwitcher from '../components/LanguageSwitcher/LanguageSwitcher';
import './CoachLayout.scss';

const coachNavItems = [
  { path: '/coach/dashboard', icon: 'analytics', labelKey: 'nav.dashboard' },
  { path: '/coach/tasks', icon: 'task_alt', labelKey: 'nav.tasks' },
  { path: '/coach/roster', icon: 'group', labelKey: 'nav.clientRoster' },
  { path: '/coach/athlete-hub', icon: 'assignment_ind', labelKey: 'nav.athleteHub' },
  { path: '/coach/exercise-library', icon: 'fitness_center', labelKey: 'nav.exerciseLibrary' },
  { path: '/coach/food-admin', icon: 'restaurant_menu', labelKey: 'nav.foodRecipes' },
  { path: '/coach/nutrition-plans', icon: 'nutrition', labelKey: 'nav.nutritionPlans' },
  { path: '/coach/template-builder', icon: 'view_week', labelKey: 'nav.templateBuilder' },
  { path: '/coach/invitations', icon: 'mail', labelKey: 'nav.invitations' },
  { path: '/coach/notifications', icon: 'notifications', labelKey: 'nav.notifications' },
  { path: '/coach/profile', icon: 'person', labelKey: 'nav.profile' },
];

const STORAGE_KEY = 'coach-sidebar-collapsed';

const CoachLayout: React.FC = () => {
  const { t } = useTranslation();
  const { isRTL } = useLanguage();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { unreadCount } = useNotifications();
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

  const chevronIcon = collapsed
    ? (isRTL ? 'chevron_left' : 'chevron_right')
    : (isRTL ? 'chevron_right' : 'chevron_left');

  return (
    <div className={`coach-layout ${collapsed ? 'coach-layout--collapsed' : ''}`}>
      {/* ── Desktop Sidebar (hidden on mobile <= 768px) ── */}
      <aside className="coach-layout__sidebar">
        {/* Logo */}
        <div className="coach-layout__sidebar-logo">
          <div className="coach-layout__logo-brand">
            <div className="coach-layout__logo-icon">JN</div>
            <div className="coach-layout__logo-info">
              <span className="coach-layout__logo-text">{t('brand.title')}</span>
              <span className="coach-layout__logo-sub">{t('brand.coachHub')}</span>
            </div>
          </div>
          <button
            id="coach-sidebar-toggle"
            className="coach-layout__toggle"
            onClick={toggleCollapsed}
            aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            <span className="material-symbols-outlined">
              {chevronIcon}
            </span>
          </button>
        </div>

        {/* Nav */}
        <nav className="coach-layout__nav">
          {coachNavItems.map((item) => {
            const labelText = t(item.labelKey);
            return (
              <Tooltip
                key={item.path}
                title={collapsed ? labelText : ''}
                placement={isRTL ? 'left' : 'right'}
                mouseEnterDelay={0.1}
              >
                <NavLink
                  to={item.path}
                  className={({ isActive }) =>
                    `coach-layout__nav-item ${isActive ? 'coach-layout__nav-item--active' : ''}`
                  }
                >
                  <span className="material-symbols-outlined">{item.icon}</span>
                  <span className="coach-layout__nav-label">{labelText}</span>
                  {item.path === '/coach/notifications' && unreadCount > 0 && (
                    <span className="coach-layout__badge">{unreadCount}</span>
                  )}
                </NavLink>
              </Tooltip>
            );
          })}
        </nav>

        {/* Footer */}
        <div className="coach-layout__sidebar-footer">
          {user && (
            <Tooltip
              title={collapsed ? `${user.firstName} ${user.lastName}` : ''}
              placement={isRTL ? 'left' : 'right'}
            >
              <NavLink to="/coach/profile" className="coach-layout__user">
                <div className="coach-layout__avatar">
                  {user.profilePictureUrl ? (
                    <img src={user.profilePictureUrl} alt="avatar" className="coach-layout__avatar-img" />
                  ) : (
                    `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
                  )}
                </div>
                {!collapsed && (
                  <div className="coach-layout__user-info">
                    <span className="coach-layout__user-name">{user.firstName} {user.lastName}</span>
                    <span className="coach-layout__user-role">{t('profile:insights.jnStaff')}</span>
                  </div>
                )}
              </NavLink>
            </Tooltip>
          )}
          <div className="coach-layout__footer-actions" style={{ display: 'flex', alignItems: 'center', justifyContent: collapsed ? 'center' : 'space-evenly', gap: '8px', width: '100%', marginTop: '4px' }}>
            {!collapsed && <LanguageSwitcher className="coach-layout__lang-switch" />}
            <Tooltip title={collapsed ? t('nav.signOut') : ''} placement={isRTL ? 'left' : 'right'}>
              <button className="coach-layout__logout" onClick={handleLogout} aria-label="Sign out">
                <span className="material-symbols-outlined">logout</span>
              </button>
            </Tooltip>
          </div>
        </div>
      </aside>

      {/* ── Mobile Layout Elements (visible on mobile <= 768px) ── */}
      <header className="coach-layout__mobile-header">
        <div className="coach-layout__logo-brand">
          <div className="coach-layout__logo-icon">JN</div>
          <div className="coach-layout__logo-info">
            <span className="coach-layout__logo-text">{t('brand.title')}</span>
            <span className="coach-layout__logo-sub">{t('brand.coachHub')}</span>
          </div>
        </div>
        <div className="coach-layout__mobile-header-actions" style={{ display: 'flex', flexDirection: 'row', alignItems: 'center', gap: '16px' }}>
          <LanguageSwitcher className="coach-layout__lang-switch" />
          <NavLink
            to="/coach/notifications"
            className="coach-layout__mobile-notification-btn"
            aria-label="View notifications"
          >
            <span className="material-symbols-outlined">notifications</span>
            {unreadCount > 0 && (
              <span className="coach-layout__mobile-badge">{unreadCount}</span>
            )}
          </NavLink>
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
        </div>
      </header>

      {/* Slide-out Navigation Drawer for Mobile */}
      <Drawer
        title={t('brand.title')}
        placement={isRTL ? 'right' : 'left'}
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
                <p>{t('profile:insights.jnStaff')}</p>
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
              <span>{t(item.labelKey)}</span>
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
            <span>{t('nav.signOut')}</span>
          </button>
        </nav>
      </Drawer>

      {/* Main content */}
      <main className="coach-layout__main">
        <HealthBanner />
        <ErrorBoundary>
          <Outlet />
        </ErrorBoundary>
      </main>

      {/* ── Mobile Bottom Tab Bar (visible on mobile <= 768px) ── */}
      <nav className="coach-layout__bottom-tabs">
        <NavLink
          to="/coach/dashboard"
          className={({ isActive }) =>
            `coach-layout__tab ${isActive ? 'coach-layout__tab--active' : ''}`
          }
        >
          <span className="material-symbols-outlined">analytics</span>
          <span className="coach-layout__tab-label">{t('nav.dashboard')}</span>
        </NavLink>

        <NavLink
          to="/coach/roster"
          className={({ isActive }) =>
            `coach-layout__tab ${isActive ? 'coach-layout__tab--active' : ''}`
          }
        >
          <span className="material-symbols-outlined">group</span>
          <span className="coach-layout__tab-label">{t('nav.clientRoster')}</span>
        </NavLink>

        <NavLink
          to="/coach/athlete-hub"
          className={({ isActive }) =>
            `coach-layout__tab ${isActive ? 'coach-layout__tab--active' : ''}`
          }
        >
          <span className="material-symbols-outlined">assignment_ind</span>
          <span className="coach-layout__tab-label">{t('nav.athleteHub')}</span>
        </NavLink>

        <button
          className="coach-layout__tab coach-layout__tab--more"
          onClick={() => setDrawerOpen(true)}
          aria-label="More navigation"
        >
          <span className="material-symbols-outlined">menu</span>
          <span className="coach-layout__tab-label">{t('nav.more', 'More')}</span>
        </button>
      </nav>
    </div>
  );
};

export default CoachLayout;
