import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { authApi } from '@/api/auth.api';
import { Bell, LogOut, Globe } from 'lucide-react';
import { useState, useEffect, useRef } from 'react';
import { notificationsApi } from '@/api/notifications.api';
import { useNavigate } from 'react-router-dom';
import { cn } from '@/utils/cn';

export default function Header() {
  const { t, i18n } = useTranslation();
  const { user, logout, updateLanguage } = useAuthStore();
  const { sidebarOpen, language, setLanguage } = useUiStore();
  const navigate = useNavigate();
  const [unreadCount, setUnreadCount] = useState(0);
  const [showNotif, setShowNotif] = useState(false);
  const [notifications, setNotifications] = useState<any[]>([]);
  const notifRef = useRef<HTMLDivElement>(null);
  const isRtl = language === 'ar';

  useEffect(() => {
    notificationsApi.getUnreadCount().then((r) => setUnreadCount(r.count)).catch(() => {});
    const interval = setInterval(() => {
      notificationsApi.getUnreadCount().then((r) => setUnreadCount(r.count)).catch(() => {});
    }, 30000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (notifRef.current && !notifRef.current.contains(e.target as Node)) setShowNotif(false);
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const toggleLang = async () => {
    const newLang = language === 'ar' ? 'en' : 'ar';
    setLanguage(newLang);
    i18n.changeLanguage(newLang);
    updateLanguage(newLang);
    try { await authApi.updateLanguage(newLang); } catch {}
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleBellClick = async () => {
    setShowNotif(!showNotif);
    if (!showNotif) {
      try {
        const res = await notificationsApi.getAll({ page: 1, pageSize: 10 });
        setNotifications(res.items);
      } catch {}
    }
  };

  return (
    <header
      className={cn(
        'fixed top-0 h-16 bg-white border-b border-neutral-200 z-30 flex items-center justify-between px-6 transition-all duration-300',
        isRtl ? (sidebarOpen ? 'left-0 right-64' : 'left-0 right-16') : (sidebarOpen ? 'right-0 left-64' : 'right-0 left-16')
      )}
    >
      <div className="flex items-center gap-2">
        <h1 className="text-lg font-semibold text-neutral-800 hidden md:block">{t('app.subtitle')}</h1>
      </div>

      <div className="flex items-center gap-4">
        <button onClick={toggleLang} className="flex items-center gap-1 px-2 py-1 text-sm rounded hover:bg-neutral-100" title="Toggle Language">
          <Globe size={18} />
          <span>{language === 'ar' ? 'EN' : 'عربي'}</span>
        </button>

        <div className="relative" ref={notifRef}>
          <button onClick={handleBellClick} className="relative p-2 rounded-lg hover:bg-neutral-100">
            <Bell size={20} />
            {unreadCount > 0 && (
              <span className="absolute -top-1 -right-1 bg-primary text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                {unreadCount > 9 ? '9+' : unreadCount}
              </span>
            )}
          </button>

          {showNotif && (
            <div className={cn(
              'absolute top-full mt-2 w-80 bg-white rounded-lg shadow-lg border border-neutral-200 max-h-96 overflow-y-auto',
              isRtl ? 'left-0' : 'right-0'
            )}>
              <div className="p-3 border-b border-neutral-100 flex items-center justify-between">
                <span className="font-medium text-sm">{language === 'ar' ? 'الإشعارات' : 'Notifications'}</span>
                {unreadCount > 0 && (
                  <button
                    onClick={async () => {
                      await notificationsApi.markAllAsRead();
                      setUnreadCount(0);
                      setNotifications(n => n.map(x => ({ ...x, isRead: true })));
                    }}
                    className="text-xs text-primary hover:underline"
                  >
                    {language === 'ar' ? 'قراءة الكل' : 'Mark all read'}
                  </button>
                )}
              </div>
              {notifications.length === 0 ? (
                <div className="p-4 text-center text-neutral-400 text-sm">
                  {language === 'ar' ? 'لا توجد إشعارات' : 'No notifications'}
                </div>
              ) : (
                notifications.map((n) => (
                  <div key={n.id} className={cn('p-3 border-b border-neutral-50 hover:bg-neutral-50', !n.isRead && 'bg-primary-50')}>
                    <p className="text-sm font-medium">{language === 'ar' ? n.titleAr : n.titleEn}</p>
                    <p className="text-xs text-neutral-500 mt-1">{language === 'ar' ? n.messageAr : n.messageEn}</p>
                  </div>
                ))
              )}
            </div>
          )}
        </div>

        <div className="flex items-center gap-2 ps-4 border-s border-neutral-200">
          <div className="text-sm">
            <p className="font-medium text-neutral-800">{user?.fullName}</p>
            <p className="text-xs text-neutral-500">{user?.role}</p>
          </div>
          <button onClick={handleLogout} className="p-2 rounded-lg hover:bg-red-50 text-neutral-500 hover:text-primary" title={t('auth.logout')}>
            <LogOut size={18} />
          </button>
        </div>
      </div>
    </header>
  );
}
