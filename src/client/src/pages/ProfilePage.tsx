import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { authApi } from '@/api/auth.api';
import PageHeader from '@/components/common/PageHeader';
import { User, Shield, Globe, Loader2, Check, AlertCircle } from 'lucide-react';
import { cn } from '@/utils/cn';

export default function ProfilePage() {
  const { t, i18n } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const updateLanguage = useAuthStore((s) => s.updateLanguage);
  const { language, setLanguage } = useUiStore();

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [passwordMessage, setPasswordMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const getRoleLabel = (role: string) => {
    const key = role.toLowerCase();
    return t(`roles.${key}`, role);
  };

  const getDepartmentLabel = (department: string) => {
    const key = department.toLowerCase();
    return t(`departments.${key}`, department);
  };

  const handlePasswordSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setPasswordMessage(null);

    // Validation
    if (!currentPassword) {
      setPasswordMessage({ type: 'error', text: t('profile.currentPasswordRequired') });
      return;
    }
    if (!newPassword) {
      setPasswordMessage({ type: 'error', text: t('profile.newPasswordRequired') });
      return;
    }
    if (newPassword.length < 6) {
      setPasswordMessage({ type: 'error', text: t('profile.passwordMinLength') });
      return;
    }
    if (newPassword !== confirmPassword) {
      setPasswordMessage({ type: 'error', text: t('profile.passwordMismatch') });
      return;
    }

    setPasswordLoading(true);
    try {
      await authApi.changePassword(currentPassword, newPassword);
      setPasswordMessage({ type: 'success', text: t('profile.passwordChanged') });
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch {
      setPasswordMessage({ type: 'error', text: t('profile.passwordChangeFailed') });
    } finally {
      setPasswordLoading(false);
    }
  };

  const handleLanguageChange = async (lang: string) => {
    setLanguage(lang);
    i18n.changeLanguage(lang);
    updateLanguage(lang);
    try {
      await authApi.updateLanguage(lang);
    } catch {
      // Silently fail - the local state is already updated
    }
  };

  return (
    <div>
      <PageHeader title={t('profile.title')} />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 max-w-4xl">
        {/* Personal Information */}
        <div className="bg-white rounded-xl border border-neutral-200 p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="p-2.5 bg-primary-50 rounded-lg">
              <User size={20} className="text-primary" />
            </div>
            <h2 className="text-lg font-semibold text-neutral-800">
              {t('profile.personalInfo')}
            </h2>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm text-neutral-500 mb-1">
                {t('profile.name')}
              </label>
              <p className="text-sm font-medium text-neutral-800">
                {user?.fullName}
                {user?.fullNameEn && (
                  <span className="text-neutral-400 ms-2">({user.fullNameEn})</span>
                )}
              </p>
            </div>

            <div>
              <label className="block text-sm text-neutral-500 mb-1">
                {t('auth.username')}
              </label>
              <p className="text-sm font-medium text-neutral-800">{user?.username}</p>
            </div>

            <div>
              <label className="block text-sm text-neutral-500 mb-1">
                {t('profile.role')}
              </label>
              <p className="text-sm font-medium text-neutral-800">
                {getRoleLabel(user?.role || '')}
              </p>
            </div>

            <div>
              <label className="block text-sm text-neutral-500 mb-1">
                {t('profile.department')}
              </label>
              <p className="text-sm font-medium text-neutral-800">
                {getDepartmentLabel(user?.department || '')}
              </p>
            </div>
          </div>
        </div>

        {/* Language Preference */}
        <div className="bg-white rounded-xl border border-neutral-200 p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="p-2.5 bg-blue-50 rounded-lg">
              <Globe size={20} className="text-blue-600" />
            </div>
            <h2 className="text-lg font-semibold text-neutral-800">
              {t('profile.languagePreference')}
            </h2>
          </div>

          <div className="flex gap-3">
            <button
              onClick={() => handleLanguageChange('ar')}
              className={cn(
                'flex-1 py-3 px-4 rounded-lg border text-sm font-medium transition-colors',
                language === 'ar'
                  ? 'bg-primary text-white border-primary'
                  : 'bg-white text-neutral-700 border-neutral-300 hover:border-primary hover:text-primary',
              )}
            >
              {t('profile.arabic')}
            </button>
            <button
              onClick={() => handleLanguageChange('en')}
              className={cn(
                'flex-1 py-3 px-4 rounded-lg border text-sm font-medium transition-colors',
                language === 'en'
                  ? 'bg-primary text-white border-primary'
                  : 'bg-white text-neutral-700 border-neutral-300 hover:border-primary hover:text-primary',
              )}
            >
              {t('profile.english')}
            </button>
          </div>
        </div>

        {/* Change Password */}
        <div className="bg-white rounded-xl border border-neutral-200 p-6 lg:col-span-2">
          <div className="flex items-center gap-3 mb-6">
            <div className="p-2.5 bg-amber-50 rounded-lg">
              <Shield size={20} className="text-amber-600" />
            </div>
            <h2 className="text-lg font-semibold text-neutral-800">
              {t('auth.changePassword')}
            </h2>
          </div>

          {/* Password message */}
          {passwordMessage && (
            <div
              className={cn(
                'mb-4 p-3 rounded-lg flex items-center gap-2 text-sm',
                passwordMessage.type === 'success'
                  ? 'bg-success-light border border-green-200 text-green-700'
                  : 'bg-danger-light border border-red-200 text-red-700',
              )}
            >
              {passwordMessage.type === 'success' ? (
                <Check size={16} className="shrink-0" />
              ) : (
                <AlertCircle size={16} className="shrink-0" />
              )}
              {passwordMessage.text}
            </div>
          )}

          <form onSubmit={handlePasswordSubmit} className="max-w-lg space-y-4">
            <div>
              <label
                htmlFor="currentPassword"
                className="block text-sm font-medium text-neutral-700 mb-1.5"
              >
                {t('auth.currentPassword')}
              </label>
              <input
                id="currentPassword"
                type="password"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                autoComplete="current-password"
                className="w-full px-4 py-2.5 border border-neutral-300 rounded-lg text-sm
                  focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary
                  placeholder:text-neutral-400 transition-colors"
              />
            </div>

            <div>
              <label
                htmlFor="newPassword"
                className="block text-sm font-medium text-neutral-700 mb-1.5"
              >
                {t('auth.newPassword')}
              </label>
              <input
                id="newPassword"
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                autoComplete="new-password"
                className="w-full px-4 py-2.5 border border-neutral-300 rounded-lg text-sm
                  focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary
                  placeholder:text-neutral-400 transition-colors"
              />
              <p className="mt-1 text-xs text-neutral-400">
                {t('profile.passwordMinLength')}
              </p>
            </div>

            <div>
              <label
                htmlFor="confirmPassword"
                className="block text-sm font-medium text-neutral-700 mb-1.5"
              >
                {t('auth.confirmPassword')}
              </label>
              <input
                id="confirmPassword"
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                autoComplete="new-password"
                className="w-full px-4 py-2.5 border border-neutral-300 rounded-lg text-sm
                  focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary
                  placeholder:text-neutral-400 transition-colors"
              />
            </div>

            <button
              type="submit"
              disabled={passwordLoading || !currentPassword || !newPassword || !confirmPassword}
              className="px-6 py-2.5 bg-primary hover:bg-primary-700 disabled:bg-primary-200
                text-white font-medium text-sm rounded-lg transition-colors
                focus:outline-none focus:ring-2 focus:ring-primary/20 focus:ring-offset-2
                flex items-center gap-2"
            >
              {passwordLoading && <Loader2 size={16} className="animate-spin" />}
              {t('auth.changePassword')}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
