import { Outlet, Navigate } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import Sidebar from './Sidebar';
import Header from './Header';
import { cn } from '@/utils/cn';

export default function MainLayout() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const { sidebarOpen, language } = useUiStore();
  const isRtl = language === 'ar';

  if (!isAuthenticated) return <Navigate to="/login" replace />;

  return (
    <div className="min-h-screen bg-neutral-50">
      <Sidebar />
      <Header />
      <main
        className={cn(
          'pt-16 transition-all duration-300 min-h-screen',
          isRtl
            ? (sidebarOpen ? 'mr-64' : 'mr-16')
            : (sidebarOpen ? 'ml-64' : 'ml-16')
        )}
      >
        <div className="p-6">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
