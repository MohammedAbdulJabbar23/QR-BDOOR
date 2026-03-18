import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowLeft, ArrowRight } from 'lucide-react';

import PageHeader from '@/components/common/PageHeader';
import { usersApi } from '@/api/users.api';
import { useUiStore } from '@/stores/uiStore';
import { useAuthStore } from '@/stores/authStore';
import { canManageUsers } from '@/utils/permissions';
import { cn } from '@/utils/cn';
import { ROLES, DEPARTMENTS } from '@/utils/constants';
import EmptyState from '@/components/common/EmptyState';
import type { ApiError } from '@/types/common.types';
import type { AxiosError } from 'axios';
import { getApiErrorMessage } from '@/utils/apiErrors';

const createUserSchema = z.object({
  username: z
    .string()
    .min(1, 'users.usernameRequired')
    .regex(/^[a-zA-Z0-9]+$/, 'users.usernameAlphanumeric'),
  fullName: z.string().min(1, 'users.fullNameRequired'),
  fullNameEn: z.string().optional(),
  password: z.string().min(6, 'users.passwordMinLength'),
  role: z.string().min(1, 'users.roleRequired'),
  department: z.string().min(1, 'users.departmentRequired'),
});

type CreateUserFormValues = z.infer<typeof createUserSchema>;

export default function CreateUserPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const language = useUiStore((s) => s.language);
  const user = useAuthStore((s) => s.user);
  const isRtl = language === 'ar';
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: {
      username: '',
      fullName: '',
      fullNameEn: '',
      password: '',
      role: '',
      department: '',
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateUserFormValues) =>
      usersApi.create({
        username: data.username,
        fullName: data.fullName,
        fullNameEn: data.fullNameEn || undefined,
        password: data.password,
        role: data.role,
        department: data.department,
      }),
    onSuccess: () => {
      navigate('/users');
    },
    onError: (error: AxiosError<ApiError>) => {
      const apiError = error.response?.data;
      if (apiError?.errors && apiError.errors.length > 0 && localStorage.getItem('lang') === 'en') {
        setServerError(apiError.errors.map((e) => e.errorMessage).join(', '));
        return;
      }

      setServerError(getApiErrorMessage(error));
    },
  });

  const onSubmit = (data: CreateUserFormValues) => {
    setServerError(null);
    createMutation.mutate(data);
  };

  if (!user || !canManageUsers(user.role)) {
    return (
      <EmptyState
        title={t('common.error')}
        message={t('common.noData')}
      />
    );
  }

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  return (
    <div>
      <PageHeader
        title={t('users.create')}
        actions={
          <button
            onClick={() => navigate('/users')}
            className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-neutral-700 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
          >
            <BackIcon size={16} />
            {t('common.back')}
          </button>
        }
      />

      <div className="max-w-2xl">
        <form onSubmit={handleSubmit(onSubmit)} className="bg-white rounded-xl border border-neutral-200 p-6 space-y-5">
          {serverError && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
              {serverError}
            </div>
          )}

          {/* Username */}
          <div>
            <label htmlFor="username" className="block text-sm font-medium text-neutral-700 mb-1">
              {t('users.username')} <span className="text-red-500">*</span>
            </label>
            <input
              id="username"
              {...register('username')}
              className={cn(
                'w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500',
                errors.username ? 'border-red-400' : 'border-neutral-300',
              )}
              placeholder={t('users.username')}
              dir="ltr"
              autoComplete="off"
            />
            {errors.username && (
              <p className="mt-1 text-xs text-red-600">{t(errors.username.message as string)}</p>
            )}
          </div>

          {/* Full Name Arabic */}
          <div>
            <label htmlFor="fullName" className="block text-sm font-medium text-neutral-700 mb-1">
              {t('users.fullName')} <span className="text-red-500">*</span>
            </label>
            <input
              id="fullName"
              {...register('fullName')}
              className={cn(
                'w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500',
                errors.fullName ? 'border-red-400' : 'border-neutral-300',
              )}
              placeholder={t('users.fullName')}
              dir="rtl"
            />
            {errors.fullName && (
              <p className="mt-1 text-xs text-red-600">{t(errors.fullName.message as string)}</p>
            )}
          </div>

          {/* Full Name English */}
          <div>
            <label htmlFor="fullNameEn" className="block text-sm font-medium text-neutral-700 mb-1">
              {t('users.fullNameEn')}
            </label>
            <input
              id="fullNameEn"
              {...register('fullNameEn')}
              className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
              placeholder={t('users.fullNameEn')}
              dir="ltr"
            />
          </div>

          {/* Password */}
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-neutral-700 mb-1">
              {t('users.password')} <span className="text-red-500">*</span>
            </label>
            <input
              id="password"
              type="password"
              {...register('password')}
              className={cn(
                'w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500',
                errors.password ? 'border-red-400' : 'border-neutral-300',
              )}
              placeholder="******"
              dir="ltr"
              autoComplete="new-password"
            />
            {errors.password && (
              <p className="mt-1 text-xs text-red-600">{t(errors.password.message as string)}</p>
            )}
          </div>

          {/* Role */}
          <div>
            <label htmlFor="role" className="block text-sm font-medium text-neutral-700 mb-1">
              {t('users.role')} <span className="text-red-500">*</span>
            </label>
            <select
              id="role"
              {...register('role')}
              className={cn(
                'w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500',
                errors.role ? 'border-red-400' : 'border-neutral-300',
              )}
            >
              <option value="">{t('users.selectRole')}</option>
              {ROLES.map((role) => (
                <option key={role} value={role}>
                  {t(`roles.${role.toLowerCase()}`)}
                </option>
              ))}
            </select>
            {errors.role && (
              <p className="mt-1 text-xs text-red-600">{t(errors.role.message as string)}</p>
            )}
          </div>

          {/* Department */}
          <div>
            <label htmlFor="department" className="block text-sm font-medium text-neutral-700 mb-1">
              {t('users.department')} <span className="text-red-500">*</span>
            </label>
            <select
              id="department"
              {...register('department')}
              className={cn(
                'w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500',
                errors.department ? 'border-red-400' : 'border-neutral-300',
              )}
            >
              <option value="">{t('users.selectDepartment')}</option>
              {DEPARTMENTS.map((dept) => (
                <option key={dept} value={dept}>
                  {t(`departments.${dept.toLowerCase()}`)}
                </option>
              ))}
            </select>
            {errors.department && (
              <p className="mt-1 text-xs text-red-600">{t(errors.department.message as string)}</p>
            )}
          </div>

          {/* Submit buttons */}
          <div className="flex items-center gap-3 pt-4 border-t border-neutral-200">
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="px-6 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {createMutation.isPending ? t('common.loading') : t('users.create')}
            </button>
            <button
              type="button"
              onClick={() => navigate('/users')}
              className="px-6 py-2 text-sm font-medium text-neutral-700 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
            >
              {t('common.cancel')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
