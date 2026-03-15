import axios from 'axios';
import type { VerificationResult } from '@/types/document.types';

const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api';

// Public API - no auth needed
export const verifyApi = {
  verify: (documentId: string) =>
    axios.get<VerificationResult>(`${baseUrl}/verify/${documentId}`).then(r => r.data),

  getPdfUrl: (documentId: string) => `${baseUrl}/verify/${documentId}/pdf`,

  getAccountStatementUrl: (documentId: string) => `${baseUrl}/verify/${documentId}/account-statement`,
};
