import React from 'react';
import { Link } from 'react-router-dom';
import './NotFound.scss';

const NotFound: React.FC = () => {
  return (
    <div className="not-found" id="not-found-page">
      <div className="not-found__content">
        <div className="not-found__code">404</div>
        <h1 className="not-found__title">Page Not Found</h1>
        <p className="not-found__message">
          The page you're looking for doesn't exist or has been moved.
        </p>
        <Link to="/" className="not-found__btn">
          <span className="material-symbols-outlined">home</span>
          Back to Home
        </Link>
      </div>
    </div>
  );
};

export default NotFound;
