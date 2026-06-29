import React from 'react';
import { useNavigate } from 'react-router-dom';
import './ComplianceBar.scss';

interface ComplianceBarProps {
  athleteId: number;
  athleteName: string;
  consumed: number;
  target: number;
  isOverTarget: boolean;
  compliancePercent: number;
}

const ComplianceBar: React.FC<ComplianceBarProps> = ({
  athleteId,
  athleteName,
  consumed,
  target,
  isOverTarget,
  compliancePercent,
}) => {
  const navigate = useNavigate();
  
  // Calculate percentage for styling. Cap at 100% for the first segment
  const displayPercent = Math.min(compliancePercent, 100);
  const overflowPercent = isOverTarget ? Math.min(((consumed - target) / target) * 100, 100) : 0;

  const handleClick = () => {
    navigate(`/coach/roster/${athleteId}`);
  };

  return (
    <div className="compliance-bar" onClick={handleClick} role="button" tabIndex={0}>
      <div className="compliance-bar__header">
        <span className="compliance-bar__name">{athleteName}</span>
        <div className="compliance-bar__stats mono">
          <span className={`compliance-bar__consumed ${isOverTarget ? 'compliance-bar__consumed--over' : ''}`}>
            {Math.round(consumed)}
          </span>
          <span className="compliance-bar__divider">/</span>
          <span className="compliance-bar__target">{Math.round(target)} kcal</span>
        </div>
      </div>
      
      <div className="compliance-bar__track-wrapper">
        <div className="compliance-bar__track">
          <div 
            className={`compliance-bar__fill ${isOverTarget ? 'compliance-bar__fill--exceeded' : ''}`}
            style={{ width: `${displayPercent}%` }}
          />
          {isOverTarget && (
            <div 
              className="compliance-bar__fill-overflow"
              style={{ width: `${overflowPercent}%`, left: '100%', transform: 'translateX(-100%)' }} // show overflow indicator
            />
          )}
        </div>
        {isOverTarget && (
          <span className="material-symbols-outlined compliance-bar__alert-icon" title="Exceeded calorie limit">
            warning
          </span>
        )}
      </div>
    </div>
  );
};

export default ComplianceBar;
