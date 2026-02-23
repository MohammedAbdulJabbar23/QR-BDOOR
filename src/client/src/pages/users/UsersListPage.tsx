import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Pencil, UserX, Plus, X, Check } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';

import PageHeader from '@/components/common/PageHeader';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import EmptyState from '@/components/common/EmptyState';
import Pagination from '@/components/common/Pagination';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import { usersApi } from '@/api/users.api';
import { useUiStore } from '@/stores/uiStore';
import { useAuthStore } from '@/stores/authStore';
import { canManageUsers } from '@/utils/permissions';
import { formatDate } from '@/utils/formatters';
import { cn } from '@/utils/cn';
import { ROLES, DEPARTMENTS } from '@/utils/constants';
import type { UserDto } from '@/types/common.types';

const editSchema = z.object({
  fullName: z.string().min(1),
  fullNameEn: z.string().optional(),
  role: z.string().min(1),
  department: z.string().min(1),
});

type EditFormValues = z.infer<typeof editSchema>;

export default function UsersListPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const language = useUiStore((s) => s.language);
  const user = useAuthStore((s) => s.user);
  const isRtl = language === 'ar';

  const [page, setPage] = useState(1);
  const [editingUserId, setEditingUserId] = useState<string | null>(null);
  const [deactivateTarget, setDeactivateTarget] = useState<UserDto | null>(null);

  const pageSize = 10;

  const { data, isLoading, isError } = useQuery({
    queryKey: ['users', page, pageSize],
    queryFn: () => usersApi.getAll({ page, pageSize }),
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => usersApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setDeactivateTarget(null);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data: updateData }: { id: string; data: EditFormValues }) =>
      usersApi.update(id, {
        fullName: updateData.fullName,
        fullNameEn: updateData.fullNameEn,
        role: updateData.role,
        department: updateData.department,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setEditingUserId(null);
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
  });

  const handleStartEdit = useCallback(
    (userItem: UserDto) => {
      reset({
        fullName: userItem.fullName,
        fullNameEn: userItem.fullNameEn ?? '',
        role: userItem.role,
        department: userItem.department,
      });
      setEditingUserId(userItem.id);
    },
    [reset],
  );

  const handleCancelEdit = useCallback(() => {
    setEditingUserId(null);
    reset();
  }, [reset]);

  const handleSaveEdit = useCallback(
    (formData: EditFormValues) => {
      if (!editingUserId) return;
      updateMutation.mutate({ id: editingUserId, data: formData });
    },
    [editingUserId, updateMutation],
  );

  const getRoleName = (role: string) => {
    const key = role.toLowerCase() as 'employee' | 'supervisor' | 'admin';
    return t(`roles.${key}`);
  };

  const getDepartmentName = (dept: string) => {
    const key = dept.toLowerCase() as 'inquiry' | 'statistics' | 'management';
    return t(`departments.${key}`);
  };

  const getUserDisplayName = (userItem: UserDto) => {
    if (language === 'ar') return userItem.fullName;
    return userItem.fullNameEn || userItem.fullName;
  };

  if (!user || !canManageUsers(user.role)) {
    return (
      <EmptyState
        title={t('common.error')}
        message={t('common.noData')}
      />
    );
  }

  return (
    <div>
      <PageHeader
        title={t('users.title')}
        actions={
          <button
            onClick={() => navigate('/users/create')}
            className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
          >
            <Plus size={16} />
            {t('users.create')}
          </button>
        }
      />

      {isLoading && <LoadingSpinner />}

      {isError && (
        <div className="text-center py-8 text-red-600">
          {t('common.error')}
        </div>
      )}

      {!isLoading && !isError && data && data.items.length === 0 && (
        <EmptyState
          title={t('users.noUsers')}
          action={
            <button
              onClick={() => navigate('/users/create')}
              className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
            >
              {t('users.create')}
            </button>
          }
        />
      )}

      {!isLoading && !isError && data && data.items.length > 0 && (
        <>
          <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-neutral-50 border-b border-neutral-200">
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('users.name')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('users.username')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('users.role')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('users.department')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('users.status')}
                    </th>
                    <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                      {t('users.actions')}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((userItem) => (
                    <tr
                      key={userItem.id}
                      className="border-b border-neutral-100 hover:bg-neutral-50 transition-colors"
                    >
                      {editingUserId === userItem.id ? (
                        <EditRow
                          register={register}
                          errors={errors}
                          isRtl={isRtl}
                          t={t}
                          onSave={handleSubmit(handleSaveEdit)}
                          onCancel={handleCancelEdit}
                          isSubmitting={updateMutation.isPending}
                        />
                      ) : (
                        <>
                          <td className="px-4 py-3 text-neutral-900 font-medium">
                            {getUserDisplayName(userItem)}
                          </td>
                          <td className="px-4 py-3 text-neutral-600 font-mono text-xs">
                            {userItem.username}
                          </td>
                          <td className="px-4 py-3 text-neutral-600">
                            {getRoleName(userItem.role)}
                          </td>
                          <td className="px-4 py-3 text-neutral-600">
                            {getDepartmentName(userItem.department)}
                          </td>
                          <td className="px-4 py-3">
                            <span
                              className={cn(
                                'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border',
                                userItem.isActive
                                  ? 'bg-green-50 text-green-800 border-green-200'
                                  : 'bg-neutral-100 text-neutral-600 border-neutral-200',
                              )}
                            >
                              {userItem.isActive ? t('users.active') : t('users.inactive')}
                            </span>
                          </td>
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-2">
                              <button
                                onClick={() => handleStartEdit(userItem)}
                                className="p-1.5 text-neutral-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                                title={t('common.edit')}
                              >
                                <Pencil size={16} />
                              </button>
                              {userItem.isActive && userItem.id !== user.id && (
                                <button
                                  onClick={() => setDeactivateTarget(userItem)}
                                  className="p-1.5 text-neutral-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                  title={t('users.deactivate')}
                                >
                                  <UserX size={16} />
                                </button>
                              )}
                            </div>
                          </td>
                        </>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            onPageChange={setPage}
          />

          {updateMutation.isError && (
            <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
              {t('users.updateError')}
            </div>
          )}
        </>
      )}

      <ConfirmDialog
        open={deactivateTarget !== null}
        title={t('users.deactivateConfirm')}
        message={t('users.deactivateMessage')}
        variant="danger"
        confirmLabel={t('users.deactivate')}
        onConfirm={() => {
          if (deactivateTarget) {
            deactivateMutation.mutate(deactivateTarget.id);
          }
        }}
        onCancel={() => setDeactivateTarget(null)}
      />
    </div>
  );
}

interface EditRowProps {
  register: ReturnType<typeof useForm<EditFormValues>>['register'];
  errors: ReturnType<typeof useForm<EditFormValues>>['formState']['errors'];
  isRtl: boolean;
  t: (key: string) => string;
  onSave: () => void;
  onCancel: () => void;
  isSubmitting: boolean;
}

function EditRow({ register, errors, isRtl, t, onSave, onCancel, isSubmitting }: EditRowProps) {
  return (
    <>
      <td className="px-4 py-2">
        <div className="space-y-1">
          <input
            {...register('fullName')}
            className={cn(
              'w-full px-2 py-1 text-sm border rounded-md focus:outline-none focus:ring-1 focus:ring-red-500',
              errors.fullName ? 'border-red-400' : 'border-neutral-300',
            )}
            placeholder={t('users.fullName')}
            dir="rtl"
          />
          <input
            {...register('fullNameEn')}
            className="w-full px-2 py-1 text-sm border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-red-500"
            placeholder={t('users.fullNameEn')}
            dir="ltr"
          />
        </div>
      </td>
      <td className="px-4 py-2 text-neutral-400 text-xs">--</td>
      <td className="px-4 py-2">
        <select
          {...register('role')}
          className={cn(
            'w-full px-2 py-1 text-sm border rounded-md focus:outline-none focus:ring-1 focus:ring-red-500',
            errors.role ? 'border-red-400' : 'border-neutral-300',
          )}
        >
          {ROLES.map((role) => (
            <option key={role} value={role}>
              {t(`roles.${role.toLowerCase()}`)}
            </option>
          ))}
        </select>
      </td>
      <td className="px-4 py-2">
        <select
          {...register('department')}
          className={cn(
            'w-full px-2 py-1 text-sm border rounded-md focus:outline-none focus:ring-1 focus:ring-red-500',
            errors.department ? 'border-red-400' : 'border-neutral-300',
          )}
        >
          {DEPARTMENTS.map((dept) => (
            <option key={dept} value={dept}>
              {t(`departments.${dept.toLowerCase()}`)}
            </option>
          ))}
        </select>
      </td>
      <td className="px-4 py-2">--</td>
      <td className="px-4 py-2">
        <div className="flex items-center gap-1">
          <button
            onClick={onSave}
            disabled={isSubmitting}
            className="p-1.5 text-green-600 hover:bg-green-50 rounded-lg transition-colors disabled:opacity-50"
            title={t('common.save')}
          >
            <Check size={16} />
          </button>
          <button
            onClick={onCancel}
            className="p-1.5 text-neutral-500 hover:bg-neutral-100 rounded-lg transition-colors"
            title={t('common.cancel')}
          >
            <X size={16} />
          </button>
        </div>
      </td>
    </>
  );
}
