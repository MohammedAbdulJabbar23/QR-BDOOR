import apiClient from './client';

export const reportsApi = {
  getDaily: (date: string) =>
    apiClient.get('/reports/daily', { params: { date } }).then(r => r.data),

  getStatusBreakdown: (from: string, to: string) =>
    apiClient.get('/reports/status-breakdown', { params: { from, to } }).then(r => r.data),

  getCancelled: (from: string, to: string) =>
    apiClient.get('/reports/cancelled', { params: { from, to } }).then(r => r.data),

  exportReport: (reportType: string, from: string, to: string) =>
    apiClient.get(`/reports/export/${reportType}`, {
      params: { from, to },
      responseType: 'blob',
    }).then(r => {
      const url = window.URL.createObjectURL(new Blob([r.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${reportType}-report.docx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    }),
};
