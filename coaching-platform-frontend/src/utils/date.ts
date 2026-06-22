/**
 * Returns today's date in ISO format: YYYY-MM-DD (local time, not UTC).
 */
export const getTodayIso = (): string => {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

/**
 * Formats an ISO date string for display (e.g. "Jun 22, 2026")
 */
export const formatDateDisplay = (isoDate: string): string => {
  return new Date(isoDate).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
};

/**
 * Returns the day of week name for an ISO date string.
 */
export const getDayOfWeek = (isoDate: string): string => {
  return new Date(isoDate).toLocaleDateString('en-US', { weekday: 'long' });
};
