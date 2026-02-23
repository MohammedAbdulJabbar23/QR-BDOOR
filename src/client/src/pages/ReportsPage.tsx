import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { BarChart3, Calendar, Download, FileText, Loader2 } from 'lucide-react';
import { reportsApi } from '@/api/reports.api';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { canViewReports } from '@/utils/permissions';
import { formatDate } from '@/utils/formatters';
import PageHeader from '@/components/common/PageHeader';
import EmptyState from '@/components/common/EmptyState';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import { cn } from '@/utils/cn';

type ReportTab = 'daily' | 'status' | 'cancelled';

export default function ReportsPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const language = useUiStore((s) => s.language);

  const today = new Date().toISOString().split('T')[0];
  const thirtyDaysAgo = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

  const [activeTab, setActiveTab] = useState<ReportTab>('daily');
  const [dailyDate, setDailyDate] = useState(today);
  const [fromDate, setFromDate] = useState(thirtyDaysAgo);
  const [toDate, setToDate] = useState(today);
  const [exporting, setExporting] = useState<string | null>(null);

  // Daily report query
  const {
    data: dailyData,
    isLoading: dailyLoading,
  } = useQuery({
    queryKey: ['reports', 'daily', dailyDate],
    queryFn: () => reportsApi.getDaily(dailyDate),
    enabled: activeTab === 'daily',
  });

  // Status breakdown query
  const {
    data: statusData,
    isLoading: statusLoading,
  } = useQuery({
    queryKey: ['reports', 'status', fromDate, toDate],
    queryFn: () => reportsApi.getStatusBreakdown(fromDate, toDate),
    enabled: activeTab === 'status',
  });

  // Cancelled/Rejected requests query
  const {
    data: cancelledData,
    isLoading: cancelledLoading,
  } = useQuery({
    queryKey: ['reports', 'cancelled', fromDate, toDate],
    queryFn: () => reportsApi.getCancelled(fromDate, toDate),
    enabled: activeTab === 'cancelled',
  });

  const handleExport = async (reportType: string) => {
    setExporting(reportType);
    try {
      const from = activeTab === 'daily' ? dailyDate : fromDate;
      const to = activeTab === 'daily' ? dailyDate : toDate;
      await reportsApi.exportReport(reportType, from, to);
    } catch {
      // Export error - silently fail
    } finally {
      setExporting(null);
    }
  };

  if (!user || !canViewReports(user.role, user.department)) {
    return (
      <EmptyState
        title={t('common.error')}
        message={t('common.noData')}
      />
    );
  }

  const tabs: { key: ReportTab; label: string }[] = [
    { key: 'daily', label: t('reports.dailyReport') },
    { key: 'status', label: t('reports.statusBreakdown') },
    { key: 'cancelled', label: t('reports.cancelledRejected') },
  ];

  return (
    <div>
      <PageHeader title={t('reports.title')} />

      {/* Tabs */}
      <div className="flex gap-1 mb-6 border-b border-neutral-200">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={cn(
              'px-4 py-2.5 text-sm font-medium border-b-2 transition-colors -mb-px',
              activeTab === tab.key
                ? 'border-primary text-primary'
                : 'border-transparent text-neutral-500 hover:text-neutral-700 hover:border-neutral-300',
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Daily Report */}
      {activeTab === 'daily' && (
        <div>
          {/* Date picker */}
          <div className="flex items-center gap-3 mb-6">
            <div className="flex items-center gap-2">
              <Calendar size={18} className="text-neutral-400" />
              <label className="text-sm font-medium text-neutral-600">
                {t('reports.date')}:
              </label>
            </div>
            <input
              type="date"
              value={dailyDate}
              onChange={(e) => setDailyDate(e.target.value)}
              className="px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
            />
            <button
              onClick={() => handleExport('daily')}
              disabled={exporting === 'daily'}
              className="flex items-center gap-2 px-4 py-2 text-sm font-medium border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors text-neutral-700 disabled:opacity-60"
            >
              {exporting === 'daily' ? (
                <Loader2 size={16} className="animate-spin" />
              ) : (
                <Download size={16} />
              )}
              {t('reports.exportDaily')}
            </button>
          </div>

          {dailyLoading ? (
            <LoadingSpinner />
          ) : !dailyData ? (
            <EmptyState title={t('reports.noData')} />
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <StatCard
                label={t('reports.totalRequests')}
                value={dailyData.totalRequests ?? 0}
                icon={FileText}
                color="text-blue-600"
                bg="bg-blue-50"
              />
              <StatCard
                label={t('reports.completed')}
                value={dailyData.completed ?? 0}
                icon={BarChart3}
                color="text-green-600"
                bg="bg-green-50"
              />
              <StatCard
                label={t('reports.pending')}
                value={dailyData.pending ?? 0}
                icon={BarChart3}
                color="text-amber-600"
                bg="bg-amber-50"
              />
              <StatCard
                label={t('reports.documentsIssued')}
                value={dailyData.documentsIssued ?? 0}
                icon={BarChart3}
                color="text-purple-600"
                bg="bg-purple-50"
              />
            </div>
          )}
        </div>
      )}

      {/* Status Breakdown */}
      {activeTab === 'status' && (
        <div>
          <DateRangeFilter
            from={fromDate}
            to={toDate}
            onFromChange={setFromDate}
            onToChange={setToDate}
            exportLabel={t('reports.exportStatus')}
            onExport={() => handleExport('status')}
            exporting={exporting === 'status'}
            t={t}
          />

          {statusLoading ? (
            <LoadingSpinner />
          ) : !statusData || (Array.isArray(statusData) && statusData.length === 0) ? (
            <EmptyState title={t('reports.noData')} />
          ) : (
            <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-neutral-200 bg-neutral-50">
                      <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                        {t('reports.status')}
                      </th>
                      <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                        {t('reports.count')}
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {(Array.isArray(statusData) ? statusData : []).map(
                      (item: { status: string; count: number }, idx: number) => (
                        <tr
                          key={idx}
                          className="border-b border-neutral-100 hover:bg-neutral-50"
                        >
                          <td className="px-4 py-3 text-neutral-800">
                            {t(`status.${item.status?.charAt(0).toLowerCase()}${item.status?.slice(1)}`, item.status)}
                          </td>
                          <td className="px-4 py-3 text-neutral-600 font-medium">
                            {item.count}
                          </td>
                        </tr>
                      ),
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Cancelled/Rejected Requests */}
      {activeTab === 'cancelled' && (
        <div>
          <DateRangeFilter
            from={fromDate}
            to={toDate}
            onFromChange={setFromDate}
            onToChange={setToDate}
            exportLabel={t('reports.exportCancelled')}
            onExport={() => handleExport('cancelled')}
            exporting={exporting === 'cancelled'}
            t={t}
          />

          {cancelledLoading ? (
            <LoadingSpinner />
          ) : !cancelledData || (Array.isArray(cancelledData) && cancelledData.length === 0) ? (
            <EmptyState title={t('reports.noData')} />
          ) : (
            <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-neutral-200 bg-neutral-50">
                      <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                        {t('reports.requestId')}
                      </th>
                      <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                        {t('reports.patientName')}
                      </th>
                      <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                        {t('reports.reason')}
                      </th>
                      <th className="text-start px-4 py-3 font-semibold text-neutral-700">
                        {t('reports.rejectedAt')}
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {(Array.isArray(cancelledData) ? cancelledData : []).map(
                      (item: { id: string; patientName: string; rejectionReason: string; updatedAt: string }, idx: number) => (
                        <tr
                          key={idx}
                          className="border-b border-neutral-100 hover:bg-neutral-50"
                        >
                          <td className="px-4 py-3 text-neutral-800 font-mono text-xs">
                            {item.id?.substring(0, 8)}...
                          </td>
                          <td className="px-4 py-3 text-neutral-800">
                            {item.patientName}
                          </td>
                          <td className="px-4 py-3 text-neutral-600">
                            {item.rejectionReason || '-'}
                          </td>
                          <td className="px-4 py-3 text-neutral-500">
                            {item.updatedAt ? formatDate(item.updatedAt, language) : '-'}
                          </td>
                        </tr>
                      ),
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

// --- Sub-components ---

function StatCard({
  label,
  value,
  icon: Icon,
  color,
  bg,
}: {
  label: string;
  value: number;
  icon: typeof BarChart3;
  color: string;
  bg: string;
}) {
  return (
    <div className="bg-white rounded-xl border border-neutral-200 p-5">
      <div className="flex items-center gap-3 mb-3">
        <div className={`p-2.5 rounded-lg ${bg}`}>
          <Icon size={20} className={color} />
        </div>
      </div>
      <p className="text-2xl font-bold text-neutral-900">{value}</p>
      <p className="text-sm text-neutral-500 mt-1">{label}</p>
    </div>
  );
}

function DateRangeFilter({
  from,
  to,
  onFromChange,
  onToChange,
  exportLabel,
  onExport,
  exporting,
  t,
}: {
  from: string;
  to: string;
  onFromChange: (v: string) => void;
  onToChange: (v: string) => void;
  exportLabel: string;
  onExport: () => void;
  exporting: boolean;
  t: (key: string) => string;
}) {
  return (
    <div className="flex flex-wrap items-center gap-3 mb-6">
      <div className="flex items-center gap-2">
        <Calendar size={18} className="text-neutral-400" />
        <label className="text-sm font-medium text-neutral-600">
          {t('reports.from')}:
        </label>
        <input
          type="date"
          value={from}
          onChange={(e) => onFromChange(e.target.value)}
          className="px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
        />
      </div>
      <div className="flex items-center gap-2">
        <label className="text-sm font-medium text-neutral-600">
          {t('reports.to')}:
        </label>
        <input
          type="date"
          value={to}
          onChange={(e) => onToChange(e.target.value)}
          className="px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
        />
      </div>
      <button
        onClick={onExport}
        disabled={exporting}
        className="flex items-center gap-2 px-4 py-2 text-sm font-medium border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors text-neutral-700 disabled:opacity-60"
      >
        {exporting ? (
          <Loader2 size={16} className="animate-spin" />
        ) : (
          <Download size={16} />
        )}
        {exportLabel}
      </button>
    </div>
  );
}
