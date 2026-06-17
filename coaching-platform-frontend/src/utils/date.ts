export const formatDate = (iso: string): string => {
  try {
    return new Date(iso).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return iso;
  }
};

export const formatDateTime = (iso: string): string => {
  try {
    return new Date(iso).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return iso;
  }
};

export const isExpired = (iso: string): boolean => {
  try {
    return new Date(iso) < new Date();
  } catch {
    return false;
  }
};

export const getToday = (): string => {
  return new Date().toISOString().split('T')[0];
};
