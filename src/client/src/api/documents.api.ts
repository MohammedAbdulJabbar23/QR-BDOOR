import apiClient from './client';
import type { IssuedDocument } from '@/types/document.types';
import type { PaginatedList } from '@/types/common.types';

export const documentsApi = {
  prepare: (data: {
    requestId: string;
    documentNumber: string;
    subject?: string;
    documentBody?: string;
    patientGender?: string;
    patientProfession?: string;
    patientAge?: string;
    admissionDate?: string;
    dischargeDate?: string;
    leaveGranted?: string;
    treatingPhysicianName?: string;
  }) =>
    apiClient.post<IssuedDocument>('/documents', data).then(r => r.data),

  getAll: (params: { status?: string; search?: string; documentTypeId?: string; fromDate?: string; toDate?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedList<IssuedDocument>>('/documents', { params }).then(r => r.data),

  getById: (id: string) =>
    apiClient.get<IssuedDocument>(`/documents/${id}`).then(r => r.data),

  getByRequest: (requestId: string) =>
    apiClient.get<IssuedDocument[]>(`/documents/by-request/${requestId}`).then(r => r.data),

  generatePdf: (id: string, includeDirectorSignature = false) =>
    apiClient.post(`/documents/${id}/generate-pdf`, { includeDirectorSignature }).then(r => r.data),

  exportExcel: (params: { status?: string; search?: string; documentTypeId?: string; fromDate?: string; toDate?: string }) =>
    apiClient.get(`/documents/export-excel`, { params, responseType: 'blob' }).then(r => r),

  revoke: (id: string, reason: string) =>
    apiClient.post(`/documents/${id}/revoke`, { reason }).then(r => r.data),

  delete: (id: string) =>
    apiClient.delete(`/documents/${id}`).then(r => r.data),

  getQrImage: (id: string) =>
    apiClient.get(`/documents/${id}/qr-image`, { responseType: 'blob' }).then(r => r.data),

  getPdf: (id: string) =>
    apiClient.get(`/documents/${id}/pdf`, { responseType: 'blob' }).then(r => r),

  uploadPdf: (id: string, file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return apiClient.post(`/documents/${id}/upload-pdf`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then(r => r.data);
  },

  transferToAccounts: (id: string) =>
    apiClient.post(`/documents/${id}/transfer-to-accounts`).then(r => r.data),

  uploadAccountStatement: (id: string, file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return apiClient.post(`/documents/${id}/upload-account-statement`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then(r => r.data);
  },

  getAccountStatement: (id: string) =>
    apiClient.get(`/documents/${id}/account-statement`, { responseType: 'blob' }).then(r => r),
};
