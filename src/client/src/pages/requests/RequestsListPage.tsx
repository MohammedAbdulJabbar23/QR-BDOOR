import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Search, Plus, Eye, Calendar, X } from 'lucide-react';
import { requestsApi } from '@/api/requests.api';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { canCreateRequest } from '@/utils/permissions';
import { formatDate } from '@/utils/formatters';
import PageHeader from '@/components/common/PageHeader';
import StatusBadge from '@/components/common/StatusBadge';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import EmptyState from '@/components/common/EmptyState';
import Pagination from '@/components/common/Pagination';

const STATUS_OPTIONS = ['', 'Pending', 'InProgress', 'Rejected', 'Completed'] as const;

const STATUS_LABEL_KEYS: Record<string, string> = {
  '': 'common.all',
  Pending: 'status.pending',
  InProgress: 'status.inProgress',
  Rejected: 'status.rejected',
  Completed: 'status.completed',
};

export default function RequestsListPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const language = useUiStore((s) => s.language);
  const isArabic = language === 'ar';

  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 10;

  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [search]);

  // Reset page when filters change
  useEffect(() => {
    setPage(1);
  }, [statusFilter, fromDate, toDate]);

  const { data, isLoading } = useQuery({
    queryKey: ['requests', { status: statusFilter, search: debouncedSearch, fromDate, toDate, page, pageSize }],
    queryFn: () =>
      requestsApi.getAll({
        status: statusFilter || undefined,
        search: debouncedSearch || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        page,
        pageSize,
      }),
  });

  const showCreateButton = canCreateRequest(user?.department || '');

  return (
    <div>
      <PageHeader
        title={t('requests.title')}
        actions={
          showCreateButton ? (
            <button
              onClick={() => navigate('/requests/create')}
              className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-700 transition-colors text-sm font-medium"
            >
              <Plus size={18} />
              {t('requests.create')}
            </button>
          ) : undefined
        }
      />

      {/* Filters */}
      <div className="space-y-3 mb-6">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search
              size={18}
              className="absolute top-1/2 -translate-y-1/2 start-3 text-neutral-400 pointer-events-none"
            />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={t('common.search')}
              className="w-full ps-10 pe-4 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
            />
          </div>

          <div className="flex gap-2 flex-wrap">
            {STATUS_OPTIONS.map((status) => (
              <button
                key={status}
                onClick={() => setStatusFilter(status)}
                className={
                  statusFilter === status
                    ? 'px-3 py-2 text-sm rounded-lg font-medium bg-primary text-white'
                    : 'px-3 py-2 text-sm rounded-lg font-medium border border-neutral-300 text-neutral-600 hover:bg-neutral-50'
                }
              >
                {t(STATUS_LABEL_KEYS[status])}
              </button>
            ))}
          </div>
        </div>

        <div className="flex flex-col sm:flex-row items-start sm:items-center gap-2">
          <div className="flex items-center gap-1.5 text-sm text-neutral-500">
            <Calendar size={16} />
            <span>{t('reports.dateRange')}:</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <input
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                placeholder={t('reports.from')}
                className="ps-3 pe-3 py-1.5 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
              />
            </div>
            <span className="text-neutral-400 text-sm">&ndash;</span>
            <div className="relative">
              <input
                type="date"
                value={toDate}
                min={fromDate || undefined}
                onChange={(e) => setToDate(e.target.value)}
                placeholder={t('reports.to')}
                className="ps-3 pe-3 py-1.5 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
              />
            </div>
            {(fromDate || toDate) && (
              <button
                onClick={() => { setFromDate(''); setToDate(''); }}
                className="p-1.5 text-neutral-400 hover:text-neutral-600 hover:bg-neutral-100 rounded-md transition-colors"
                title={t('common.reset')}
              >
                <X size={16} />
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Content */}
      {isLoading ? (
        <LoadingSpinner />
      ) : !data || data.items.length === 0 ? (
        <EmptyState
          title={t('requests.noRequests')}
          action={
            showCreateButton ? (
              <button
                onClick={() => navigate('/requests/create')}
                className="px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-700 transition-colors text-sm font-medium"
              >
                {t('requests.create')}
              </button>
            ) : undefined
          }
        />
      ) : (
        <>
          {/* Table */}
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
                      {t('requests.recipientEntity')}
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
                  {data.items.map((request) => (
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
                        {request.recipientEntity}
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={request.status} />
                      </td>
                      <td className="px-4 py-3 text-neutral-500">
                        {formatDate(request.createdAt, language)}
                      </td>
                      <td className="px-4 py-3">
                        <button
                          onClick={() => navigate(`/requests/${request.id}`)}
                          className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm text-primary hover:bg-primary-50 rounded-lg transition-colors"
                        >
                          <Eye size={16} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination */}
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            onPageChange={setPage}
          />
        </>
      )}
    </div>
  );
}
