import React from 'react';
import { Slider } from 'antd';
import './SubjectiveSlider.scss';

interface SubjectiveSliderProps {
  label: string;
  value: number;
  onChange: (val: number) => void;
  min?: number;
  max?: number;
}

const SubjectiveSlider: React.FC<SubjectiveSliderProps> = ({
  label,
  value,
  onChange,
  min = 1,
  max = 10,
}) => {
  const getStatusColorClass = (val: number) => {
    if (val <= 3) return 'subjective-slider__value--low'; // Red
    if (val <= 6) return 'subjective-slider__value--mid'; // Gold
    return 'subjective-slider__value--high'; // Green
  };

  const getStatusLabel = (val: number) => {
    if (val <= 3) return 'Poor';
    if (val <= 5) return 'Fair';
    if (val <= 7) return 'Good';
    if (val <= 9) return 'Very Good';
    return 'Excellent';
  };

  return (
    <div className="subjective-slider">
      <div className="subjective-slider__header">
        <span className="subjective-slider__label">{label}</span>
        <div className="subjective-slider__status">
          <span className="subjective-slider__status-text">{getStatusLabel(value)}</span>
          <span className={`subjective-slider__value mono data-label ${getStatusColorClass(value)}`}>
            {value}
          </span>
        </div>
      </div>
      <div className="subjective-slider__slider-wrapper">
        <Slider
          min={min}
          max={max}
          value={value}
          onChange={onChange}
          tooltip={{ open: false }}
          className="subjective-slider__slider"
        />
        <div className="subjective-slider__marks mono">
          <span>{min}</span>
          <span>5</span>
          <span>{max}</span>
        </div>
      </div>
    </div>
  );
};

export default SubjectiveSlider;
