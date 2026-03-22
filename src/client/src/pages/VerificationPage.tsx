import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  ShieldCheck, ShieldX, ShieldAlert, ShieldQuestion,
  FileDown, Globe,
} from 'lucide-react';
import { verifyApi } from '@/api/verify.api';
import { cn } from '@/utils/cn';
import type { VerificationResult } from '@/types/document.types';

type Language = 'ar' | 'en';

const labels = {
  ar: {
    title: 'التحقق من الوثيقة',
    verified: 'وثيقة موثقة',
    verifiedMessage: 'تم التحقق من صحة هذه الوثيقة.',
    revoked: 'وثيقة ملغاة',
    revokedMessage: 'تم إلغاء هذه الوثيقة.',
    invalid: 'وثيقة غير موجودة',
    invalidMessage: 'لا يمكن التحقق من هذه الوثيقة. يرجى التواصل مع مستشفى البدور.',
    expired: 'وثيقة منتهية الصلاحية',
    expiredMessage: 'انتهت صلاحية التحقق من هذه الوثيقة. يرجى التواصل مع مستشفى البدور.',
    documentNumber: 'رقم الوثيقة',
    patientName: 'اسم المريض',
    recipientEntity: 'الجهة المستلمة',
    issuedDate: 'تاريخ الإصدار',
    revokedDate: 'تاريخ الإلغاء',
    revocationReason: 'سبب الإلغاء',
    replacementAvailable: 'تم إصدار وثيقة بديلة محدثة.',
    viewReplacement: 'عرض الوثيقة البديلة',
    downloadPdf: 'تحميل التقرير',
    downloadAccountStatement: 'تحميل كشف الحساب',
    loading: 'جاري التحقق...',
    toggleLang: 'EN',
    footer: 'مستشفى البدور - نظام إدارة الوثائق',
  },
  en: {
    title: 'Document Verification',
    verified: 'Document Verified',
    verifiedMessage: 'This document has been verified as authentic.',
    revoked: 'Document Revoked',
    revokedMessage: 'This document has been revoked.',
    invalid: 'Document Not Found',
    invalidMessage: 'This document could not be verified. Please contact Al-Badour Hospital.',
    expired: 'Document Expired',
    expiredMessage: 'Verification for this document has expired. Please contact Al-Badour Hospital.',
    documentNumber: 'Document Number',
    patientName: 'Patient Name',
    recipientEntity: 'Recipient Entity',
    issuedDate: 'Issue Date',
    revokedDate: 'Revoked Date',
    revocationReason: 'Revocation Reason',
    replacementAvailable: 'An updated document has been issued.',
    viewReplacement: 'View Replacement Document',
    downloadPdf: 'Download Report',
    downloadAccountStatement: 'Download Account Statement',
    loading: 'Verifying...',
    toggleLang: 'عربي',
    footer: 'Al-Badour Hospital - Document Management System',
  },
} as const;

const statusConfig: Record<
  VerificationResult['status'],
  { icon: typeof ShieldCheck; colorClass: string; bgClass: string }
> = {
  verified: {
    icon: ShieldCheck,
    colorClass: 'text-green-600',
    bgClass: 'bg-green-50 border-green-200',
  },
  revoked: {
    icon: ShieldX,
    colorClass: 'text-red-600',
    bgClass: 'bg-red-50 border-red-200',
  },
  invalid: {
    icon: ShieldQuestion,
    colorClass: 'text-neutral-500',
    bgClass: 'bg-neutral-50 border-neutral-200',
  },
  expired: {
    icon: ShieldAlert,
    colorClass: 'text-amber-600',
    bgClass: 'bg-amber-50 border-amber-200',
  },
};

function formatVerificationDate(dateStr: string, lang: Language): string {
  const date = new Date(dateStr);
  return date.toLocaleDateString(lang === 'ar' ? 'ar-SA' : 'en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function VerificationPage() {
  const { documentId } = useParams<{ documentId: string }>();
  const [lang, setLang] = useState<Language>(() => {
    const stored = localStorage.getItem('lang');
    return stored === 'en' ? 'en' : 'ar';
  });

  const t = labels[lang];
  const isRtl = lang === 'ar';

  const { data, isLoading, isError } = useQuery({
    queryKey: ['verify', documentId],
    queryFn: () => verifyApi.verify(documentId!),
    enabled: !!documentId,
    retry: false,
  });

  const toggleLang = () => {
    setLang((prev) => (prev === 'ar' ? 'en' : 'ar'));
  };

  const status = data?.status ?? 'invalid';
  const config = statusConfig[status];
  const StatusIcon = config.icon;

  return (
    <div className="min-h-screen bg-neutral-50 flex flex-col" dir={isRtl ? 'rtl' : 'ltr'}>
      {/* Header */}
      <header className="bg-white border-b border-neutral-200">
        <div className="max-w-3xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-10 h-10 bg-primary rounded-lg">
              <span className="text-lg font-bold text-white">B</span>
            </div>
            <div>
              <h1 className="text-base font-bold text-neutral-900">
                {isRtl ? 'مستشفى البدور' : 'Al-Badour Hospital'}
              </h1>
              <p className="text-xs text-neutral-500">
                {isRtl ? 'Al-Badour Hospital' : 'مستشفى البدور'}
              </p>
            </div>
          </div>
          <button
            onClick={toggleLang}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-neutral-600 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
          >
            <Globe size={16} />
            {t.toggleLang}
          </button>
        </div>
      </header>

      {/* Content */}
      <main className="flex-1 flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-lg">
          {/* Page title */}
          <h2 className="text-center text-lg font-semibold text-neutral-700 mb-6">
            {t.title}
          </h2>

          {isLoading ? (
            /* Loading state */
            <div className="bg-white rounded-2xl shadow-lg border border-neutral-200 p-12 text-center">
              <div className="w-10 h-10 border-4 border-primary-200 border-t-primary rounded-full animate-spin mx-auto mb-4" />
              <p className="text-sm text-neutral-500">{t.loading}</p>
            </div>
          ) : isError || !data ? (
            /* Error / Invalid fallback */
            <div className="bg-white rounded-2xl shadow-lg border border-neutral-200 p-8 text-center">
              <div className="inline-flex items-center justify-center w-16 h-16 bg-neutral-100 rounded-full mb-4">
                <ShieldQuestion size={32} className="text-neutral-500" />
              </div>
              <h3 className="text-xl font-bold text-neutral-900 mb-2">
                {t.invalid}
              </h3>
              <p className="text-neutral-500 text-sm">
                {t.invalidMessage}
              </p>
            </div>
          ) : (
            /* Verification Result */
            <div className="bg-white rounded-2xl shadow-lg border border-neutral-200 overflow-hidden">
              {/* Status banner */}
              <div className={cn('p-8 border-b text-center', config.bgClass)}>
                <div className="inline-flex items-center justify-center w-16 h-16 bg-white rounded-full shadow-sm mb-4">
                  <StatusIcon size={32} className={config.colorClass} />
                </div>
                <h3 className="text-xl font-bold text-neutral-900">
                  {status === 'verified' && t.verified}
                  {status === 'revoked' && t.revoked}
                  {status === 'invalid' && t.invalid}
                  {status === 'expired' && t.expired}
                </h3>
                <p className="text-sm text-neutral-600 mt-1">
                  {status === 'verified' && t.verifiedMessage}
                  {status === 'revoked' && t.revokedMessage}
                  {status === 'invalid' && t.invalidMessage}
                  {status === 'expired' && t.expiredMessage}
                </p>
              </div>

              {/* Document details (for verified and revoked) */}
              {(status === 'verified' || status === 'revoked') && (
                <div className="p-6 space-y-0">
                  {data.documentNumber && (
                    <DetailRow label={t.documentNumber} value={data.documentNumber} mono />
                  )}
                  {data.patientName && (
                    <DetailRow label={t.patientName} value={data.patientName} />
                  )}
                  {data.recipientEntity && (
                    <DetailRow label={t.recipientEntity} value={data.recipientEntity} />
                  )}
                  {data.issuedAt && (
                    <DetailRow
                      label={t.issuedDate}
                      value={formatVerificationDate(data.issuedAt, lang)}
                    />
                  )}

                  {/* Revocation details */}
                  {status === 'revoked' && (
                    <>
                      {data.revokedAt && (
                        <DetailRow
                          label={t.revokedDate}
                          value={formatVerificationDate(data.revokedAt, lang)}
                        />
                      )}
                      {data.revocationReason && (
                        <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg">
                          <p className="text-xs font-medium text-red-700 mb-1">{t.revocationReason}</p>
                          <p className="text-sm text-red-800">{data.revocationReason}</p>
                        </div>
                      )}
                    </>
                  )}

                  {/* Replacement link */}
                  {data.replacementDocumentId && (
                    <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                      <p className="text-sm text-blue-800 mb-2">{t.replacementAvailable}</p>
                      <Link
                        to={`/verify/${data.replacementDocumentId}`}
                        className="text-sm text-blue-700 font-medium hover:underline"
                      >
                        {t.viewReplacement}
                        {data.replacementDocumentNumber && (
                          <span> ({data.replacementDocumentNumber})</span>
                        )}
                      </Link>
                    </div>
                  )}

                  {/* PDF downloads */}
                  {status === 'verified' && documentId && (data.hasPdf || data.hasAccountStatement) && (
                    <div className="mt-6 pt-4 space-y-3">
                      {data.hasPdf && (
                        <a
                          href={verifyApi.getPdfUrl(documentId)}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="flex items-center justify-center gap-2 w-full py-3 px-4 bg-primary hover:bg-primary-700 text-white text-sm font-medium rounded-lg transition-colors"
                        >
                          <FileDown size={18} />
                          {t.downloadPdf}
                        </a>
                      )}
                      {data.hasAccountStatement && (
                        <a
                          href={verifyApi.getAccountStatementUrl(documentId)}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="flex items-center justify-center gap-2 w-full py-3 px-4 bg-purple-600 hover:bg-purple-700 text-white text-sm font-medium rounded-lg transition-colors"
                        >
                          <FileDown size={18} />
                          {t.downloadAccountStatement}
                        </a>
                      )}
                    </div>
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t border-neutral-200 bg-white px-6 py-4">
        <p className="text-center text-xs text-neutral-400">
          {t.footer}
        </p>
      </footer>
    </div>
  );
}

function DetailRow({
  label,
  value,
  mono = false,
}: {
  label: string;
  value: string;
  mono?: boolean;
}) {
  return (
    <div className="flex justify-between items-center py-3 border-b border-neutral-100 last:border-0">
      <span className="text-sm text-neutral-500">{label}</span>
      <span
        className={cn(
          'text-sm font-medium text-neutral-900',
          mono && 'font-mono'
        )}
      >
        {value}
      </span>
    </div>
  );
}
