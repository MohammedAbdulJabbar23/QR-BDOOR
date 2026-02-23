import { cn } from '@/utils/cn';

export default function LoadingSpinner({ className }: { className?: string }) {
  return (
    <div className={cn('flex items-center justify-center p-8', className)}>
      <div className="w-8 h-8 border-4 border-primary-200 border-t-primary rounded-full animate-spin" />
    </div>
  );
}
