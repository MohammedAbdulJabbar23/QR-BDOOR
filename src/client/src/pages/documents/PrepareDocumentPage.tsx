import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation } from '@tanstack/react-query';
import { ArrowLeft, ArrowRight, FileText } from 'lucide-react';
import { requestsApi } from '@/api/requests.api';
import { documentsApi } from '@/api/documents.api';
import { useUiStore } from '@/stores/uiStore';
import { cn } from '@/utils/cn';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import PageHeader from '@/components/common/PageHeader';

export default function PrepareDocumentPage() {
  const { requestId } = useParams<{ requestId: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const language = useUiStore((s) => s.language);
  const isRtl = language === 'ar';

  const [documentBody, setDocumentBody] = useState('');
  const [error, setError] = useState('');

  const { data: request, isLoading: requestLoading } = useQuery({
    queryKey: ['request', requestId],
    queryFn: () => requestsApi.getById(requestId!),
    enabled: !!requestId,
  });

  const prepareMutation = useMutation({
    mutationFn: (data: { requestId: string; documentBody?: string }) =>
      documentsApi.prepare(data),
    onSuccess: (doc) => {
      navigate(`/documents/${doc.id}`);
    },
    onError: () => {
      setError(t('common.error'));
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!requestId) return;
    setError('');
    prepareMutation.mutate({
      requestId,
      documentBody: documentBody.trim() || undefined,
    });
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  if (requestLoading) {
    return <LoadingSpinner />;
  }

  if (!request) {
    return (
      <div className="text-center py-12">
        <p className="text-neutral-500">{t('common.noData')}</p>
      </div>
    );
  }

  const patientName = language === 'ar'
    ? request.patientName
    : (request.patientNameEn || request.patientName);

  const documentType = language === 'ar'
    ? request.documentTypeNameAr
    : request.documentTypeNameEn;

  return (
    <div>
      <PageHeader
        title={t('documents.prepare')}
        actions={
          <button
            onClick={() => navigate(-1)}
            className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-neutral-700 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
          >
            <BackIcon size={16} />
            {t('common.cancel')}
          </button>
        }
      />

      {/* Request Info */}
      <div className="bg-white rounded-xl border border-neutral-200 p-6 mb-6">
        <div className="flex items-center gap-2 mb-4">
          <FileText size={20} className="text-primary" />
          <h2 className="text-lg font-semibold text-neutral-900">
            {language === 'ar' ? 'معلومات الطلب' : 'Request Information'}
          </h2>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <p className="text-xs font-medium text-neutral-500 mb-1">
              {t('requests.patientName')}
            </p>
            <p className="text-sm font-medium text-neutral-900">{patientName}</p>
          </div>
          <div>
            <p className="text-xs font-medium text-neutral-500 mb-1">
              {t('requests.recipientEntity')}
            </p>
            <p className="text-sm font-medium text-neutral-900">{request.recipientEntity}</p>
          </div>
          <div>
            <p className="text-xs font-medium text-neutral-500 mb-1">
              {t('requests.documentType')}
            </p>
            <p className="text-sm font-medium text-neutral-900">{documentType}</p>
          </div>
        </div>
      </div>

      {/* Prepare Form */}
      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-neutral-200 p-6">
        <div className="mb-6">
          <label
            htmlFor="documentBody"
            className="block text-sm font-medium text-neutral-700 mb-2"
          >
            {t('documents.documentBody')}
          </label>
          <div className="grid">
            <textarea
              id="documentBody"
              value={documentBody}
              onChange={(e) => setDocumentBody(e.target.value)}
              rows={6}
              className={cn(
                'w-full px-4 py-3 border border-neutral-300 rounded-lg text-sm',
                'focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary',
                'placeholder:text-neutral-400',
                '[grid-area:1/1/2/2] resize-none overflow-hidden'
              )}
              placeholder={
                language === 'ar'
                  ? 'أدخل محتوى الوثيقة (اختياري)...'
                  : 'Enter document content (optional)...'
              }
            />
            <div
              className={cn(
                'w-full px-4 py-3 border border-transparent rounded-lg text-sm',
                'whitespace-pre-wrap break-words invisible',
                '[grid-area:1/1/2/2] min-h-[150px]'
              )}
              aria-hidden="true"
            >
              {documentBody + ' '}
            </div>
          </div>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-danger-light text-red-700 text-sm rounded-lg">
            {error}
          </div>
        )}

        <div className="flex items-center justify-end gap-3">
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="px-4 py-2 text-sm font-medium text-neutral-700 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
          >
            {t('common.cancel')}
          </button>
          <button
            type="submit"
            disabled={prepareMutation.isPending}
            className={cn(
              'px-6 py-2 text-sm font-medium text-white rounded-lg transition-colors',
              'bg-primary hover:bg-primary-700',
              'disabled:opacity-50 disabled:cursor-not-allowed'
            )}
          >
            {prepareMutation.isPending
              ? t('common.loading')
              : t('documents.prepare')}
          </button>
        </div>
      </form>
    </div>
  );
}
