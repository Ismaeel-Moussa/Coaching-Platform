export const getRateLimitMessage = (url: string | undefined): string => {
  if (!url) return 'Too many requests. Please wait and try again.';

  if (url.includes('/auth/login')) {
    return 'Too many login attempts. Please wait 1 minute.';
  }

  if (url.includes('/auth/forgot-password')) {
    return 'Reset link request limit reached. Please try again in 1 hour.';
  }

  return 'Too many requests. Please wait and try again.';
};
