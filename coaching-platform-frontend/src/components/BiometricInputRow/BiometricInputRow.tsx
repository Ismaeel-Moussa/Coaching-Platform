import React from 'react';
import { InputNumber, Space } from 'antd';
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
        <Space.Compact style={{ width: '100%' }}>
          <InputNumber
            value={value}
            onChange={onChange}
            placeholder={placeholder}
            min={min}
            max={max}
            step={step}
            className="biometric-row__input data-label mono"
            stringMode={false}
            style={{ width: '100%' }}
          />
          <span style={{
            background: 'var(--ant-color-fill-alter, #fafafa)',
            border: '1px solid var(--ant-color-border, #d9d9d9)',
            borderLeft: 'none',
            padding: '0 11px',
            display: 'inline-flex',
            alignItems: 'center',
            borderRadius: '0 6px 6px 0',
            color: 'var(--ant-color-text-description, rgba(0, 0, 0, 0.45))'
          }}>
            {unit}
          </span>
        </Space.Compact>
      </div>
    </div>
  );
};

export default BiometricInputRow;
