import apiClient from './client';
import type { NotificationDto, PaginatedList } from '@/types/common.types';

export const notificationsApi = {
  getAll: (params: { page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedList<NotificationDto>>('/notifications', { params }).then(r => r.data),

  getUnreadCount: () =>
    apiClient.get<{ count: number }>('/notifications/unread-count').then(r => r.data),

  markAsRead: (id: number) =>
    apiClient.put(`/notifications/${id}/read`).then(r => r.data),

  markAllAsRead: () =>
    apiClient.put('/notifications/read-all').then(r => r.data),
};
