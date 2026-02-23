import apiClient from './client';
import type { AuditLogDto, PaginatedList } from '@/types/common.types';

export const auditLogsApi = {
  getAll: (params: {
    userId?: string; action?: string; entityType?: string;
    from?: string; to?: string; page?: number; pageSize?: number;
  }) =>
    apiClient.get<PaginatedList<AuditLogDto>>('/audit-logs', { params }).then(r => r.data),
};
