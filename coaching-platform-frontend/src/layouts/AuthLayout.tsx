import React from 'react';
import { Outlet } from 'react-router-dom';
import ErrorBoundary from '../components/ErrorBoundary/ErrorBoundary';
import './AuthLayout.scss';

const AuthLayout: React.FC = () => {
  return (
    <div className="auth-layout">
      {/* Left branding panel */}
      <div className="auth-layout__brand">
        <div className="auth-layout__brand-content">
          <div className="auth-layout__logo">
            <div className="auth-layout__logo-icon">
              <span>JN</span>
            </div>
            <span className="auth-layout__logo-text">JOKER NUTRITION</span>
          </div>
          <h1 className="auth-layout__tagline">
            Precision Fuels<br />Performance
          </h1>
          <p className="auth-layout__sub">
            Elite coaching, data-driven nutrition, and real-time athlete monitoring — all in one platform.
          </p>
          <div className="auth-layout__stats">
            <div className="auth-layout__stat">
              <span className="auth-layout__stat-value">500+</span>
              <span className="auth-layout__stat-label">Athletes</span>
            </div>
            <div className="auth-layout__stat-divider" />
            <div className="auth-layout__stat">
              <span className="auth-layout__stat-value">98%</span>
              <span className="auth-layout__stat-label">Compliance Rate</span>
            </div>
            <div className="auth-layout__stat-divider" />
            <div className="auth-layout__stat">
              <span className="auth-layout__stat-value">7-Day</span>
              <span className="auth-layout__stat-label">Avg. Results</span>
            </div>
          </div>
        </div>
        <div className="auth-layout__brand-accent" />
      </div>

      {/* Right form panel */}
      <div className="auth-layout__form-panel">
        <div className="auth-layout__form-container">
          <ErrorBoundary>
            <Outlet />
          </ErrorBoundary>
        </div>
      </div>
    </div>
  );
};

export default AuthLayout;
