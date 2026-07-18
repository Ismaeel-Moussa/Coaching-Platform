import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import PhotoUploadZone from './PhotoUploadZone';

const { showError } = vi.hoisted(() => ({ showError: vi.fn() }));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock('antd', () => ({
  Button: ({ children, icon, ...props }: ButtonHTMLAttributes<HTMLButtonElement> & { icon?: ReactNode }) => (
    <button {...props}>{icon}{children}</button>
  ),
  Progress: ({ percent }: { percent: number }) => <div>{percent}%</div>,
  Spin: () => <div>Loading</div>,
  message: { error: showError },
}));

describe('PhotoUploadZone', () => {
  beforeEach(() => {
    showError.mockReset();
    vi.stubGlobal('URL', {
      createObjectURL: vi.fn(() => 'blob:preview'),
      revokeObjectURL: vi.fn(),
    });
  });

  it('displays an existing signed private photo and supports deleting it', () => {
    const onDelete = vi.fn();
    const signedUrl = 'https://private.blob.example/side.jpg?sig=temporary';

    render(
      <PhotoUploadZone
        angle="Side"
        file={null}
        existingUrl={signedUrl}
        onFileSelect={vi.fn()}
        onDelete={onDelete}
      />,
    );

    expect(screen.getByRole('img', { name: 'Side progress preview' })).toHaveAttribute('src', signedUrl);
    fireEvent.click(screen.getByRole('button'));
    expect(onDelete).toHaveBeenCalledOnce();
  });

  it('copies a valid selected image into a durable in-memory file', async () => {
    const onFileSelect = vi.fn();
    const { container } = render(
      <PhotoUploadZone
        angle="Front"
        file={null}
        onFileSelect={onFileSelect}
        onDelete={vi.fn()}
      />,
    );
    const input = container.querySelector('input[type="file"]') as HTMLInputElement;
    const file = new File(['photo-bytes'], 'front.jpg', { type: 'image/jpeg', lastModified: 123 });
    Object.defineProperty(file, 'arrayBuffer', {
      value: vi.fn().mockResolvedValue(new TextEncoder().encode('photo-bytes').buffer),
    });

    fireEvent.change(input, { target: { files: [file] } });

    await waitFor(() => expect(onFileSelect).toHaveBeenCalledOnce());
    const durableFile = onFileSelect.mock.calls[0]?.[0] as File;
    expect(durableFile).not.toBe(file);
    expect(durableFile.name).toBe('front.jpg');
    expect(durableFile.type).toBe('image/jpeg');
  });

  it('rejects unsupported files before invoking the upload callback', async () => {
    const onFileSelect = vi.fn();
    const { container } = render(
      <PhotoUploadZone
        angle="Back"
        file={null}
        onFileSelect={onFileSelect}
        onDelete={vi.fn()}
      />,
    );
    const input = container.querySelector('input[type="file"]') as HTMLInputElement;
    const file = new File(['not-an-image'], 'notes.txt', { type: 'text/plain' });

    fireEvent.change(input, { target: { files: [file] } });

    await waitFor(() => expect(showError).toHaveBeenCalledWith('common:alerts.imageTypeInvalid'));
    expect(onFileSelect).not.toHaveBeenCalled();
  });
});
