import { useMutation, useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { message as antMessage } from 'antd';
import type { AxiosError } from 'axios';
import {
  loginUser,
  registerWithInvite,
  forgotPassword,
  resetPassword,
  validateInviteToken,
} from '../../api/auth';
import type {
  LoginForm,
  RegisterForm,
  ForgotPasswordForm,
  ResetPasswordForm,
  AuthTokenDto,
} from '../../types/auth';

// ── Persist auth response to localStorage ────────────────────────────────────
const persistAuth = (data: AuthTokenDto) => {
  localStorage.setItem('token', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  localStorage.setItem('user', JSON.stringify(data.user));
};

// ── Determine where to navigate after login based on role ────────────────────
const getRoleRoute = (role: string): string => {
  switch (role) {
    case 'Athlete':
      return '/athlete/dashboard';
    case 'Coach':
    case 'Admin':
      return '/coach/dashboard';
    default:
      return '/sign-in';
  }
};

// ── useLogin ──────────────────────────────────────────────────────────────────
export const useLogin = () => {
  const navigate = useNavigate();

  return useMutation<AuthTokenDto, AxiosError, LoginForm>({
    mutationFn: loginUser,
    onSuccess: (data) => {
      persistAuth(data);
      antMessage.success(`Welcome back, ${data.user.firstName}!`);
      navigate(getRoleRoute(data.user.role), { replace: true });
    },
    onError: (error) => {
      if (error.response?.status === 429 || (error.response?.status && error.response.status >= 500)) return;
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Invalid email or password. Please try again.';
      antMessage.error(msg);
    },
  });
};

// ── useRegister ───────────────────────────────────────────────────────────────
export const useRegister = () => {
  const navigate = useNavigate();

  return useMutation<AuthTokenDto, AxiosError, RegisterForm>({
    mutationFn: registerWithInvite,
    onSuccess: (data) => {
      persistAuth(data);
      antMessage.success(`Welcome to Joker Nutrition, ${data.user.firstName}!`);
      navigate(getRoleRoute(data.user.role), { replace: true });
    },
    onError: (error) => {
      if (error.response?.status === 429 || (error.response?.status && error.response.status >= 500)) return;
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Registration failed. Please check your details and try again.';
      antMessage.error(msg);
    },
  });
};

// ── useValidateInviteToken ────────────────────────────────────────────────────
export const useValidateInviteToken = (token: string | null) => {
  return useQuery({
    queryKey: ['invite-token', token],
    queryFn: () => validateInviteToken(token!),
    enabled: !!token,
    retry: false,
  });
};

// ── useForgotPassword ─────────────────────────────────────────────────────────
export const useForgotPassword = () => {
  return useMutation<{ message: string }, AxiosError, ForgotPasswordForm>({
    mutationFn: forgotPassword,
    onError: (error) => {
      if (error.response?.status === 429 || (error.response?.status && error.response.status >= 500)) return;
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Something went wrong. Please try again.';
      antMessage.error(msg);
    },
  });
};

// ── useResetPassword ──────────────────────────────────────────────────────────
export const useResetPassword = () => {
  const navigate = useNavigate();

  return useMutation<{ message: string }, AxiosError, ResetPasswordForm>({
    mutationFn: resetPassword,
    onSuccess: () => {
      antMessage.success('Password reset successfully! Please sign in.');
      navigate('/sign-in', { replace: true });
    },
    onError: (error) => {
      if (error.response?.status === 429 || (error.response?.status && error.response.status >= 500)) return;
      const msg =
        (error.response?.data as { message?: string })?.message ??
        'Reset failed. Your link may have expired.';
      antMessage.error(msg);
    },
  });
};
