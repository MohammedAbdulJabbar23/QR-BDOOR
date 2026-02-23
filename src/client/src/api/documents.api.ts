import apiClient from './client';
import type { IssuedDocument } from '@/types/document.types';
import type { PaginatedList } from '@/types/common.types';

export const documentsApi = {
  prepare: (data: { requestId: string; documentBody?: string }) =>
    apiClient.post<IssuedDocument>('/documents', data).then(r => r.data),

  getAll: (params: { status?: string; search?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedList<IssuedDocument>>('/documents', { params }).then(r => r.data),

  getById: (id: string) =>
    apiClient.get<IssuedDocument>(`/documents/${id}`).then(r => r.data),

  getByRequest: (requestId: string) =>
    apiClient.get<IssuedDocument[]>(`/documents/by-request/${requestId}`).then(r => r.data),

  uploadPdf: (id: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post(`/documents/${id}/upload-pdf`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then(r => r.data);
  },

  revoke: (id: string, reason: string) =>
    apiClient.post(`/documents/${id}/revoke`, { reason }).then(r => r.data),

  delete: (id: string) =>
    apiClient.delete(`/documents/${id}`).then(r => r.data),

  getQrImage: (id: string) =>
    apiClient.get(`/documents/${id}/qr-image`, { responseType: 'blob' }).then(r => r.data),
};
