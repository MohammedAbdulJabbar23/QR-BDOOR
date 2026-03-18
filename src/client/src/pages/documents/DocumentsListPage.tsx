import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Search, Eye, Calendar, X } from 'lucide-react';
import { documentsApi } from '@/api/documents.api';
import { documentTypesApi } from '@/api/documentTypes.api';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { formatDate } from '@/utils/formatters';
import { DOCUMENT_STATUSES } from '@/utils/constants';
import { filterDocumentTypesForDepartment } from '@/utils/documentTypeFilters';
import PageHeader from '@/components/common/PageHeader';
import StatusBadge from '@/components/common/StatusBadge';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import EmptyState from '@/components/common/EmptyState';
import Pagination from '@/components/common/Pagination';
import type { IssuedDocument } from '@/types/document.types';

const STATUS_LABEL_KEYS: Record<string, string> = {
  '': 'common.all',
  Draft: 'status.draft',
  Archived: 'status.archived',
  Revoked: 'status.revoked',
  AwaitingAccountStatement: 'status.awaitingAccountStatement',
};

const STATUS_OPTIONS = ['', ...DOCUMENT_STATUSES] as const;
const PAGE_SIZE = 10;

export default function DocumentsListPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const language = useUiStore((s) => s.language);

  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [documentTypeFilter, setDocumentTypeFilter] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [page, setPage] = useState(1);

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
  }, [statusFilter, documentTypeFilter, fromDate, toDate]);

  const { data: documentTypes } = useQuery({
    queryKey: ['documentTypes', true],
    queryFn: () => documentTypesApi.getAll(true),
  });
  const visibleDocumentTypes = filterDocumentTypesForDepartment(documentTypes, user?.department);

  const { data, isLoading } = useQuery({
    queryKey: ['documents', { search: debouncedSearch, status: statusFilter, documentTypeId: documentTypeFilter, fromDate, toDate, page, pageSize: PAGE_SIZE }],
    queryFn: () =>
      documentsApi.getAll({
        search: debouncedSearch || undefined,
        status: statusFilter || undefined,
        documentTypeId: documentTypeFilter || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
  });

  const documents = data?.items ?? [];
  const totalPages = data?.totalPages ?? 0;

  const getPatientName = (doc: IssuedDocument) =>
    language === 'ar'
      ? (doc.patientName || '-')
      : (doc.patientNameEn || doc.patientName || '-');

  const getDocumentType = (doc: IssuedDocument) =>
    language === 'ar' ? doc.documentTypeNameAr : doc.documentTypeNameEn;

  return (
    <div>
      <PageHeader title={t('documents.title')} />

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

        <div className="flex flex-col sm:flex-row gap-3">
          <select
            value={documentTypeFilter}
            onChange={(e) => setDocumentTypeFilter(e.target.value)}
            className="w-full sm:w-72 px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary bg-white"
          >
            <option value="">{t('common.all')} {t('requests.documentType')}</option>
            {visibleDocumentTypes.map((documentType) => (
              <option key={documentType.id} value={documentType.id}>
                {language === 'ar' ? documentType.nameAr : documentType.nameEn}
              </option>
            ))}
          </select>
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
      ) : !data || documents.length === 0 ? (
        <EmptyState title={t('documents.noDocuments')} />
      ) : (
        <>
          <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-neutral-200 bg-neutral-50">
                    <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                      {t('documents.documentNumber')}
                    </th>
                    <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                      {t('requests.patientName')}
                    </th>
                    <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                      {t('requests.documentType')}
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
                  {documents.map((doc) => (
                    <tr
                      key={doc.id}
                      className="border-b border-neutral-100 hover:bg-neutral-50 transition-colors"
                    >
                      <td className="px-4 py-3 font-medium text-neutral-900">
                        {doc.documentNumber}
                      </td>
                      <td className="px-4 py-3 text-neutral-800">
                        {getPatientName(doc)}
                      </td>
                      <td className="px-4 py-3 text-neutral-600">
                        {getDocumentType(doc)}
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={doc.status} />
                      </td>
                      <td className="px-4 py-3 text-neutral-500">
                        {formatDate(doc.issuedAt, language)}
                      </td>
                      <td className="px-4 py-3">
                        <button
                          onClick={() => navigate(`/documents/${doc.id}`)}
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
