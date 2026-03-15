import { useTranslation } from 'react-i18next';
import { cn } from '@/utils/cn';

const statusStyles: Record<string, string> = {
  Pending: 'bg-warning-light text-amber-800 border-amber-200',
  InProgress: 'bg-blue-50 text-blue-800 border-blue-200',
  Rejected: 'bg-danger-light text-red-800 border-red-200',
  Completed: 'bg-success-light text-green-800 border-green-200',
  Draft: 'bg-neutral-100 text-neutral-700 border-neutral-200',
  Archived: 'bg-success-light text-green-800 border-green-200',
  Revoked: 'bg-danger-light text-red-800 border-red-200',
  AwaitingAccountStatement: 'bg-purple-50 text-purple-800 border-purple-200',
};

const statusKeys: Record<string, string> = {
  Pending: 'status.pending',
  InProgress: 'status.inProgress',
  Rejected: 'status.rejected',
  Completed: 'status.completed',
  Draft: 'status.draft',
  Archived: 'status.archived',
  Revoked: 'status.revoked',
  AwaitingAccountStatement: 'status.awaitingAccountStatement',
};

export default function StatusBadge({ status }: { status: string }) {
  const { t } = useTranslation();
  return (
    <span className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border', statusStyles[status] || 'bg-neutral-100 text-neutral-600')}>
      {t(statusKeys[status] || status)}
    </span>
  );
}
