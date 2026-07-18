import { afterEach, describe, expect, it, vi } from 'vitest';
import { formatRelativeTime, parseUtcDate } from './date';

describe('UTC date helpers', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('treats API timestamps without a timezone as UTC', () => {
    expect(parseUtcDate('2026-07-18T10:00:00').toISOString()).toBe('2026-07-18T10:00:00.000Z');
  });

  it('formats recent timestamps against a deterministic clock', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-07-18T12:00:00Z'));

    expect(formatRelativeTime('2026-07-18T11:55:00Z')).toBe('5m ago');
    expect(formatRelativeTime('2026-07-17T12:00:00Z')).toBe('Yesterday');
  });
});
