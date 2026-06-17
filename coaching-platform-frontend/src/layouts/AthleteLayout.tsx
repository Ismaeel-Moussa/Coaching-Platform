import React from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import './AthleteLayout.scss';

const athleteNavItems = [
  { path: '/athlete/dashboard', icon: 'dashboard', label: 'Dashboard' },
  { path: '/athlete/meal-logger', icon: 'restaurant', label: 'Nutrition' },
  { path: '/athlete/recipes', icon: 'menu_book', label: 'Recipes' },
  { path: '/athlete/workouts', icon: 'fitness_center', label: 'Workouts' },
  { path: '/athlete/supplements', icon: 'medication', label: 'Supplements' },
  { path: '/athlete/check-in', icon: 'assignment', label: 'Check-In' },
];

const AthleteLayout: React.FC = () => {
  const navigate = useNavigate();
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;

  const handleLogout = () => {
    localStorage.clear();
    navigate('/sign-in', { replace: true });
  };

  return (
    <div className="athlete-layout">
      {/* Sidebar */}
      <aside className="athlete-layout__sidebar">
        <div className="athlete-layout__sidebar-logo">
          <div className="athlete-layout__logo-icon">JN</div>
          <span className="athlete-layout__logo-text">JOKER NUTRITION</span>
        </div>

        <nav className="athlete-layout__nav">
          {athleteNavItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `athlete-layout__nav-item ${isActive ? 'athlete-layout__nav-item--active' : ''}`
              }
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="athlete-layout__sidebar-footer">
          {user && (
            <div className="athlete-layout__user">
              <div className="athlete-layout__avatar">
                {user.firstName?.[0]}{user.lastName?.[0]}
              </div>
              <div className="athlete-layout__user-info">
                <span className="athlete-layout__user-name">{user.firstName} {user.lastName}</span>
                <span className="athlete-layout__user-role">Athlete</span>
              </div>
            </div>
          )}
          <button className="athlete-layout__logout" onClick={handleLogout} title="Sign out">
            <span className="material-symbols-outlined">logout</span>
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="athlete-layout__main">
        <Outlet />
      </main>
    </div>
  );
};

export default AthleteLayout;
