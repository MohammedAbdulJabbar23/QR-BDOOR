import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { canManageUsers, canViewAuditLogs, canViewReports } from '@/utils/permissions';
import {
  LayoutDashboard, FileText, FolderOpen, BarChart3,
  Users, Shield, Settings, ChevronLeft, ChevronRight
} from 'lucide-react';
import { cn } from '@/utils/cn';

export default function Sidebar() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const { sidebarOpen, toggleSidebar, language } = useUiStore();
  const isRtl = language === 'ar';

  const links = [
    { to: '/', icon: LayoutDashboard, label: t('nav.dashboard'), show: true },
    { to: '/requests', icon: FileText, label: t('nav.requests'), show: true },
    { to: '/documents', icon: FolderOpen, label: t('nav.documents'), show: true },
    { to: '/reports', icon: BarChart3, label: t('nav.reports'), show: canViewReports(user?.role || '') },
    { to: '/users', icon: Users, label: t('nav.users'), show: canManageUsers(user?.role || '') },
    { to: '/audit-logs', icon: Shield, label: t('nav.audit'), show: canViewAuditLogs(user?.role || '') },
    { to: '/settings/document-types', icon: Settings, label: t('nav.documentTypes'), show: canManageUsers(user?.role || '') },
  ].filter((l) => l.show);

  const CollapseIcon = isRtl
    ? (sidebarOpen ? ChevronRight : ChevronLeft)
    : (sidebarOpen ? ChevronLeft : ChevronRight);

  return (
    <aside
      className={cn(
        'fixed top-0 h-screen bg-primary-dark text-white transition-all duration-300 z-40 flex flex-col',
        sidebarOpen ? 'w-64' : 'w-16',
        isRtl ? 'right-0' : 'left-0'
      )}
    >
      <div className="flex items-center justify-between p-4 border-b border-white/10">
        {sidebarOpen && (
          <span className="text-lg font-bold truncate">{t('app.title')}</span>
        )}
        <button onClick={toggleSidebar} className="p-1 rounded hover:bg-white/10">
          <CollapseIcon size={20} />
        </button>
      </div>

      <nav className="flex-1 py-4 space-y-1 overflow-y-auto">
        {links.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 px-4 py-2.5 mx-2 rounded-lg transition-colors',
                isActive
                  ? 'bg-white/20 text-white font-medium'
                  : 'text-white/70 hover:bg-white/10 hover:text-white'
              )
            }
          >
            <Icon size={20} className="shrink-0" />
            {sidebarOpen && <span className="truncate">{label}</span>}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
