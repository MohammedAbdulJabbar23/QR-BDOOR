import apiClient from './client';
import type { DocumentRequest, CreateRequestDto } from '@/types/request.types';
import type { PaginatedList } from '@/types/common.types';

export const requestsApi = {
  create: (data: CreateRequestDto) =>
    apiClient.post<{ id: string }>('/requests', data).then(r => r.data),

  getAll: (params: { status?: string; search?: string; fromDate?: string; toDate?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedList<DocumentRequest>>('/requests', { params }).then(r => r.data),

  getById: (id: string) =>
    apiClient.get<DocumentRequest>(`/requests/${id}`).then(r => r.data),

  getPending: () =>
    apiClient.get<DocumentRequest[]>('/requests/pending').then(r => r.data),

  update: (id: string, data: CreateRequestDto) =>
    apiClient.put(`/requests/${id}`, data).then(r => r.data),

  delete: (id: string) =>
    apiClient.delete(`/requests/${id}`).then(r => r.data),

  accept: (id: string) =>
    apiClient.post(`/requests/${id}/accept`).then(r => r.data),

  reject: (id: string, reason: string) =>
    apiClient.post(`/requests/${id}/reject`, { reason }).then(r => r.data),
};
