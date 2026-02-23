export interface DocumentRequest {
  id: string;
  patientName: string;
  patientNameEn: string | null;
  recipientEntity: string;
  documentTypeId: string;
  documentTypeNameAr: string;
  documentTypeNameEn: string;
  notes: string | null;
  status: string;
  rejectionReason: string | null;
  createdById: string;
  createdByName: string;
  assignedToId: string | null;
  assignedToName: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateRequestDto {
  patientName: string;
  patientNameEn?: string;
  recipientEntity: string;
  documentTypeId: string;
  notes?: string;
}
