import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Home, FileQuestion } from 'lucide-react';

export default function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-50 px-4">
      <div className="text-center max-w-md">
        <div className="inline-flex items-center justify-center w-20 h-20 bg-primary-50 rounded-full mb-6">
          <FileQuestion size={40} className="text-primary" />
        </div>

        <h1 className="text-6xl font-bold text-neutral-900 mb-2">404</h1>

        <h2 className="text-xl font-semibold text-neutral-700 mb-3">
          {t('notFound.title')}
        </h2>

        <p className="text-neutral-500 mb-8">
          {t('notFound.message')}
        </p>

        <Link
          to="/"
          className="inline-flex items-center gap-2 px-6 py-3 bg-primary hover:bg-primary-700
            text-white font-medium text-sm rounded-lg transition-colors
            focus:outline-none focus:ring-2 focus:ring-primary/20 focus:ring-offset-2"
        >
          <Home size={18} />
          {t('notFound.goHome')}
        </Link>
      </div>
    </div>
  );
}
