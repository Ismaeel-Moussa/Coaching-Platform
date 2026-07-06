import React, { useState, useEffect } from 'react';
import { useGetHealth } from '../../hooks/useHealth/useHealth';
import './HealthBanner.scss';

const HealthBanner: React.FC = () => {
  const { data } = useGetHealth();
  const [isDismissed, setIsDismissed] = useState(false);
  const [prevStatus, setPrevStatus] = useState<string | null>(null);

  // If health status changes (e.g. goes from healthy -> degraded), reset dismissal
  useEffect(() => {
    if (data) {
      const currentKey = `${data.status}-${data.database}`;
      if (prevStatus && prevStatus !== currentKey) {
        setIsDismissed(false);
      }
      setPrevStatus(currentKey);
    }
  }, [data, prevStatus]);

  if (!data || isDismissed) return null;

  const isDegraded = data.status === 'degraded' || data.database !== 'connected';

  if (!isDegraded) return null;

  return (
    <div className="health-banner">
      <div className="health-banner__content">
        <span className="material-symbols-outlined health-banner__icon">warning</span>
        <span className="health-banner__text">
          Service is temporarily degraded. Some features may not be available. (Database status: {data.database})
        </span>
      </div>
      <button className="health-banner__close" onClick={() => setIsDismissed(true)} aria-label="Dismiss banner">
        <span className="material-symbols-outlined">close</span>
      </button>
    </div>
  );
};

export default HealthBanner;
