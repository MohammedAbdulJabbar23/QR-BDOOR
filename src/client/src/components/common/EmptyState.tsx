import { FileX } from 'lucide-react';

interface Props {
  title: string;
  message?: string;
  action?: React.ReactNode;
}

export default function EmptyState({ title, message, action }: Props) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <FileX size={48} className="text-neutral-300 mb-4" />
      <h3 className="text-lg font-medium text-neutral-700">{title}</h3>
      {message && <p className="mt-1 text-sm text-neutral-500 max-w-sm">{message}</p>}
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
}
