import React from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './InvalidInvite.scss';

const InvalidInvite: React.FC = () => {
  const { t } = useTranslation();

  return (
    <div className="invalid-invite" id="invalid-invite-page">
      <div className="invalid-invite__icon">
        <span className="material-symbols-outlined">link_off</span>
      </div>
      <h2 className="invalid-invite__title">{t('auth:invalidInvite.title')}</h2>
      <p className="invalid-invite__message">
        {t('auth:invalidInvite.subtitle')}
        <br />
        {t('auth:invalidInvite.sub')}
      </p>
      <div className="invalid-invite__actions">
        <Link to="/sign-in" className="invalid-invite__btn">
          <span className="material-symbols-outlined">arrow_back</span>
          {t('auth:invalidInvite.backBtn')}
        </Link>
      </div>
    </div>
  );
};

export default InvalidInvite;
