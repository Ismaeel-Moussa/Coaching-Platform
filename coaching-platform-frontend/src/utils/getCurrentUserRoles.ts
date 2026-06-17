import type { UserRole } from '../types/auth';

export interface StoredUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  profilePictureUrl: string | null;
}

export const getCurrentUserRoles = (): string[] => {
  const user = localStorage.getItem('user');
  if (!user) return [];
  try {
    const parsed: StoredUser = JSON.parse(user);
    return parsed.role ? [parsed.role] : [];
  } catch {
    return [];
  }
};

export const getCurrentUser = (): StoredUser | null => {
  const user = localStorage.getItem('user');
  if (!user) return null;
  try {
    return JSON.parse(user) as StoredUser;
  } catch {
    return null;
  }
};
