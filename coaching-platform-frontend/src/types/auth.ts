export type UserRole = 'Admin' | 'Coach' | 'Athlete';

export interface LoginForm {
  email: string;
  password: string;
}

export interface RegisterForm {
  invitationToken: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
}

export interface RefreshTokenForm {
  refreshToken: string;
}

export interface ForgotPasswordForm {
  email: string;
}

export interface ResetPasswordForm {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface AuthUserDto {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  profilePictureUrl: string | null;
}

export interface AuthTokenDto {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresAt: string;
  user: AuthUserDto;
}
