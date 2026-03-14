import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft, ArrowRight, FileText, Printer, Ban, Trash2,
  FileDown, ExternalLink, Upload,
} from 'lucide-react';
import { documentsApi } from '@/api/documents.api';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { canUploadPdf, canRevokeDocument, canDeleteDocument } from '@/utils/permissions';
import { formatDateTime } from '@/utils/formatters';
import { cn } from '@/utils/cn';
import PageHeader from '@/components/common/PageHeader';
import StatusBadge from '@/components/common/StatusBadge';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import ConfirmDialog from '@/components/common/ConfirmDialog';

export default function DocumentDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const language = useUiStore((s) => s.language);
  const isRtl = language === 'ar';

  const [qrImageUrl, setQrImageUrl] = useState<string | null>(null);
  const [showRevokeDialog, setShowRevokeDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [revokeReason, setRevokeReason] = useState('');
  const [generateError, setGenerateError] = useState('');
  const [uploadError, setUploadError] = useState('');
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { data: document, isLoading } = useQuery({
    queryKey: ['document', id],
    queryFn: () => documentsApi.getById(id!),
    enabled: !!id,
  });

  // Fetch QR code image
  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    let objectUrl: string | null = null;

    documentsApi.getQrImage(id).then((blob: Blob) => {
      if (cancelled) return;
      objectUrl = URL.createObjectURL(blob);
      setQrImageUrl(objectUrl);
    }).catch(() => {
      // QR image not available
    });

    return () => {
      cancelled = true;
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [id]);

  const generateMutation = useMutation({
    mutationFn: () => documentsApi.generatePdf(id!),
    onSuccess: () => {
      setGenerateError('');
      queryClient.invalidateQueries({ queryKey: ['document', id] });
    },
    onError: () => {
      setGenerateError(t('common.error'));
    },
  });

  const uploadMutation = useMutation({
    mutationFn: (file: File) => documentsApi.uploadPdf(id!, file),
    onSuccess: () => {
      setUploadError('');
      queryClient.invalidateQueries({ queryKey: ['document', id] });
    },
    onError: () => {
      setUploadError(t('common.error'));
    },
  });

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      if (file.size > 10 * 1024 * 1024) {
        setUploadError(t('documents.fileTooLarge'));
        e.target.value = '';
        return;
      }
      uploadMutation.mutate(file);
    }
    e.target.value = '';
  };

  const revokeMutation = useMutation({
    mutationFn: (reason: string) => documentsApi.revoke(id!, reason),
    onSuccess: () => {
      setShowRevokeDialog(false);
      setRevokeReason('');
      queryClient.invalidateQueries({ queryKey: ['document', id] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => documentsApi.delete(id!),
    onSuccess: () => {
      navigate('/documents');
    },
  });

  const handleRevoke = () => {
    if (!revokeReason.trim()) return;
    revokeMutation.mutate(revokeReason.trim());
  };

  const handleDelete = () => {
    deleteMutation.mutate();
  };

  const handlePrint = async () => {
    if (!document?.hasPdf) {
      window.print();
      return;
    }
    const res = await documentsApi.getPdf(document.id);
    const blob = new Blob([res.data], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);
    const iframe = window.document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = url;
    window.document.body.appendChild(iframe);
    iframe.onload = () => {
      iframe.contentWindow?.print();
      setTimeout(() => {
        window.document.body.removeChild(iframe);
        URL.revokeObjectURL(url);
      }, 60000);
    };
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!document) {
    return (
      <div className="text-center py-12">
        <p className="text-neutral-500">{t('common.noData')}</p>
      </div>
    );
  }

  const patientName = language === 'ar'
    ? document.patientName
    : (document.patientNameEn || document.patientName);

  const documentType = language === 'ar'
    ? document.documentTypeNameAr
    : document.documentTypeNameEn;

  const isRevoked = document.status === 'Revoked';
  const isDraft = document.status === 'Draft';
  const isStatistics = user && canUploadPdf(user.department);
  const showGenerate = isStatistics && isDraft && !isRevoked;
  const showUpload = isStatistics && isDraft && !isRevoked && document.hasPdf;
  const showRevoke = user && canRevokeDocument(user.role) && !isRevoked;
  const showDelete = user && canDeleteDocument(user.role);

  return (
    <div>
      <PageHeader
        title={t('documents.documentNumber')}
        subtitle={document.documentNumber}
        actions={
          <button
            onClick={() => navigate('/documents')}
            className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-neutral-700 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
          >
            <BackIcon size={16} />
            {language === 'ar' ? 'العودة' : 'Back'}
          </button>
        }
      />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Info */}
        <div className="lg:col-span-2 space-y-6">
          {/* Document Header Card */}
          <div className="bg-white rounded-xl border border-neutral-200 p-6">
            <div className="flex items-start justify-between mb-6">
              <div>
                <p className="text-sm text-neutral-500 mb-1">{t('documents.documentNumber')}</p>
                <p className="text-2xl font-bold text-neutral-900">{document.documentNumber}</p>
              </div>
              <StatusBadge status={document.status} />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <InfoItem
                label={t('requests.patientName')}
                value={patientName}
              />
              <InfoItem
                label={t('requests.recipientEntity')}
                value={document.recipientEntity}
              />
              <InfoItem
                label={t('requests.documentType')}
                value={documentType}
              />
              <InfoItem
                label={language === 'ar' ? 'صادرة بواسطة' : 'Issued By'}
                value={document.issuedByName}
              />
              <InfoItem
                label={language === 'ar' ? 'تاريخ الإصدار' : 'Issued Date'}
                value={formatDateTime(document.issuedAt, language)}
              />
              {document.archivedAt && (
                <InfoItem
                  label={language === 'ar' ? 'تاريخ الأرشفة' : 'Archived Date'}
                  value={formatDateTime(document.archivedAt, language)}
                />
              )}
              {document.patientGender && (
                <InfoItem label={t('documents.patientGender')} value={document.patientGender} />
              )}
              {document.patientProfession && (
                <InfoItem label={t('documents.patientProfession')} value={document.patientProfession} />
              )}
              {document.patientAge && (
                <InfoItem label={t('documents.patientAge')} value={document.patientAge} />
              )}
              {document.admissionDate && (
                <InfoItem label={t('documents.admissionDate')} value={document.admissionDate} />
              )}
              {document.dischargeDate && (
                <InfoItem label={t('documents.dischargeDate')} value={document.dischargeDate} />
              )}
              {document.leaveGranted && (
                <InfoItem label={t('documents.leaveGranted')} value={document.leaveGranted} />
              )}
            </div>
          </div>

          {/* Document Body */}
          {document.documentBody && (
            <div className="bg-white rounded-xl border border-neutral-200 p-6">
              <h3 className="text-base font-semibold text-neutral-900 mb-4">
                {t('documents.documentBody')}
              </h3>
              <div className="p-4 bg-neutral-50 rounded-lg border border-neutral-100 whitespace-pre-wrap break-words text-sm leading-relaxed text-neutral-800">
                {document.documentBody}
              </div>
            </div>
          )}

          {/* Revocation Info */}
          {isRevoked && (
            <div className="bg-red-50 rounded-xl border border-red-200 p-6">
              <h3 className="text-base font-semibold text-red-800 mb-4">
                {language === 'ar' ? 'معلومات الإلغاء' : 'Revocation Information'}
              </h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {document.revokedByName && (
                  <InfoItem
                    label={language === 'ar' ? 'ملغاة بواسطة' : 'Revoked By'}
                    value={document.revokedByName}
                  />
                )}
                {document.revokedAt && (
                  <InfoItem
                    label={language === 'ar' ? 'تاريخ الإلغاء' : 'Revoked Date'}
                    value={formatDateTime(document.revokedAt, language)}
                  />
                )}
                {document.revocationReason && (
                  <div className="sm:col-span-2">
                    <InfoItem
                      label={t('documents.revocationReason')}
                      value={document.revocationReason}
                    />
                  </div>
                )}
              </div>
              {document.replacementDocumentId && (
                <div className="mt-4 pt-4 border-t border-red-200">
                  <p className="text-sm text-red-700 mb-2">
                    {language === 'ar' ? 'وثيقة بديلة:' : 'Replacement Document:'}
                  </p>
                  <Link
                    to={`/documents/${document.replacementDocumentId}`}
                    className="inline-flex items-center gap-1.5 text-sm font-medium text-primary hover:underline"
                  >
                    <ExternalLink size={14} />
                    {document.replacementDocumentNumber || document.replacementDocumentId}
                  </Link>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* QR Code */}
          <div className="bg-white rounded-xl border border-neutral-200 p-6">
            <h3 className="text-base font-semibold text-neutral-900 mb-4">
              {t('documents.qrCode')}
            </h3>
            {qrImageUrl ? (
              <div className="flex flex-col items-center">
                <img
                  src={qrImageUrl}
                  alt="QR Code"
                  className="w-48 h-48 mb-4"
                />
              </div>
            ) : (
              <div className="flex items-center justify-center w-48 h-48 mx-auto mb-4 bg-neutral-100 rounded-lg">
                <p className="text-sm text-neutral-400">{t('common.loading')}</p>
              </div>
            )}
            {document.qrCodeUrl && (
              <div className="mt-2">
                <p className="text-xs text-neutral-500 mb-1">
                  {language === 'ar' ? 'رابط التحقق' : 'Verification URL'}
                </p>
                <p className="text-xs text-neutral-700 break-all bg-neutral-50 p-2 rounded">
                  {document.qrCodeUrl}
                </p>
              </div>
            )}
            {document.qrExpiresAt && (
              <p className="mt-2 text-xs text-neutral-500">
                {language === 'ar' ? 'ينتهي:' : 'Expires:'}{' '}
                {formatDateTime(document.qrExpiresAt, language)}
              </p>
            )}
          </div>

          {/* Actions */}
          <div className="bg-white rounded-xl border border-neutral-200 p-6">
            <h3 className="text-base font-semibold text-neutral-900 mb-4">
              {t('requests.actions')}
            </h3>
            <div className="flex flex-col gap-2">
              {/* Generate PDF */}
              {showGenerate && (
                <>
                  <button
                    onClick={() => generateMutation.mutate()}
                    disabled={generateMutation.isPending}
                    className={cn(
                      'flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors w-full',
                      'bg-primary text-white hover:bg-primary-700',
                      'disabled:opacity-50 disabled:cursor-not-allowed'
                    )}
                  >
                    <FileText size={16} />
                    {generateMutation.isPending
                      ? t('common.loading')
                      : t('documents.generatePdf')}
                  </button>
                  {generateError && (
                    <p className="text-xs text-red-600">{generateError}</p>
                  )}
                  {generateMutation.isSuccess && (
                    <p className="text-xs text-green-600">{t('documents.pdfGenerated')}</p>
                  )}
                </>
              )}

              {/* Upload signed PDF (visible after generating) */}
              {showUpload && (
                <>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept=".pdf"
                    className="hidden"
                    onChange={handleFileSelect}
                  />
                  <button
                    onClick={() => fileInputRef.current?.click()}
                    disabled={uploadMutation.isPending}
                    className={cn(
                      'flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors w-full',
                      'bg-emerald-600 text-white hover:bg-emerald-700',
                      'disabled:opacity-50 disabled:cursor-not-allowed'
                    )}
                  >
                    <Upload size={16} />
                    {uploadMutation.isPending
                      ? t('common.loading')
                      : t('documents.uploadPdf')}
                  </button>
                  {uploadError && (
                    <p className="text-xs text-red-600">{uploadError}</p>
                  )}
                  {uploadMutation.isSuccess && (
                    <p className="text-xs text-green-600">{t('documents.pdfUploaded')}</p>
                  )}
                </>
              )}

              {/* View/Download PDF */}
              {document.hasPdf && (
                <button
                  onClick={async () => {
                    const res = await documentsApi.getPdf(document.id);
                    const blob = new Blob([res.data], { type: 'application/pdf' });
                    const url = URL.createObjectURL(blob);
                    window.open(url, '_blank');
                    setTimeout(() => URL.revokeObjectURL(url), 60000);
                  }}
                  className="flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors w-full bg-blue-600 text-white hover:bg-blue-700"
                >
                  <FileDown size={16} />
                  {language === 'ar' ? 'عرض / تحميل PDF' : 'View / Download PDF'}
                </button>
              )}

              {/* Print */}
              <button
                onClick={handlePrint}
                className="flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors w-full border border-neutral-300 text-neutral-700 hover:bg-neutral-50"
              >
                <Printer size={16} />
                {t('documents.print')}
              </button>

              {/* Revoke */}
              {showRevoke && (
                <button
                  onClick={() => setShowRevokeDialog(true)}
                  className="flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors w-full border border-red-300 text-red-700 hover:bg-red-50"
                >
                  <Ban size={16} />
                  {t('documents.revoke')}
                </button>
              )}

              {/* Delete */}
              {showDelete && (
                <button
                  onClick={() => setShowDeleteDialog(true)}
                  className="flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors w-full bg-red-600 text-white hover:bg-red-700"
                >
                  <Trash2 size={16} />
                  {t('common.delete')}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Revoke Dialog */}
      <ConfirmDialog
        open={showRevokeDialog}
        title={t('documents.revoke')}
        message={t('common.areYouSure')}
        confirmLabel={t('documents.revoke')}
        variant="danger"
        onConfirm={handleRevoke}
        onCancel={() => {
          setShowRevokeDialog(false);
          setRevokeReason('');
        }}
      >
        <textarea
          value={revokeReason}
          onChange={(e) => setRevokeReason(e.target.value)}
          rows={3}
          placeholder={t('documents.revocationReason')}
          className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary resize-none"
        />
      </ConfirmDialog>

      {/* Delete Dialog */}
      <ConfirmDialog
        open={showDeleteDialog}
        title={t('common.delete')}
        message={t('common.thisActionCannot')}
        confirmLabel={t('common.delete')}
        variant="danger"
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteDialog(false)}
      />
    </div>
  );
}

function InfoItem({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs font-medium text-neutral-500 mb-1">{label}</p>
      <p className="text-sm text-neutral-900">{value}</p>
    </div>
  );
}
