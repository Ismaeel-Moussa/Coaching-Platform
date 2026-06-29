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

/**
 * Parses a datetime string from the API as UTC.
 * The backend stores all timestamps via DateTime.UtcNow, but SQL Server's datetime2
 * strips the Kind metadata. This helper appends 'Z' if the string has no timezone
 * indicator, ensuring JavaScript's Date constructor treats it as UTC.
 */
export const parseUtcDate = (dateStr: string): Date => {
  if (!dateStr) return new Date(NaN);
  // If string already has timezone info (Z, +HH:MM, or -HH:MM after the date part), parse as-is
  if (dateStr.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(dateStr)) {
    return new Date(dateStr);
  }
  return new Date(dateStr + 'Z');
};

/**
 * Formats a ISO datetime string to relative time (e.g. "5m ago", "2h ago", "Yesterday", "Jun 29")
 */
export const formatRelativeTime = (dateStr: string | null | undefined): string => {
  if (!dateStr) return '';
  const date = parseUtcDate(dateStr);
  const now = new Date();
  
  // Calculate difference in milliseconds
  const diffMs = now.getTime() - date.getTime();
  if (diffMs < 0) return 'Just now';
  
  const diffMins = Math.floor(diffMs / 60000);
  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours}h ago`;
  
  const diffDays = Math.floor(diffHours / 24);
  if (diffDays === 1) return 'Yesterday';
  if (diffDays < 7) return `${diffDays}d ago`;
  
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
};

