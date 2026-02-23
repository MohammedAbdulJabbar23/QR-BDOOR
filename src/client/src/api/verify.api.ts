import axios from 'axios';
import type { VerificationResult } from '@/types/document.types';

// Public API - no auth needed
export const verifyApi = {
  verify: (documentId: string) =>
    axios.get<VerificationResult>(`/api/verify/${documentId}`).then(r => r.data),

  getPdfUrl: (documentId: string) => `/api/verify/${documentId}/pdf`,
};
