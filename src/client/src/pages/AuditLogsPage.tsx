import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { ChevronDown, ChevronUp, Filter, X } from 'lucide-react';

import PageHeader from '@/components/common/PageHeader';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import EmptyState from '@/components/common/EmptyState';
import Pagination from '@/components/common/Pagination';
import { auditLogsApi } from '@/api/auditLogs.api';
import { useUiStore } from '@/stores/uiStore';
import { useAuthStore } from '@/stores/authStore';
import { canViewAuditLogs } from '@/utils/permissions';
import { formatDateTime } from '@/utils/formatters';
import { cn } from '@/utils/cn';
import type { AuditLogDto } from '@/types/common.types';

interface Filters {
  userId: string;
  action: string;
  entityType: string;
  from: string;
  to: string;
}

const initialFilters: Filters = {
  userId: '',
  action: '',
  entityType: '',
  from: '',
  to: '',
};

export default function AuditLogsPage() {
  const { t } = useTranslation();
  const language = useUiStore((s) => s.language);
  const user = useAuthStore((s) => s.user);
  const isRtl = language === 'ar';

  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<Filters>(initialFilters);
  const [appliedFilters, setAppliedFilters] = useState<Filters>(initialFilters);
  const [expandedRow, setExpandedRow] = useState<number | null>(null);
  const [showFilters, setShowFilters] = useState(true);

  const pageSize = 15;

  const queryParams = {
    page,
    pageSize,
    ...(appliedFilters.userId && { userId: appliedFilters.userId }),
    ...(appliedFilters.action && { action: appliedFilters.action }),
    ...(appliedFilters.entityType && { entityType: appliedFilters.entityType }),
    ...(appliedFilters.from && { from: appliedFilters.from }),
    ...(appliedFilters.to && { to: appliedFilters.to }),
  };

  const { data, isLoading, isError } = useQuery({
    queryKey: ['auditLogs', queryParams],
    queryFn: () => auditLogsApi.getAll(queryParams),
  });

  const handleApplyFilters = useCallback(() => {
    setAppliedFilters({ ...filters });
    setPage(1);
  }, [filters]);

  const handleClearFilters = useCallback(() => {
    setFilters(initialFilters);
    setAppliedFilters(initialFilters);
    setPage(1);
  }, []);

  const handleFilterChange = useCallback(
    (field: keyof Filters, value: string) => {
      setFilters((prev) => ({ ...prev, [field]: value }));
    },
    [],
  );

  const toggleExpandRow = useCallback((id: number) => {
    setExpandedRow((prev) => (prev === id ? null : id));
  }, []);

  const formatDetails = (details: string | null): string => {
    if (!details) return '--';
    try {
      const parsed = JSON.parse(details);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return details;
    }
  };

  const hasActiveFilters =
    appliedFilters.userId !== '' ||
    appliedFilters.action !== '' ||
    appliedFilters.entityType !== '' ||
    appliedFilters.from !== '' ||
    appliedFilters.to !== '';

  if (!user || !canViewAuditLogs(user.role)) {
    return (
      <EmptyState
        title={t('common.error')}
        message={t('common.noData')}
      />
    );
  }

  return (
    <div>
      <PageHeader title={t('audit.title')} />

      {/* Filters */}
      <div className="bg-white rounded-xl border border-neutral-200 mb-4">
        <button
          onClick={() => setShowFilters((prev) => !prev)}
          className="w-full flex items-center justify-between px-4 py-3 text-sm font-medium text-neutral-700 hover:bg-neutral-50 transition-colors rounded-xl"
        >
          <span className="flex items-center gap-2">
            <Filter size={16} />
            {t('audit.filters')}
            {hasActiveFilters && (
              <span className="inline-flex items-center justify-center w-5 h-5 text-xs font-bold text-white bg-red-600 rounded-full">
                !
              </span>
            )}
          </span>
          {showFilters ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
        </button>

        {showFilters && (
          <div className="px-4 pb-4 border-t border-neutral-100">
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 mt-3">
              {/* Date From */}
              <div>
                <label className="block text-xs font-medium text-neutral-600 mb-1">
                  {t('audit.from')}
                </label>
                <input
                  type="date"
                  value={filters.from}
                  onChange={(e) => handleFilterChange('from', e.target.value)}
                  className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
              </div>

              {/* Date To */}
              <div>
                <label className="block text-xs font-medium text-neutral-600 mb-1">
                  {t('audit.to')}
                </label>
                <input
                  type="date"
                  value={filters.to}
                  onChange={(e) => handleFilterChange('to', e.target.value)}
                  className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                />
              </div>

              {/* User ID */}
              <div>
                <label className="block text-xs font-medium text-neutral-600 mb-1">
                  {t('audit.userId')}
                </label>
                <input
                  type="text"
                  value={filters.userId}
                  onChange={(e) => handleFilterChange('userId', e.target.value)}
                  className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                  placeholder={t('audit.userId')}
                />
              </div>

              {/* Action */}
              <div>
                <label className="block text-xs font-medium text-neutral-600 mb-1">
                  {t('audit.action')}
                </label>
                <input
                  type="text"
                  value={filters.action}
                  onChange={(e) => handleFilterChange('action', e.target.value)}
                  className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                  placeholder={t('audit.action')}
                />
              </div>

              {/* Entity Type */}
              <div>
                <label className="block text-xs font-medium text-neutral-600 mb-1">
                  {t('audit.entityType')}
                </label>
                <input
                  type="text"
                  value={filters.entityType}
                  onChange={(e) => handleFilterChange('entityType', e.target.value)}
                  className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
                  placeholder={t('audit.entityType')}
                />
              </div>
            </div>

            <div className="flex items-center gap-2 mt-3">
              <button
                onClick={handleApplyFilters}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
              >
                {t('audit.applyFilters')}
              </button>
              {hasActiveFilters && (
                <button
                  onClick={handleClearFilters}
                  className="flex items-center gap-1 px-4 py-2 text-sm font-medium text-neutral-600 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
                >
                  <X size={14} />
                  {t('audit.clearFilters')}
                </button>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Table */}
      {isLoading && <LoadingSpinner />}

      {isError && (
        <div className="text-center py-8 text-red-600">
          {t('common.error')}
        </div>
      )}

      {!isLoading && !isError && data && data.items.length === 0 && (
        <EmptyState title={t('audit.noLogs')} />
      )}

      {!isLoading && !isError && data && data.items.length > 0 && (
        <>
          <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-neutral-50 border-b border-neutral-200">
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('audit.timestamp')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('audit.user')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('audit.action')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('audit.entityType')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('audit.entityId')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('audit.ipAddress')}
                    </th>
                    <th className="px-4 py-3 w-10" />
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((log) => (
                    <AuditLogRow
                      key={log.id}
                      log={log}
                      language={language}
                      isRtl={isRtl}
                      isExpanded={expandedRow === log.id}
                      onToggleExpand={() => toggleExpandRow(log.id)}
                      formatDetails={formatDetails}
                      t={t}
                    />
                  ))}
                </tbody>
              </table>
            </div>
          </div>

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

interface AuditLogRowProps {
  log: AuditLogDto;
  language: string;
  isRtl: boolean;
  isExpanded: boolean;
  onToggleExpand: () => void;
  formatDetails: (details: string | null) => string;
  t: (key: string) => string;
}

function AuditLogRow({ log, language, isRtl, isExpanded, onToggleExpand, formatDetails, t }: AuditLogRowProps) {
  const hasDetails = log.details !== null && log.details !== '';

  return (
    <>
      <tr
        className={cn(
          'border-b border-neutral-100 transition-colors',
          hasDetails ? 'cursor-pointer hover:bg-neutral-50' : '',
          isExpanded ? 'bg-neutral-50' : '',
        )}
        onClick={hasDetails ? onToggleExpand : undefined}
      >
        <td className="px-4 py-3 text-neutral-600 whitespace-nowrap">
          {formatDateTime(log.createdAt, language)}
        </td>
        <td className="px-4 py-3 text-neutral-900 font-medium">
          {log.userName}
        </td>
        <td className="px-4 py-3">
          <span className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-blue-50 text-blue-700 border border-blue-200">
            {log.action}
          </span>
        </td>
        <td className="px-4 py-3 text-neutral-600">
          {log.entityType}
        </td>
        <td className="px-4 py-3 text-neutral-500 font-mono text-xs">
          {log.entityId}
        </td>
        <td className="px-4 py-3 text-neutral-500 font-mono text-xs">
          {log.ipAddress || '--'}
        </td>
        <td className="px-4 py-3">
          {hasDetails && (
            <span className="text-neutral-400">
              {isExpanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
            </span>
          )}
        </td>
      </tr>

      {isExpanded && hasDetails && (
        <tr className="border-b border-neutral-100">
          <td colSpan={7} className="px-4 py-3 bg-neutral-50">
            <div className={cn('text-xs', isRtl ? 'text-right' : 'text-left')}>
              <span className="font-medium text-neutral-700 block mb-1">
                {t('audit.details')}:
              </span>
              <pre className="bg-white border border-neutral-200 rounded-lg p-3 overflow-x-auto text-neutral-600 font-mono whitespace-pre-wrap max-h-64">
                {formatDetails(log.details)}
              </pre>
            </div>
          </td>
        </tr>
      )}
    </>
  );
}
