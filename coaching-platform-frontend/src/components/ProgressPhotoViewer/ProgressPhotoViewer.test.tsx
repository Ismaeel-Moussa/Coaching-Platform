import type { ComponentProps, ReactNode } from 'react';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import ProgressPhotoViewer from './ProgressPhotoViewer';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (_key: string, fallback?: string) => fallback ?? _key,
  }),
}));

vi.mock('antd', () => {
  const Image = (props: ComponentProps<'img'>) => <img {...props} />;
  Image.PreviewGroup = ({ children }: { children: ReactNode }) => <>{children}</>;
  return { Image };
});

describe('ProgressPhotoViewer', () => {
  it('renders signed private-photo URLs returned by the backend', () => {
    const signedUrl = 'https://private.blob.example/front.jpg?sig=temporary';

    render(<ProgressPhotoViewer photos={[{ angle: 'Front', url: signedUrl }]} />);

    expect(screen.getByRole('img', { name: 'Front View' })).toHaveAttribute('src', signedUrl);
  });

  it('shows a safe empty state when a grid photo is unavailable', () => {
    render(<ProgressPhotoViewer photos={[{ angle: 'Side', url: null }]} />);
    expect(screen.getByText('No photo')).toBeInTheDocument();
  });

  it('shows one compact fallback when a thumbnail group has no photos', () => {
    render(<ProgressPhotoViewer variant="thumb" photos={[
      { angle: 'Front', url: null },
      { angle: 'Side', url: null },
      { angle: 'Back', url: null },
    ]} />);

    expect(screen.getByText('No Photos')).toBeInTheDocument();
    expect(screen.queryByRole('img')).not.toBeInTheDocument();
  });
});
