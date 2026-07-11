import React from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './NotFound.scss';

const NotFound: React.FC = () => {
  const { t } = useTranslation(['common']);
  return (
    <div className="not-found" id="not-found-page">
      <div className="not-found__content">
        <div className="not-found__code">{t('common:notFound.code')}</div>
        <h1 className="not-found__title">{t('common:notFound.title')}</h1>
        <p className="not-found__message">
          {t('common:notFound.message')}
        </p>
        <Link to="/" className="not-found__btn">
          <span className="material-symbols-outlined">home</span>
          {t('common:notFound.backHome')}
        </Link>
      </div>
    </div>
  );
};

export default NotFound;
