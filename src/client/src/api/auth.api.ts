import apiClient from './client';
import type { LoginRequest, AuthResponse } from '@/types/auth.types';

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>('/auth/login', data).then(r => r.data),

  getMe: () =>
    apiClient.get('/auth/me').then(r => r.data),

  changePassword: (currentPassword: string, newPassword: string) =>
    apiClient.post('/auth/change-password', { currentPassword, newPassword }).then(r => r.data),

  updateLanguage: (language: string) =>
    apiClient.put('/auth/me/language', { language }).then(r => r.data),
};
