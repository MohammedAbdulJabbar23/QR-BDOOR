import { useState, type FormEvent } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import { authApi } from '@/api/auth.api';
import { Cross, Eye, EyeOff, Loader2 } from 'lucide-react';
import type { ApiError } from '@/types/common.types';
import type { AxiosError } from 'axios';

export default function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { isAuthenticated, setAuth } = useAuthStore();
  const language = useUiStore((s) => s.language);

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authApi.login({ username, password });
      setAuth(response.accessToken, response.refreshToken, response.user);
      navigate('/', { replace: true });
    } catch (err) {
      const axiosError = err as AxiosError<ApiError>;
      if (axiosError.response?.status === 401) {
        setError(t('auth.invalidCredentials'));
      } else if (axiosError.response?.data?.error) {
        setError(axiosError.response.data.error);
      } else {
        setError(t('auth.loginError'));
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-50 px-4">
      {/* Background decorative elements */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute -top-40 -right-40 w-96 h-96 bg-primary-100 rounded-full opacity-50 blur-3xl" />
        <div className="absolute -bottom-40 -left-40 w-96 h-96 bg-primary-50 rounded-full opacity-50 blur-3xl" />
      </div>

      <div className="relative w-full max-w-md">
        {/* Card */}
        <div className="bg-white rounded-2xl shadow-xl border border-neutral-200 p-8">
          {/* Header with hospital branding */}
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-primary rounded-2xl mb-4">
              <Cross size={32} className="text-white" />
            </div>
            <h1 className="text-xl font-bold text-neutral-900">
              {language === 'ar' ? 'مستشفى البدور' : 'Al-Badour Hospital'}
            </h1>
            <p className="text-sm text-neutral-500 mt-1">
              {t('app.subtitle')}
            </p>
          </div>

          {/* Welcome text */}
          <div className="mb-6">
            <h2 className="text-lg font-semibold text-neutral-800">
              {t('auth.welcomeBack')}
            </h2>
            <p className="text-sm text-neutral-500 mt-0.5">
              {t('auth.loginSubtitle')}
            </p>
          </div>

          {/* Error message */}
          {error && (
            <div className="mb-4 p-3 bg-danger-light border border-red-200 rounded-lg">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          )}

          {/* Login form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label
                htmlFor="username"
                className="block text-sm font-medium text-neutral-700 mb-1.5"
              >
                {t('auth.username')}
              </label>
              <input
                id="username"
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                autoComplete="username"
                autoFocus
                className="w-full px-4 py-2.5 border border-neutral-300 rounded-lg text-sm
                  focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary
                  placeholder:text-neutral-400 transition-colors"
                placeholder={t('auth.username')}
              />
            </div>

            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-neutral-700 mb-1.5"
              >
                {t('auth.password')}
              </label>
              <div className="relative">
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  autoComplete="current-password"
                  className="w-full px-4 py-2.5 border border-neutral-300 rounded-lg text-sm
                    focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary
                    placeholder:text-neutral-400 transition-colors
                    ltr:pr-10 rtl:pl-10"
                  placeholder={t('auth.password')}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute top-1/2 -translate-y-1/2 ltr:right-3 rtl:left-3
                    text-neutral-400 hover:text-neutral-600 transition-colors"
                  tabIndex={-1}
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading || !username || !password}
              className="w-full py-2.5 px-4 bg-primary hover:bg-primary-700 disabled:bg-primary-200
                text-white font-medium text-sm rounded-lg transition-colors
                focus:outline-none focus:ring-2 focus:ring-primary/20 focus:ring-offset-2
                flex items-center justify-center gap-2"
            >
              {loading && <Loader2 size={18} className="animate-spin" />}
              {t('auth.loginButton')}
            </button>
          </form>
        </div>

        {/* Footer */}
        <p className="text-center text-xs text-neutral-400 mt-6">
          {language === 'ar'
            ? 'مستشفى البدور - نظام إدارة الوثائق'
            : 'Al-Badour Hospital - Document Management System'}
        </p>
      </div>
    </div>
  );
}
