import apiClient from './client';
import type { DocumentType } from '@/types/common.types';

export const documentTypesApi = {
  getAll: (activeOnly = true) =>
    apiClient.get<DocumentType[]>('/document-types', { params: { activeOnly } }).then(r => r.data),

  create: (data: { nameAr: string; nameEn: string; descriptionAr?: string; descriptionEn?: string }) =>
    apiClient.post<{ id: string }>('/document-types', data).then(r => r.data),

  update: (id: string, data: { nameAr: string; nameEn: string; descriptionAr?: string; descriptionEn?: string; isActive: boolean }) =>
    apiClient.put(`/document-types/${id}`, data).then(r => r.data),
};
