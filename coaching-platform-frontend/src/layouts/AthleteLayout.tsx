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
import './AthleteLayout.scss';

const athleteNavItems = [
  { path: '/athlete/dashboard', icon: 'dashboard', labelKey: 'nav.dashboard' },
  { path: '/athlete/meal-logger', icon: 'restaurant', labelKey: 'nav.nutrition' },
  { path: '/athlete/workouts', icon: 'fitness_center', labelKey: 'nav.workouts' },
  { path: '/athlete/supplements', icon: 'medication', labelKey: 'nav.supplements' },
  { path: '/athlete/recipes', icon: 'menu_book', labelKey: 'nav.recipes' },
  { path: '/athlete/check-in', icon: 'assignment', labelKey: 'nav.checkIn' },
  { path: '/athlete/feedback', icon: 'chat', labelKey: 'nav.feedback' },
  { path: '/athlete/history', icon: 'history', labelKey: 'nav.history' },
  { path: '/athlete/notifications', icon: 'notifications', labelKey: 'nav.notifications' },
  { path: '/athlete/profile', icon: 'person', labelKey: 'nav.profile' },
];

const STORAGE_KEY = 'athlete-sidebar-collapsed';

const AthleteLayout: React.FC = () => {
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
  const [moreDrawerOpen, setMoreDrawerOpen] = useState(false);

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
    <div className={`athlete-layout ${collapsed ? 'athlete-layout--collapsed' : ''}`}>
      {/* ── Desktop Sidebar (hidden on mobile <= 768px) ── */}
      <aside className="athlete-layout__sidebar">
        {/* Logo */}
        <div className="athlete-layout__sidebar-logo">
          <div className="athlete-layout__logo-brand">
            <div className="athlete-layout__logo-icon">JN</div>
            <div className="athlete-layout__logo-info">
              <span className="athlete-layout__logo-text">{t('brand.title')}</span>
              <span className="athlete-layout__logo-sub">{t('brand.clientPortal')}</span>
            </div>
          </div>
          <button
            id="athlete-sidebar-toggle"
            className="athlete-layout__toggle"
            onClick={toggleCollapsed}
            aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            <span className="material-symbols-outlined">
              {chevronIcon}
            </span>
          </button>
        </div>

        {/* Nav */}
        <nav className="athlete-layout__nav">
          {athleteNavItems.map((item) => {
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
                    `athlete-layout__nav-item ${isActive ? 'athlete-layout__nav-item--active' : ''}`
                  }
                >
                  <span className="material-symbols-outlined">{item.icon}</span>
                  <span className="athlete-layout__nav-label">{labelText}</span>
                  {item.path === '/athlete/notifications' && unreadCount > 0 && (
                    <span className="athlete-layout__badge">{unreadCount}</span>
                  )}
                </NavLink>
              </Tooltip>
            );
          })}
        </nav>

        {/* Footer */}
        <div className="athlete-layout__sidebar-footer">
          {user && (
            <Tooltip
              title={collapsed ? `${user.firstName} ${user.lastName}` : ''}
              placement={isRTL ? 'left' : 'right'}
            >
              <NavLink to="/athlete/profile" className="athlete-layout__user">
                <div className="athlete-layout__avatar">
                  {user.profilePictureUrl ? (
                    <img src={user.profilePictureUrl} alt="avatar" className="athlete-layout__avatar-img" />
                  ) : (
                    `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
                  )}
                </div>
                {!collapsed && (
                  <div className="athlete-layout__user-info">
                    <span className="athlete-layout__user-name">{user.firstName} {user.lastName}</span>
                    <span className="athlete-layout__user-role">{t('profile:insights.roleTag', { role: 'Athlete' })}</span>
                  </div>
                )}
              </NavLink>
            </Tooltip>
          )}
          <div className="athlete-layout__footer-actions" style={{ display: 'flex', alignItems: 'center', justifyContent: collapsed ? 'center' : 'space-evenly', gap: '8px', width: '100%', marginTop: '4px' }}>
            {!collapsed && <LanguageSwitcher className="athlete-layout__lang-switch" />}
            <Tooltip title={collapsed ? t('nav.signOut') : ''} placement={isRTL ? 'left' : 'right'}>
              <button className="athlete-layout__logout" onClick={handleLogout} aria-label="Sign out">
                <span className="material-symbols-outlined">logout</span>
              </button>
            </Tooltip>
          </div>
        </div>
      </aside>

      {/* ── Mobile Header (visible on mobile <= 768px) ── */}
      <header className="athlete-layout__mobile-header">
        <div className="athlete-layout__logo-brand">
          <div className="athlete-layout__logo-icon">JN</div>
          <div className="athlete-layout__logo-info">
            <span className="athlete-layout__logo-text">{t('brand.title')}</span>
            <span className="athlete-layout__logo-sub">{t('brand.clientPortal')}</span>
          </div>
        </div>

        <div className="athlete-layout__mobile-header-actions" style={{ display: 'flex', flexDirection: 'row', alignItems: 'center', gap: '16px' }}>
          <LanguageSwitcher className="athlete-layout__lang-switch" />
          <NavLink
            to="/athlete/notifications"
            className="athlete-layout__mobile-notification-btn"
            aria-label="View notifications"
          >
            <span className="material-symbols-outlined">notifications</span>
            {unreadCount > 0 && (
              <span className="athlete-layout__mobile-badge">{unreadCount}</span>
            )}
          </NavLink>
          {user && (
            <NavLink
              to="/athlete/profile"
              className="athlete-layout__mobile-avatar-btn"
              aria-label="Open profile"
            >
              <div className="athlete-layout__avatar">
                {user.profilePictureUrl ? (
                  <img src={user.profilePictureUrl} alt="avatar" className="athlete-layout__avatar-img" />
                ) : (
                  `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
                )}
              </div>
            </NavLink>
          )}
        </div>
      </header>

      {/* More Drawer for Mobile */}
      <Drawer
        title={t('brand.title')}
        placement={isRTL ? 'right' : 'left'}
        onClose={() => setMoreDrawerOpen(false)}
        open={moreDrawerOpen}
        className="athlete-layout__mobile-drawer"
        width={260}
        styles={{
          content: { backgroundColor: 'var(--color-navy)' },
          header: { backgroundColor: 'var(--color-navy)', borderBottom: '1px solid rgba(255, 255, 255, 0.08)' },
          body: { padding: '16px 0 24px', backgroundColor: 'var(--color-navy)' }
        }}
      >
        <div className="athlete-layout__drawer-user">
          {user && (
            <>
              <div className="athlete-layout__avatar athlete-layout__avatar--large">
                {user.profilePictureUrl ? (
                  <img src={user.profilePictureUrl} alt="avatar" className="athlete-layout__avatar-img" />
                ) : (
                  `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
                )}
              </div>
              <div className="athlete-layout__drawer-user-info">
                <h4>{user.firstName} {user.lastName}</h4>
                <p>{t('profile:insights.roleTag', { role: 'Athlete' })}</p>
              </div>
            </>
          )}
        </div>
        <hr className="athlete-layout__drawer-divider" />
        <nav className="athlete-layout__drawer-nav">
          {athleteNavItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) => `athlete-layout__drawer-item ${isActive ? 'athlete-layout__drawer-item--active' : ''}`}
              onClick={() => setMoreDrawerOpen(false)}
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              <span>{t(item.labelKey)}</span>
              {item.path === '/athlete/notifications' && unreadCount > 0 && (
                <span className="athlete-layout__badge">{unreadCount}</span>
              )}
            </NavLink>
          ))}
          <button
            className="athlete-layout__drawer-item athlete-layout__drawer-item--logout"
            onClick={() => {
              setMoreDrawerOpen(false);
              handleLogout();
            }}
          >
            <span className="material-symbols-outlined">logout</span>
            <span>{t('nav.signOut')}</span>
          </button>
        </nav>
      </Drawer>

      {/* Main content */}
      <main className="athlete-layout__main">
        <HealthBanner />
        <ErrorBoundary>
          <Outlet />
        </ErrorBoundary>
      </main>

      {/* ── Mobile Bottom Tab Bar (visible on mobile <= 768px) ── */}
      <nav className="athlete-layout__bottom-tabs">
        <NavLink
          to="/athlete/dashboard"
          className={({ isActive }) =>
            `athlete-layout__tab ${isActive ? 'athlete-layout__tab--active' : ''}`
          }
        >
          <span className="material-symbols-outlined">dashboard</span>
          <span className="athlete-layout__tab-label">{t('nav.dashboard')}</span>
        </NavLink>

        <NavLink
          to="/athlete/meal-logger"
          className={({ isActive }) =>
            `athlete-layout__tab ${isActive ? 'athlete-layout__tab--active' : ''}`
          }
        >
          <span className="material-symbols-outlined">restaurant</span>
          <span className="athlete-layout__tab-label">{t('nav.nutrition')}</span>
        </NavLink>

        <NavLink
          to="/athlete/workouts"
          className={({ isActive }) =>
            `athlete-layout__tab ${isActive ? 'athlete-layout__tab--active' : ''}`
          }
        >
          <span className="material-symbols-outlined">fitness_center</span>
          <span className="athlete-layout__tab-label">{t('nav.workouts')}</span>
        </NavLink>

        <NavLink
          to="/athlete/supplements"
          className={({ isActive }) =>
            `athlete-layout__tab ${isActive ? 'athlete-layout__tab--active' : ''}`
          }
        >
          <span className="material-symbols-outlined">medication</span>
          <span className="athlete-layout__tab-label">{t('nav.supplements')}</span>
        </NavLink>

        <button
          className="athlete-layout__tab athlete-layout__tab--more"
          onClick={() => setMoreDrawerOpen(true)}
          aria-label="More navigation"
        >
          <span className="material-symbols-outlined">menu</span>
          <span className="athlete-layout__tab-label">{t('nav.more', 'More')}</span>
        </button>
      </nav>
    </div>
  );
};

export default AthleteLayout;
