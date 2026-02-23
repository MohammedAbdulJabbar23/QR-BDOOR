import apiClient from './client';
import type { UserDto, PaginatedList } from '@/types/common.types';
import type { CreateUserDto } from '@/types/user.types';

export const usersApi = {
  getAll: (params: { page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedList<UserDto>>('/users', { params }).then(r => r.data),

  getById: (id: string) =>
    apiClient.get<UserDto>(`/users/${id}`).then(r => r.data),

  create: (data: CreateUserDto) =>
    apiClient.post<{ id: string }>('/users', data).then(r => r.data),

  update: (id: string, data: { fullName: string; fullNameEn?: string; role: string; department: string }) =>
    apiClient.put(`/users/${id}`, data).then(r => r.data),

  deactivate: (id: string) =>
    apiClient.put(`/users/${id}/deactivate`).then(r => r.data),
};
