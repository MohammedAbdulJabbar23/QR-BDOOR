import { useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, ArrowRight, Pencil, Trash2, Check, X, FileText } from 'lucide-react';
import { requestsApi } from '@/api/requests.api';
import { documentsApi } from '@/api/documents.api';
import { documentTypesApi } from '@/api/documentTypes.api';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { formatDateTime } from '@/utils/formatters';
import { canAcceptRejectRequest, canEditRequest, canPrepareDocument } from '@/utils/permissions';
import { getTableVariantOptions, getGenericDocTypeName } from '@/utils/documentTypeFilters';
import PageHeader from '@/components/common/PageHeader';
import StatusBadge from '@/components/common/StatusBadge';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import ConfirmDialog from '@/components/common/ConfirmDialog';

export default function RequestDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const language = useUiStore((s) => s.language);
  const isArabic = language === 'ar';
  const isRtl = language === 'ar';

  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showRejectDialog, setShowRejectDialog] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');
  const [showAcceptDialog, setShowAcceptDialog] = useState(false);
  const [selectedDocTypeId, setSelectedDocTypeId] = useState('');

  // Fetch request details
  const {
    data: request,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['request', id],
    queryFn: () => requestsApi.getById(id!),
    enabled: !!id,
  });

  // Fetch linked documents
  const { data: linkedDocuments } = useQuery({
    queryKey: ['documents', 'byRequest', id],
    queryFn: () => documentsApi.getByRequest(id!),
    enabled: !!id,
  });

  // Mutations
  const deleteMutation = useMutation({
    mutationFn: () => requestsApi.delete(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['requests'] });
      navigate('/requests');
    },
  });

  const acceptMutation = useMutation({
    mutationFn: (docTypeId?: string) => requestsApi.accept(id!, docTypeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['request', id] });
      queryClient.invalidateQueries({ queryKey: ['requests'] });
      queryClient.invalidateQueries({ queryKey: ['pendingRequests'] });
      setShowAcceptDialog(false);
      setSelectedDocTypeId('');
    },
  });

  const rejectMutation = useMutation({
    mutationFn: (reason: string) => requestsApi.reject(id!, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['request', id] });
      queryClient.invalidateQueries({ queryKey: ['requests'] });
      queryClient.invalidateQueries({ queryKey: ['pendingRequests'] });
      setShowRejectDialog(false);
      setRejectionReason('');
    },
  });

  const department = user?.department || '';
  const isMedicalReport = request?.documentTypeNameEn?.toLowerCase().includes('medical report') ?? false;
  const needsTableChoice = department === 'Statistics' && isMedicalReport;

  const { data: allDocumentTypes } = useQuery({
    queryKey: ['documentTypes', true],
    queryFn: () => documentTypesApi.getAll(true),
    enabled: needsTableChoice,
  });

  const tableVariantOptions = useMemo(
    () => allDocumentTypes && request
      ? getTableVariantOptions(allDocumentTypes, request.documentTypeNameEn)
      : [],
    [allDocumentTypes, request],
  );

  if (isLoading) return <LoadingSpinner />;

  if (isError || !request) {
    return (
      <div className="text-center py-12">
        <p className="text-neutral-500">{t('common.error')}</p>
        <button
          onClick={() => navigate('/requests')}
          className="mt-4 text-sm text-primary hover:underline"
        >
          {t('requests.title')}
        </button>
      </div>
    );
  }

  const userId = user?.id || '';
  const role = user?.role || '';
  const isAdminOrSupervisor = role === 'Admin' || role === 'Supervisor';
  const isClosed = request.status === 'Completed' || request.status === 'Rejected';
  const showDelete = isAdminOrSupervisor && !isClosed;
  const showEdit = canEditRequest(
    department,
    request.status,
    request.createdById,
    userId,
    request.documentTypeNameEn,
  );
  const showAcceptReject = request.status === 'Pending'
    && canAcceptRejectRequest(department, request.documentTypeNameEn);
  const showPrepareDoc = request.status === 'InProgress'
    && canPrepareDocument(department, request.documentTypeNameEn);
  const isAdminLetter = request.documentTypeNameEn?.toLowerCase() === 'administrative letter';

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  return (
    <div>
      <PageHeader
        title={t('requests.title')}
        actions={
          <button
            onClick={() => navigate('/requests')}
            className="flex items-center gap-2 px-3 py-2 text-sm text-neutral-600 hover:bg-neutral-100 rounded-lg transition-colors"
          >
            <BackIcon size={16} />
          </button>
        }
      />

      {/* Request Details Card */}
      <div className="bg-white rounded-xl border border-neutral-200 p-6 mb-6">
        <div className="flex items-start justify-between mb-6">
          <div className="flex items-center gap-3">
            <StatusBadge status={request.status} />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {!isAdminLetter && (
            <>
              <div>
                <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
                  {t('requests.patientName')}
                </span>
                <p className="mt-1 text-neutral-800 font-medium">{request.patientName || '-'}</p>
              </div>

              <div>
                <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
                  {t('requests.patientNameEn')}
                </span>
                <p className="mt-1 text-neutral-800">
                  {request.patientNameEn || '-'}
                </p>
              </div>
            </>
          )}

          {/* Recipient Entity */}
          <div>
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {t('requests.recipientEntity')}
            </span>
            <p className="mt-1 text-neutral-800">{request.recipientEntity}</p>
          </div>

          {/* Document Type */}
          <div>
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {t('requests.documentType')}
            </span>
            <p className="mt-1 text-neutral-800">
              {department === 'Inquiry' || request.status === 'Pending'
                ? getGenericDocTypeName(request.documentTypeNameAr, request.documentTypeNameEn, isArabic)
                : (isArabic ? request.documentTypeNameAr : request.documentTypeNameEn)}
            </p>
          </div>

          {/* Created By */}
          <div>
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {isArabic ? 'أنشئ بواسطة' : 'Created By'}
            </span>
            <p className="mt-1 text-neutral-800">{request.createdByName}</p>
          </div>

          {/* Assigned To */}
          <div>
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {isArabic ? 'مسند إلى' : 'Assigned To'}
            </span>
            <p className="mt-1 text-neutral-800">{request.assignedToName || '-'}</p>
          </div>

          {/* Created At */}
          <div>
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {isArabic ? 'تاريخ الإنشاء' : 'Created At'}
            </span>
            <p className="mt-1 text-neutral-800">
              {formatDateTime(request.createdAt, language)}
            </p>
          </div>

          {/* Updated At */}
          <div>
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {isArabic ? 'آخر تحديث' : 'Last Updated'}
            </span>
            <p className="mt-1 text-neutral-800">
              {formatDateTime(request.updatedAt, language)}
            </p>
          </div>
        </div>

        {/* Language */}
        {request.language && !isAdminLetter && (
          <div className="mt-6 pt-6 border-t border-neutral-100">
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {t('requests.language')}
            </span>
            <p className="mt-1 text-neutral-800">
              {request.language === 'English' ? t('requests.languageEnglish') : t('requests.languageArabic')}
            </p>
          </div>
        )}

        {/* Notes */}
        {request.notes && (
          <div className="mt-6 pt-6 border-t border-neutral-100">
            <span className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
              {isAdminLetter ? t('requests.topic') : t('requests.notes')}
            </span>
            <p className="mt-1 text-neutral-700 whitespace-pre-wrap">{request.notes}</p>
          </div>
        )}

        {/* Rejection Reason */}
        {request.status === 'Rejected' && request.rejectionReason && (
          <div className="mt-6 pt-6 border-t border-neutral-100">
            <span className="text-xs font-medium text-red-500 uppercase tracking-wider">
              {t('requests.rejectionReason')}
            </span>
            <p className="mt-1 text-red-700 bg-red-50 p-3 rounded-lg">
              {request.rejectionReason}
            </p>
          </div>
        )}

        {/* Action Buttons */}
        {(showEdit || showDelete || showAcceptReject || showPrepareDoc) && (
          <div className="mt-6 pt-6 border-t border-neutral-100 flex flex-wrap gap-3">
            {showEdit && (
              <button
                onClick={() => navigate(`/requests/create?edit=${request.id}`)}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors text-neutral-700"
              >
                <Pencil size={16} />
                {t('requests.edit')}
              </button>
            )}

            {showDelete && (
              <button
                onClick={() => setShowDeleteDialog(true)}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium border border-red-200 rounded-lg hover:bg-red-50 transition-colors text-red-600"
              >
                <Trash2 size={16} />
                {t('common.delete')}
              </button>
            )}

            {showAcceptReject && (
              <>
                <button
                  onClick={() => {
                    if (needsTableChoice) {
                      setSelectedDocTypeId(request.documentTypeId);
                      setShowAcceptDialog(true);
                    } else {
                      acceptMutation.mutate(undefined);
                    }
                  }}
                  disabled={acceptMutation.isPending}
                  className="flex items-center gap-2 px-4 py-2 text-sm font-medium bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors disabled:opacity-60"
                >
                  <Check size={16} />
                  {t('requests.accept')}
                </button>
                <button
                  onClick={() => setShowRejectDialog(true)}
                  className="flex items-center gap-2 px-4 py-2 text-sm font-medium bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
                >
                  <X size={16} />
                  {t('requests.reject')}
                </button>
              </>
            )}

            {showPrepareDoc && (
              <button
                onClick={() => navigate(`/documents/prepare/${request.id}`)}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-700 transition-colors"
              >
                <FileText size={16} />
                {t('documents.prepare')}
              </button>
            )}
          </div>
        )}
      </div>

      {/* Linked Documents */}
      {linkedDocuments && linkedDocuments.length > 0 && (
        <div className="bg-white rounded-xl border border-neutral-200 p-6">
          <h2 className="text-lg font-semibold text-neutral-800 mb-4">
            {t('documents.title')}
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {linkedDocuments.map((doc) => (
              <button
                key={doc.id}
                onClick={() => navigate(`/documents/${doc.id}`)}
                className="text-start p-4 border border-neutral-200 rounded-lg hover:border-primary/30 hover:bg-primary-50/30 transition-colors group"
              >
                <div className="flex items-center gap-3 mb-2">
                  <FileText
                    size={20}
                    className="text-neutral-400 group-hover:text-primary transition-colors"
                  />
                  <span className="text-sm font-semibold text-neutral-800">
                    {doc.documentNumber}
                  </span>
                </div>
                <p className="text-xs text-neutral-500">
                  {department === 'Inquiry'
                    ? getGenericDocTypeName(doc.documentTypeNameAr, doc.documentTypeNameEn, isArabic)
                    : (isArabic ? doc.documentTypeNameAr : doc.documentTypeNameEn)}
                </p>
                <div className="mt-2">
                  <StatusBadge status={doc.status} />
                </div>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={showDeleteDialog}
        title={t('common.areYouSure')}
        message={t('common.thisActionCannot')}
        variant="danger"
        confirmLabel={t('common.delete')}
        onConfirm={() => deleteMutation.mutate()}
        onCancel={() => setShowDeleteDialog(false)}
      />

      {/* Accept Dialog (Statistics picks table variant) */}
      <ConfirmDialog
        open={showAcceptDialog}
        title={t('requests.accept')}
        message={isArabic ? 'اختر نوع التقرير' : 'Select report format'}
        variant="primary"
        confirmLabel={t('requests.accept')}
        onConfirm={() => {
          if (selectedDocTypeId) {
            acceptMutation.mutate(selectedDocTypeId);
          }
        }}
        onCancel={() => {
          setShowAcceptDialog(false);
          setSelectedDocTypeId('');
        }}
      >
        <select
          value={selectedDocTypeId}
          onChange={(e) => setSelectedDocTypeId(e.target.value)}
          className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary bg-white"
        >
          {tableVariantOptions.map((dt) => (
            <option key={dt.id} value={dt.id}>
              {isArabic ? dt.nameAr : dt.nameEn}
            </option>
          ))}
        </select>
      </ConfirmDialog>

      {/* Reject Dialog */}
      <ConfirmDialog
        open={showRejectDialog}
        title={t('requests.reject')}
        message={t('requests.rejectionReason')}
        variant="danger"
        confirmLabel={t('requests.reject')}
        onConfirm={() => {
          if (rejectionReason.trim()) {
            rejectMutation.mutate(rejectionReason.trim());
          }
        }}
        onCancel={() => {
          setShowRejectDialog(false);
          setRejectionReason('');
        }}
      >
        <textarea
          value={rejectionReason}
          onChange={(e) => setRejectionReason(e.target.value)}
          rows={3}
          className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary resize-none"
          placeholder={t('requests.rejectionReason')}
          autoFocus
        />
      </ConfirmDialog>
    </div>
  );
}
