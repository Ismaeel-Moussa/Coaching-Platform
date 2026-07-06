import React from 'react';
import { InputNumber } from 'antd';
import './BiometricInputRow.scss';

interface BiometricInputRowProps {
  label: string;
  value: number | null;
  onChange: (val: number | null) => void;
  unit: 'kg' | 'cm';
  placeholder?: string;
  min?: number;
  max?: number;
  step?: number;
}

const BiometricInputRow: React.FC<BiometricInputRowProps> = ({
  label,
  value,
  onChange,
  unit,
  placeholder,
  min = 0,
  max = 500,
  step = 0.1,
}) => {
  return (
    <div className="biometric-row">
      <label className="biometric-row__label">{label}</label>
      <div className="biometric-row__input-container">
        <InputNumber
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          min={min}
          max={max}
          step={step}
          className="biometric-row__input data-label mono"
          addonAfter={unit}
          stringMode={false}
          style={{ width: '100%' }}
        />
      </div>
    </div>
  );
};

export default BiometricInputRow;
