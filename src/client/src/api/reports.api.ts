import type { AxiosError } from 'axios';
import apiClient from './client';
import type {
  CancelledDocumentReportItem,
  DailyReport,
  ExportReportParams,
  StatusBreakdownItem,
} from '@/types/report.types';

function getDownloadFilename(contentDisposition: string | undefined, fallback: string): string {
  if (!contentDisposition) {
    return fallback;
  }

  const match = contentDisposition.match(/filename="?([^"]+)"?/i);
  return match?.[1] ?? fallback;
}

export const reportsApi = {
  getDaily: (date: string) =>
    apiClient.get<DailyReport>('/reports/daily', { params: { date } }).then(r => r.data),

  getStatusBreakdown: (from: string, to: string) =>
    apiClient.get<StatusBreakdownItem[]>('/reports/status-breakdown', { params: { from, to } }).then(r => r.data),

  getCancelled: (from: string, to: string) =>
    apiClient.get<CancelledDocumentReportItem[]>('/reports/cancelled', { params: { from, to } }).then(r => r.data),

  exportReport: async (reportType: string, params: ExportReportParams) => {
    try {
      const r = await apiClient.get(`/reports/export/${reportType}`, {
        params,
        responseType: 'blob',
      });

      const url = window.URL.createObjectURL(new Blob([r.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute(
        'download',
        getDownloadFilename(r.headers['content-disposition'], `${reportType}-report.csv`),
      );
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      const apiError = error as AxiosError<Blob>;
      const blob = apiError.response?.data;

      if (blob instanceof Blob && blob.type.includes('application/json')) {
        const text = await blob.text();
        apiError.response!.data = JSON.parse(text);
      }

      throw apiError;
    }
  },
};
