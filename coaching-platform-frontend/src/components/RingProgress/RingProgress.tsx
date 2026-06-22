import React from 'react';
import './RingProgress.scss';

interface RingProgressProps {
  value: number;
  max: number;
  unit: string;
  label: string;
  size?: number;
  strokeWidth?: number;
  onIncrement?: () => void;
  incrementLabel?: string;
}

const RingProgress: React.FC<RingProgressProps> = ({
  value,
  max,
  unit,
  label,
  size = 120,
  strokeWidth = 10,
  onIncrement,
  incrementLabel,
}) => {
  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const percentage = max > 0 ? Math.min(value / max, 1) : 0;
  const isExceeded = value > max;
  const strokeDashoffset = circumference * (1 - percentage);
  const center = size / 2;

  return (
    <div className="ring-progress">
      <div className="ring-progress__svg-wrapper" style={{ width: size, height: size }}>
        <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
          {/* Background track */}
          <circle
            cx={center}
            cy={center}
            r={radius}
            fill="none"
            stroke="var(--surface-container)"
            strokeWidth={strokeWidth}
          />
          {/* Progress arc */}
          <circle
            cx={center}
            cy={center}
            r={radius}
            fill="none"
            stroke={isExceeded ? 'var(--color-red)' : 'var(--color-gold)'}
            strokeWidth={strokeWidth}
            strokeLinecap="round"
            strokeDasharray={circumference}
            strokeDashoffset={strokeDashoffset}
            transform={`rotate(-90 ${center} ${center})`}
            style={{ transition: 'stroke-dashoffset var(--transition-slow)' }}
          />
        </svg>
        {/* Center text */}
        <div className="ring-progress__center">
          <span className="ring-progress__value mono">{value}</span>
          <span className="ring-progress__unit">{unit}</span>
        </div>
      </div>

      <div className="ring-progress__info">
        <span className="ring-progress__label">{label}</span>
        <span className="ring-progress__target mono">
          / {max} {unit}
        </span>
        {onIncrement && (
          <button
            className="ring-progress__increment"
            onClick={onIncrement}
            type="button"
            aria-label={incrementLabel ?? `Add ${unit}`}
          >
            {incrementLabel ?? `+${unit}`}
          </button>
        )}
      </div>
    </div>
  );
};

export default RingProgress;
