import axios from 'axios';
import { message as antMessage } from 'antd';
import { getRateLimitMessage } from '../utils/rateLimitMessages';
import type {
  AuthTokenDto,
  LoginForm,
  RegisterForm,
  RefreshTokenForm,
  ForgotPasswordForm,
  ResetPasswordForm,
} from '../types/auth';
import type { InvitationDto } from '../types/Invitation';

// Bare (unauthenticated) Axios instance — used for public auth endpoints only
const authApi = axios.create({
  baseURL: import.meta.env.VITE_API_URL as string,
  headers: {
    'Content-Type': 'application/json',
  },
});

authApi.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    if (status === 429) {
      const msg = getRateLimitMessage(error.config?.url);
      antMessage.error(msg);
      return Promise.reject(error);
    }
    if (status >= 500) {
      antMessage.error('Server error. Please try again later.');
      return Promise.reject(error);
    }
    return Promise.reject(error);
  }
);

// ── Auth API functions ────────────────────────────────────────────────────────

export const loginUser = async (form: LoginForm): Promise<AuthTokenDto> => {
  const response = await authApi.post<AuthTokenDto>('/auth/login', form);
  return response.data;
};

export const registerWithInvite = async (form: RegisterForm): Promise<AuthTokenDto> => {
  const response = await authApi.post<AuthTokenDto>('/auth/register', form);
  return response.data;
};

export const refreshAccessToken = async (form: RefreshTokenForm): Promise<AuthTokenDto> => {
  const response = await authApi.post<AuthTokenDto>('/auth/refresh', form);
  return response.data;
};

export const forgotPassword = async (form: ForgotPasswordForm): Promise<{ message: string }> => {
  const response = await authApi.post<{ message: string }>('/auth/forgot-password', form);
  return response.data;
};

export const resetPassword = async (form: ResetPasswordForm): Promise<{ message: string }> => {
  const response = await authApi.post<{ message: string }>('/auth/reset-password', form);
  return response.data;
};

export const validateInviteToken = async (token: string): Promise<InvitationDto> => {
  const response = await authApi.get<InvitationDto>(`/invitations/validate/${token}`);
  return response.data;
};
