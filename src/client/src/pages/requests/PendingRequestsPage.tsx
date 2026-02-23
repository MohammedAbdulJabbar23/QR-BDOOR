import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Check, X, Eye } from 'lucide-react';
import { requestsApi } from '@/api/requests.api';
import { useUiStore } from '@/stores/uiStore';
import { formatDate } from '@/utils/formatters';
import PageHeader from '@/components/common/PageHeader';
import StatusBadge from '@/components/common/StatusBadge';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import EmptyState from '@/components/common/EmptyState';
import ConfirmDialog from '@/components/common/ConfirmDialog';

export default function PendingRequestsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const language = useUiStore((s) => s.language);
  const isArabic = language === 'ar';

  const [rejectTarget, setRejectTarget] = useState<string | null>(null);
  const [rejectionReason, setRejectionReason] = useState('');

  const { data: requests, isLoading } = useQuery({
    queryKey: ['pendingRequests'],
    queryFn: () => requestsApi.getPending(),
  });

  const acceptMutation = useMutation({
    mutationFn: (id: string) => requestsApi.accept(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pendingRequests'] });
      queryClient.invalidateQueries({ queryKey: ['requests'] });
    },
  });

  const rejectMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      requestsApi.reject(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pendingRequests'] });
      queryClient.invalidateQueries({ queryKey: ['requests'] });
      setRejectTarget(null);
      setRejectionReason('');
    },
  });

  const handleRejectConfirm = () => {
    if (rejectTarget && rejectionReason.trim()) {
      rejectMutation.mutate({ id: rejectTarget, reason: rejectionReason.trim() });
    }
  };

  return (
    <div>
      <PageHeader title={t('dashboard.pendingRequests')} />

      {isLoading ? (
        <LoadingSpinner />
      ) : !requests || requests.length === 0 ? (
        <EmptyState title={t('requests.noRequests')} />
      ) : (
        <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-neutral-200 bg-neutral-50">
                  <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                    {t('requests.patientName')}
                  </th>
                  <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                    {t('requests.documentType')}
                  </th>
                  <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                    {isArabic ? 'مقدم الطلب' : 'Requester'}
                  </th>
                  <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                    {t('requests.status')}
                  </th>
                  <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                    {t('requests.date')}
                  </th>
                  <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                    {t('requests.actions')}
                  </th>
                </tr>
              </thead>
              <tbody>
                {requests.map((request) => (
                  <tr
                    key={request.id}
                    className="border-b border-neutral-100 hover:bg-neutral-50 transition-colors"
                  >
                    <td className="px-4 py-3 text-neutral-800">
                      {isArabic
                        ? request.patientName
                        : request.patientNameEn || request.patientName}
                    </td>
                    <td className="px-4 py-3 text-neutral-600">
                      {isArabic
                        ? request.documentTypeNameAr
                        : request.documentTypeNameEn}
                    </td>
                    <td className="px-4 py-3 text-neutral-600">
                      {request.createdByName}
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={request.status} />
                    </td>
                    <td className="px-4 py-3 text-neutral-500">
                      {formatDate(request.createdAt, language)}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-1">
                        <button
                          onClick={() => acceptMutation.mutate(request.id)}
                          disabled={acceptMutation.isPending}
                          className="inline-flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors disabled:opacity-60"
                          title={t('requests.accept')}
                        >
                          <Check size={14} />
                          {t('requests.accept')}
                        </button>
                        <button
                          onClick={() => setRejectTarget(request.id)}
                          className="inline-flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
                          title={t('requests.reject')}
                        >
                          <X size={14} />
                          {t('requests.reject')}
                        </button>
                        <button
                          onClick={() => navigate(`/requests/${request.id}`)}
                          className="inline-flex items-center p-1.5 text-primary hover:bg-primary-50 rounded-lg transition-colors"
                          title={isArabic ? 'عرض' : 'View'}
                        >
                          <Eye size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Reject Dialog */}
      <ConfirmDialog
        open={!!rejectTarget}
        title={t('requests.reject')}
        message={t('requests.rejectionReason')}
        variant="danger"
        confirmLabel={t('requests.reject')}
        onConfirm={handleRejectConfirm}
        onCancel={() => {
          setRejectTarget(null);
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
