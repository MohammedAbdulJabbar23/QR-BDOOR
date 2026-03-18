export interface DailyReport {
  date: string;
  totalRequests: number;
  pendingRequests: number;
  completedRequests: number;
  rejectedRequests: number;
  documentsIssued: number;
  documentsArchived: number;
}

export interface StatusBreakdownItem {
  status: string;
  count: number;
}

export interface CancelledDocumentReportItem {
  documentNumber: string;
  patientName: string;
  revocationReason: string | null;
  revokedAt: string | null;
  replacementDocumentNumber: string | null;
}

export type FilteredReportDataset = 'requests' | 'documents';

export interface ExportReportParams {
  from?: string;
  to?: string;
  dataset?: FilteredReportDataset;
  search?: string;
  status?: string;
  documentTypeId?: string;
}
