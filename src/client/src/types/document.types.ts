export interface IssuedDocument {
  id: string;
  documentNumber: string;
  requestId: string;
  patientName: string;
  patientNameEn: string | null;
  recipientEntity: string;
  documentTypeNameAr: string;
  documentTypeNameEn: string;
  qrCodeUrl: string;
  qrCodeImagePath: string | null;
  hasPdf: boolean;
  documentBody: string | null;
  status: string;
  revocationReason: string | null;
  replacementDocumentId: string | null;
  replacementDocumentNumber: string | null;
  qrExpiresAt: string | null;
  patientGender: string | null;
  patientProfession: string | null;
  patientAge: string | null;
  admissionDate: string | null;
  dischargeDate: string | null;
  leaveGranted: string | null;
  issuedById: string;
  issuedByName: string;
  revokedById: string | null;
  revokedByName: string | null;
  issuedAt: string;
  archivedAt: string | null;
  revokedAt: string | null;
}

export interface VerificationResult {
  status: 'verified' | 'revoked' | 'invalid' | 'expired';
  documentNumber: string | null;
  patientName: string | null;
  recipientEntity: string | null;
  issuedAt: string | null;
  revokedAt: string | null;
  revocationReason: string | null;
  replacementDocumentId: string | null;
  replacementDocumentNumber: string | null;
  hasPdf: boolean;
}
