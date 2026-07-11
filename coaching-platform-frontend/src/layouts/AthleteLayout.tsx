import React, { useState } from 'react';
import { Outlet, NavLink, useNavigate, useLocation } from 'react-router-dom';
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
  { path: '/athlete/notifications', icon: 'notifications', labelKey: 'nav.notifications' },
  { path: '/athlete/profile', icon: 'person', labelKey: 'nav.profile' },
];

const mobileBottomNavItems = [
  { path: '/athlete/dashboard', icon: 'dashboard', labelKey: 'nav.dashboard' },
  { path: '/athlete/meal-logger', icon: 'restaurant', labelKey: 'nav.nutrition' },
  { path: '/athlete/workouts', icon: 'fitness_center', labelKey: 'nav.workouts' },
  { path: '/athlete/supplements', icon: 'medication', labelKey: 'nav.supplements' },
];

const STORAGE_KEY = 'athlete-sidebar-collapsed';

const AthleteLayout: React.FC = () => {
  const { t } = useTranslation();
  const { isRTL } = useLanguage();
  const navigate = useNavigate();
  const location = useLocation();
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

  // Determine if a bottom nav path matches the current location
  const isBottomTabActive = (path: string) => {
    return location.pathname.startsWith(path);
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

      {/* ── Mobile Layout Elements (visible on mobile <= 768px) ── */}
      <header className="athlete-layout__mobile-header">
        <button
          className="athlete-layout__mobile-hamburger"
          onClick={() => setMoreDrawerOpen(true)}
          aria-label="Open navigation menu"
        >
          <span className="material-symbols-outlined">menu</span>
        </button>
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
              <span className="athlete-layout__mobile-nav-label">{t(item.labelKey)}</span>
            </NavLink>
          );
        })}
      </nav>

      {/* More Drawer for Mobile */}
      <Drawer
        title={t('brand.title')}
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
                {user.profilePictureUrl ? (
                  <img src={user.profilePictureUrl} alt="avatar" className="athlete-layout__avatar-img" />
                ) : (
                  `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`
                )}
              </div>
              <div className="athlete-layout__drawer-user-info">
                <h4>{user.firstName} {user.lastName}</h4>
                <p>{t('profile:insights.jnStaff')}</p>
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
            <span>{t('nav.recipes')}</span>
          </NavLink>
          <NavLink
            to="/athlete/check-in"
            className={({ isActive }) => `athlete-layout__drawer-item ${isActive ? 'athlete-layout__drawer-item--active' : ''}`}
            onClick={() => setMoreDrawerOpen(false)}
          >
            <span className="material-symbols-outlined">assignment</span>
            <span>{t('nav.checkIn')}</span>
          </NavLink>
          <NavLink
            to="/athlete/profile"
            className={({ isActive }) => `athlete-layout__drawer-item ${isActive ? 'athlete-layout__drawer-item--active' : ''}`}
            onClick={() => setMoreDrawerOpen(false)}
          >
            <span className="material-symbols-outlined">person</span>
            <span>{t('nav.profile')}</span>
          </NavLink>
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
        </div>
      </Drawer>

      {/* Main content */}
      <main className="athlete-layout__main">
        <HealthBanner />
        <ErrorBoundary>
          <Outlet />
        </ErrorBoundary>
      </main>
    </div>
  );
};

export default AthleteLayout;
