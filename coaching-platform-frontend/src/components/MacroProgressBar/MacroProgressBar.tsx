import React from 'react';
import './MacroProgressBar.scss';

interface MacroProgressBarProps {
  label: string;
  consumed: number;
  target: number;
  unit: string;
  color?: string; // override the bar color if needed
}

const MacroProgressBar: React.FC<MacroProgressBarProps> = ({
  label,
  consumed,
  target,
  unit,
  color,
}) => {
  const percentage = target > 0 ? Math.min((consumed / target) * 100, 100) : 0;
  const isExceeded = consumed > target;

  return (
    <div className="macro-bar">
      <div className="macro-bar__header">
        <span className="macro-bar__label">{label}</span>
        <span className={`macro-bar__values mono ${isExceeded ? 'macro-bar__values--exceeded' : ''}`}>
          {Math.round(consumed)}<span className="macro-bar__unit">/{Math.round(target)}{unit}</span>
        </span>
      </div>
      <div className="macro-bar__track">
        <div
          className={`macro-bar__fill ${isExceeded ? 'macro-bar__fill--exceeded' : ''}`}
          style={{
            width: `${percentage}%`,
            ...(color && !isExceeded ? { backgroundColor: color } : {}),
          }}
        />
      </div>
      {isExceeded && (
        <span className="macro-bar__over-label">
          +{Math.round(consumed - target)}{unit} over
        </span>
      )}
    </div>
  );
};

export default MacroProgressBar;
