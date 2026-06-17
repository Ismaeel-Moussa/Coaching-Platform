import React from 'react';
import { Link } from 'react-router-dom';
import './InvalidInvite.scss';

const InvalidInvite: React.FC = () => {
  return (
    <div className="invalid-invite" id="invalid-invite-page">
      <div className="invalid-invite__icon">
        <span className="material-symbols-outlined">link_off</span>
      </div>
      <h2 className="invalid-invite__title">Invitation Not Valid</h2>
      <p className="invalid-invite__message">
        This invitation link has expired, been revoked, or has already been used.
        Please contact your coach to receive a new invitation.
      </p>
      <div className="invalid-invite__actions">
        <Link to="/sign-in" className="invalid-invite__btn">
          <span className="material-symbols-outlined">arrow_back</span>
          Back to Sign In
        </Link>
      </div>
    </div>
  );
};

export default InvalidInvite;
