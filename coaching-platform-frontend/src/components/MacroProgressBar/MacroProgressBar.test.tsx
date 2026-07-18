import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import MacroProgressBar from './MacroProgressBar';

describe('MacroProgressBar', () => {
  it('renders progress and applies the configured color below target', () => {
    const { container } = render(
      <MacroProgressBar label="Protein" consumed={75} target={150} unit="g" color="#123456" />,
    );

    expect(screen.getByText('Protein')).toBeInTheDocument();
    expect(container.querySelector('.macro-bar__fill')).toHaveStyle({
      width: '50%',
      backgroundColor: '#123456',
    });
    expect(screen.queryByText(/over$/)).not.toBeInTheDocument();
  });

  it('caps the bar and reports values above the target', () => {
    const { container } = render(
      <MacroProgressBar label="Protein" consumed={175} target={150} unit="g" />,
    );

    expect(container.querySelector('.macro-bar__fill')).toHaveStyle({ width: '100%' });
    expect(container.querySelector('.macro-bar__fill')).toHaveClass('macro-bar__fill--exceeded');
    expect(screen.getByText('+25g over')).toBeInTheDocument();
  });
});
