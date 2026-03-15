import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { requestsApi } from '@/api/requests.api';
import { documentsApi } from '@/api/documents.api';
import { canCreateRequest, canAcceptRejectRequest } from '@/utils/permissions';
import { formatDate } from '@/utils/formatters';
import PageHeader from '@/components/common/PageHeader';
import StatusBadge from '@/components/common/StatusBadge';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import EmptyState from '@/components/common/EmptyState';
import {
  FileText, FolderOpen, Clock, Archive,
  Plus, ArrowRight, ArrowLeft,
} from 'lucide-react';

export default function DashboardPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const language = useUiStore((s) => s.language);
  const isRtl = language === 'ar';
  const ArrowIcon = isRtl ? ArrowLeft : ArrowRight;

  const { data: requestsData, isLoading: requestsLoading } = useQuery({
    queryKey: ['requests', 'dashboard'],
    queryFn: () => requestsApi.getAll({ page: 1, pageSize: 10 }),
    refetchInterval: 180000,
  });

  const { data: pendingRequests, isLoading: pendingLoading } = useQuery({
    queryKey: ['requests', 'pending'],
    queryFn: () => requestsApi.getPending(),
    enabled: canAcceptRejectRequest(user?.department || ''),
    refetchInterval: 180000,
  });

  const { data: documentsData, isLoading: documentsLoading } = useQuery({
    queryKey: ['documents', 'dashboard'],
    queryFn: () => documentsApi.getAll({ page: 1, pageSize: 10 }),
    refetchInterval: 180000,
  });

  const { data: archivedDocs } = useQuery({
    queryKey: ['documents', 'archived'],
    queryFn: () => documentsApi.getAll({ status: 'Archived', page: 1, pageSize: 1 }),
    refetchInterval: 180000,
  });

  const isLoading = requestsLoading || documentsLoading;

  // Calculate today's counts
  const today = new Date().toDateString();
  const todayRequests = requestsData?.items.filter(
    (r) => new Date(r.createdAt).toDateString() === today,
  ).length ?? 0;
  const pendingCount = requestsData?.items.filter(
    (r) => r.status === 'Pending',
  ).length ?? 0;
  const todayDocuments = documentsData?.items.filter(
    (d) => new Date(d.issuedAt).toDateString() === today,
  ).length ?? 0;
  const archivedCount = archivedDocs?.totalCount ?? 0;

  const statCards = [
    {
      label: t('dashboard.totalRequests'),
      value: todayRequests,
      icon: FileText,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
    },
    {
      label: t('dashboard.pending'),
      value: pendingCount,
      icon: Clock,
      color: 'text-amber-600',
      bgColor: 'bg-amber-50',
    },
    {
      label: t('dashboard.documentsIssued'),
      value: todayDocuments,
      icon: FolderOpen,
      color: 'text-green-600',
      bgColor: 'bg-green-50',
    },
    {
      label: t('dashboard.archived'),
      value: archivedCount,
      icon: Archive,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
    },
  ];

  if (isLoading) {
    return <LoadingSpinner className="min-h-[60vh]" />;
  }

  return (
    <div>
      <PageHeader title={t('nav.dashboard')} />

      {/* Stat cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        {statCards.map((card) => {
          const Icon = card.icon;
          return (
            <div
              key={card.label}
              className="bg-white rounded-xl border border-neutral-200 p-5 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between mb-3">
                <div className={`p-2.5 rounded-lg ${card.bgColor}`}>
                  <Icon size={20} className={card.color} />
                </div>
              </div>
              <p className="text-2xl font-bold text-neutral-900">{card.value}</p>
              <p className="text-sm text-neutral-500 mt-1">{card.label}</p>
            </div>
          );
        })}
      </div>

      {/* Department-specific content */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Inquiry department: Create request + Recent requests */}
        {canCreateRequest(user?.department || '') && (
          <>
            {/* Quick action */}
            <div className="bg-white rounded-xl border border-neutral-200 p-6">
              <h3 className="text-lg font-semibold text-neutral-800 mb-4">
                {t('dashboard.quickActions')}
              </h3>
              <Link
                to="/requests/create"
                className="inline-flex items-center gap-2 px-4 py-2.5 bg-primary hover:bg-primary-700
                  text-white text-sm font-medium rounded-lg transition-colors"
              >
                <Plus size={18} />
                {t('dashboard.createNew')}
              </Link>
            </div>

            {/* Recent requests table */}
            <div className="bg-white rounded-xl border border-neutral-200 p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-neutral-800">
                  {t('dashboard.myRequests')}
                </h3>
                <Link
                  to="/requests"
                  className="text-sm text-primary hover:text-primary-700 font-medium inline-flex items-center gap-1"
                >
                  {t('dashboard.viewAll')}
                  <ArrowIcon size={14} />
                </Link>
              </div>
              {requestsData && requestsData.items.length > 0 ? (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-neutral-100">
                        <th className="text-start py-2 px-3 text-neutral-500 font-medium">
                          {t('requests.patientName')}
                        </th>
                        <th className="text-start py-2 px-3 text-neutral-500 font-medium">
                          {t('requests.documentType')}
                        </th>
                        <th className="text-start py-2 px-3 text-neutral-500 font-medium">
                          {t('requests.status')}
                        </th>
                        <th className="text-start py-2 px-3 text-neutral-500 font-medium">
                          {t('requests.date')}
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {requestsData.items.slice(0, 5).map((req) => (
                        <tr
                          key={req.id}
                          className="border-b border-neutral-50 hover:bg-neutral-50"
                        >
                          <td className="py-2.5 px-3">
                            <Link
                              to={`/requests/${req.id}`}
                              className="text-neutral-800 hover:text-primary font-medium"
                            >
                              {req.patientName}
                            </Link>
                          </td>
                          <td className="py-2.5 px-3 text-neutral-600">
                            {language === 'ar' ? req.documentTypeNameAr : req.documentTypeNameEn}
                          </td>
                          <td className="py-2.5 px-3">
                            <StatusBadge status={req.status} />
                          </td>
                          <td className="py-2.5 px-3 text-neutral-500">
                            {formatDate(req.createdAt, language)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <EmptyState
                  title={t('requests.noRequests')}
                />
              )}
            </div>
          </>
        )}

        {/* Statistics department: Pending requests */}
        {canAcceptRejectRequest(user?.department || '') && (
          <div className="bg-white rounded-xl border border-neutral-200 p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-neutral-800">
                {t('dashboard.pendingRequests')}
              </h3>
              <Link
                to="/requests/pending"
                className="text-sm text-primary hover:text-primary-700 font-medium inline-flex items-center gap-1"
              >
                {t('dashboard.viewPending')}
                <ArrowIcon size={14} />
              </Link>
            </div>
            {pendingLoading ? (
              <LoadingSpinner />
            ) : pendingRequests && pendingRequests.length > 0 ? (
              <div className="space-y-3">
                <div className="flex items-center gap-3 p-4 bg-warning-light rounded-lg border border-amber-200">
                  <Clock size={24} className="text-amber-600 shrink-0" />
                  <div>
                    <p className="text-2xl font-bold text-amber-800">
                      {pendingRequests.length}
                    </p>
                    <p className="text-sm text-amber-700">
                      {t('dashboard.pendingRequests')}
                    </p>
                  </div>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-neutral-100">
                        <th className="text-start py-2 px-3 text-neutral-500 font-medium">
                          {t('requests.patientName')}
                        </th>
                        <th className="text-start py-2 px-3 text-neutral-500 font-medium">
                          {t('requests.documentType')}
                        </th>
                        <th className="text-start py-2 px-3 text-neutral-500 font-medium">
                          {t('requests.date')}
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {pendingRequests.slice(0, 5).map((req) => (
                        <tr
                          key={req.id}
                          className="border-b border-neutral-50 hover:bg-neutral-50"
                        >
                          <td className="py-2.5 px-3">
                            <Link
                              to={`/requests/${req.id}`}
                              className="text-neutral-800 hover:text-primary font-medium"
                            >
                              {req.patientName}
                            </Link>
                          </td>
                          <td className="py-2.5 px-3 text-neutral-600">
                            {language === 'ar' ? req.documentTypeNameAr : req.documentTypeNameEn}
                          </td>
                          <td className="py-2.5 px-3 text-neutral-500">
                            {formatDate(req.createdAt, language)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            ) : (
              <EmptyState
                title={t('requests.noRequests')}
              />
            )}
          </div>
        )}
      </div>
    </div>
  );
}
