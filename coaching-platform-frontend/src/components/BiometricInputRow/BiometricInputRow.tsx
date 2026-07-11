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
  required?: boolean;
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
  required = false,
}) => {
  return (
    <div className="biometric-row">
      <label className="biometric-row__label">
        {label}
        {required && <span className="biometric-row__required"> *</span>}
      </label>
      <div className="biometric-row__input-wrapper">
        <InputNumber
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          min={min}
          max={max}
          step={step}
          controls={false}
          className="biometric-row__input"
          stringMode={false}
          // Numbers are always LTR regardless of UI language direction
          style={{ width: '100%', direction: 'ltr' }}
        />
        <span className="biometric-row__unit">{unit}</span>
      </div>
    </div>
  );
};

export default BiometricInputRow;
