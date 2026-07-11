import React from 'react';
import { Outlet } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ErrorBoundary from '../components/ErrorBoundary/ErrorBoundary';
import LanguageSwitcher from '../components/LanguageSwitcher/LanguageSwitcher';
import './AuthLayout.scss';

const AuthLayout: React.FC = () => {
  const { t } = useTranslation(['common', 'auth']);

  return (
    <div className="auth-layout">
      {/* Left branding panel */}
      <div className="auth-layout__brand">
        <div className="auth-layout__brand-content">
          <div className="auth-layout__logo-container">
            <div className="auth-layout__logo">
              <div className="auth-layout__logo-icon">
                <span>JN</span>
              </div>
              <span className="auth-layout__logo-text">{t('brand.title')}</span>
            </div>
            <div className="auth-layout__brand-lang">
              <LanguageSwitcher />
            </div>
          </div>
          <h1 className="auth-layout__tagline">
            {t('auth:authLayout.tagline').split('\n').map((line, idx) => (
              <React.Fragment key={idx}>
                {line}
                <br />
              </React.Fragment>
            ))}
          </h1>
          <p className="auth-layout__sub">
            {t('auth:authLayout.sub')}
          </p>
          <div className="auth-layout__stats">
            <div className="auth-layout__stat">
              <span className="auth-layout__stat-value">500+</span>
              <span className="auth-layout__stat-label">{t('auth:authLayout.athletes')}</span>
            </div>
            <div className="auth-layout__stat-divider" />
            <div className="auth-layout__stat">
              <span className="auth-layout__stat-value">98%</span>
              <span className="auth-layout__stat-label">{t('auth:authLayout.compliance')}</span>
            </div>
            <div className="auth-layout__stat-divider" />
            <div className="auth-layout__stat">
              <span className="auth-layout__stat-value">7-Day</span>
              <span className="auth-layout__stat-label">{t('auth:authLayout.results')}</span>
            </div>
          </div>
        </div>
        <div className="auth-layout__brand-accent" />
      </div>

      {/* Right form panel */}
      <div className="auth-layout__form-panel">
        {/* Language Switcher in Auth */}
        <div className="auth-layout__lang-container">
          <LanguageSwitcher className="language-switcher--light-bg" />
        </div>
        
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
