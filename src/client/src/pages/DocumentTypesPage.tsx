import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Pencil, X, Check, ToggleLeft, ToggleRight } from 'lucide-react';

import PageHeader from '@/components/common/PageHeader';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import EmptyState from '@/components/common/EmptyState';
import { documentTypesApi } from '@/api/documentTypes.api';
import { useUiStore } from '@/stores/uiStore';
import { useAuthStore } from '@/stores/authStore';
import { canManageUsers } from '@/utils/permissions';
import { formatDate } from '@/utils/formatters';
import { cn } from '@/utils/cn';
import type { DocumentType } from '@/types/common.types';

const documentTypeSchema = z.object({
  nameAr: z.string().min(1, 'documentTypes.nameArRequired'),
  nameEn: z.string().min(1, 'documentTypes.nameEnRequired'),
  descriptionAr: z.string().optional(),
  descriptionEn: z.string().optional(),
});

type DocumentTypeFormValues = z.infer<typeof documentTypeSchema>;

export default function DocumentTypesPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const language = useUiStore((s) => s.language);
  const user = useAuthStore((s) => s.user);
  const isRtl = language === 'ar';

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingTypeId, setEditingTypeId] = useState<string | null>(null);

  const { data: documentTypes, isLoading, isError } = useQuery({
    queryKey: ['documentTypes', false],
    queryFn: () => documentTypesApi.getAll(false),
  });

  const createMutation = useMutation({
    mutationFn: (data: DocumentTypeFormValues) =>
      documentTypesApi.create({
        nameAr: data.nameAr,
        nameEn: data.nameEn,
        descriptionAr: data.descriptionAr || undefined,
        descriptionEn: data.descriptionEn || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documentTypes'] });
      setShowCreateForm(false);
      createForm.reset();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: DocumentTypeFormValues & { isActive: boolean };
    }) =>
      documentTypesApi.update(id, {
        nameAr: data.nameAr,
        nameEn: data.nameEn,
        descriptionAr: data.descriptionAr || undefined,
        descriptionEn: data.descriptionEn || undefined,
        isActive: data.isActive,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documentTypes'] });
      setEditingTypeId(null);
      editForm.reset();
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: (docType: DocumentType) =>
      documentTypesApi.update(docType.id, {
        nameAr: docType.nameAr,
        nameEn: docType.nameEn,
        descriptionAr: docType.descriptionAr || undefined,
        descriptionEn: docType.descriptionEn || undefined,
        isActive: !docType.isActive,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documentTypes'] });
    },
  });

  const createForm = useForm<DocumentTypeFormValues>({
    resolver: zodResolver(documentTypeSchema),
    defaultValues: {
      nameAr: '',
      nameEn: '',
      descriptionAr: '',
      descriptionEn: '',
    },
  });

  const editForm = useForm<DocumentTypeFormValues>({
    resolver: zodResolver(documentTypeSchema),
  });

  const handleStartEdit = useCallback(
    (docType: DocumentType) => {
      editForm.reset({
        nameAr: docType.nameAr,
        nameEn: docType.nameEn,
        descriptionAr: docType.descriptionAr ?? '',
        descriptionEn: docType.descriptionEn ?? '',
      });
      setEditingTypeId(docType.id);
    },
    [editForm],
  );

  const handleCancelEdit = useCallback(() => {
    setEditingTypeId(null);
    editForm.reset();
  }, [editForm]);

  const handleSaveEdit = useCallback(
    (formData: DocumentTypeFormValues) => {
      if (!editingTypeId || !documentTypes) return;
      const docType = documentTypes.find((dt) => dt.id === editingTypeId);
      if (!docType) return;
      updateMutation.mutate({
        id: editingTypeId,
        data: { ...formData, isActive: docType.isActive },
      });
    },
    [editingTypeId, documentTypes, updateMutation],
  );

  const handleCreateSubmit = useCallback(
    (formData: DocumentTypeFormValues) => {
      createMutation.mutate(formData);
    },
    [createMutation],
  );

  const getDisplayName = (docType: DocumentType) => {
    if (language === 'ar') return docType.nameAr;
    return docType.nameEn || docType.nameAr;
  };

  const getDisplayDescription = (docType: DocumentType) => {
    if (language === 'ar') return docType.descriptionAr || '--';
    return docType.descriptionEn || docType.descriptionAr || '--';
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
        title={t('documentTypes.title')}
        actions={
          !showCreateForm ? (
            <button
              onClick={() => setShowCreateForm(true)}
              className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
            >
              <Plus size={16} />
              {t('documentTypes.addType')}
            </button>
          ) : undefined
        }
      />

      {/* Create Form */}
      {showCreateForm && (
        <div className="bg-white rounded-xl border border-neutral-200 p-5 mb-4">
          <form onSubmit={createForm.handleSubmit(handleCreateSubmit)} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  {t('documentTypes.nameAr')} <span className="text-red-500">*</span>
                </label>
                <input
                  {...createForm.register('nameAr')}
                  className={cn(
                    'w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500',
                    createForm.formState.errors.nameAr ? 'border-red-400' : 'border-neutral-300',
                  )}
                  placeholder={t('documentTypes.nameAr')}
                  dir="rtl"
                />
                {createForm.formState.errors.nameAr && (
                  <p className="mt-1 text-xs text-red-600">
                    {t(createForm.formState.errors.nameAr.message as string)}
                  </p>
                )}
              </div>
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  {t('documentTypes.nameEn')} <span className="text-red-500">*</span>
                </label>
                <input
                  {...createForm.register('nameEn')}
                  className={cn(
                    'w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500',
                    createForm.formState.errors.nameEn ? 'border-red-400' : 'border-neutral-300',
                  )}
                  placeholder={t('documentTypes.nameEn')}
                  dir="ltr"
                />
                {createForm.formState.errors.nameEn && (
                  <p className="mt-1 text-xs text-red-600">
                    {t(createForm.formState.errors.nameEn.message as string)}
                  </p>
                )}
              </div>
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  {t('documentTypes.descriptionAr')}
                </label>
                <textarea
                  {...createForm.register('descriptionAr')}
                  rows={2}
                  className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 resize-none"
                  placeholder={t('documentTypes.descriptionAr')}
                  dir="rtl"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  {t('documentTypes.descriptionEn')}
                </label>
                <textarea
                  {...createForm.register('descriptionEn')}
                  rows={2}
                  className="w-full px-3 py-2 text-sm border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 resize-none"
                  placeholder={t('documentTypes.descriptionEn')}
                  dir="ltr"
                />
              </div>
            </div>

            {createMutation.isError && (
              <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
                {t('documentTypes.createError')}
              </div>
            )}

            <div className="flex items-center gap-3">
              <button
                type="submit"
                disabled={createMutation.isPending}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50"
              >
                {createMutation.isPending ? t('common.loading') : t('documentTypes.addType')}
              </button>
              <button
                type="button"
                onClick={() => {
                  setShowCreateForm(false);
                  createForm.reset();
                }}
                className="px-4 py-2 text-sm font-medium text-neutral-700 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors"
              >
                {t('common.cancel')}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Table */}
      {isLoading && <LoadingSpinner />}

      {isError && (
        <div className="text-center py-8 text-red-600">
          {t('common.error')}
        </div>
      )}

      {!isLoading && !isError && documentTypes && documentTypes.length === 0 && (
        <EmptyState
          title={t('documentTypes.noTypes')}
          action={
            !showCreateForm ? (
              <button
                onClick={() => setShowCreateForm(true)}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
              >
                {t('documentTypes.addType')}
              </button>
            ) : undefined
          }
        />
      )}

      {!isLoading && !isError && documentTypes && documentTypes.length > 0 && (
        <div className="bg-white rounded-xl border border-neutral-200 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-neutral-50 border-b border-neutral-200">
                  <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                    {t('documentTypes.name')}
                  </th>
                  <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                    {t('documentTypes.description')}
                  </th>
                  <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                    {t('documentTypes.isActive')}
                  </th>
                  <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                    {t('documentTypes.createdAt')}
                  </th>
                  <th className={cn('px-4 py-3 font-medium text-neutral-700', isRtl ? 'text-right' : 'text-left')}>
                    {t('documentTypes.actions')}
                  </th>
                </tr>
              </thead>
              <tbody>
                {documentTypes.map((docType) => (
                  <DocumentTypeRow
                    key={docType.id}
                    docType={docType}
                    isEditing={editingTypeId === docType.id}
                    editForm={editForm}
                    isRtl={isRtl}
                    language={language}
                    t={t}
                    getDisplayName={getDisplayName}
                    getDisplayDescription={getDisplayDescription}
                    onStartEdit={() => handleStartEdit(docType)}
                    onCancelEdit={handleCancelEdit}
                    onSaveEdit={editForm.handleSubmit(handleSaveEdit)}
                    onToggleActive={() => toggleActiveMutation.mutate(docType)}
                    isUpdating={updateMutation.isPending}
                    isToggling={toggleActiveMutation.isPending}
                  />
                ))}
              </tbody>
            </table>
          </div>

          {updateMutation.isError && (
            <div className="m-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
              {t('documentTypes.updateError')}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

interface DocumentTypeRowProps {
  docType: DocumentType;
  isEditing: boolean;
  editForm: ReturnType<typeof useForm<DocumentTypeFormValues>>;
  isRtl: boolean;
  language: string;
  t: (key: string) => string;
  getDisplayName: (docType: DocumentType) => string;
  getDisplayDescription: (docType: DocumentType) => string;
  onStartEdit: () => void;
  onCancelEdit: () => void;
  onSaveEdit: () => void;
  onToggleActive: () => void;
  isUpdating: boolean;
  isToggling: boolean;
}

function DocumentTypeRow({
  docType,
  isEditing,
  editForm,
  isRtl,
  language,
  t,
  getDisplayName,
  getDisplayDescription,
  onStartEdit,
  onCancelEdit,
  onSaveEdit,
  onToggleActive,
  isUpdating,
  isToggling,
}: DocumentTypeRowProps) {
  if (isEditing) {
    return (
      <tr className="border-b border-neutral-100 bg-neutral-50">
        <td className="px-4 py-2">
          <div className="space-y-1">
            <input
              {...editForm.register('nameAr')}
              className={cn(
                'w-full px-2 py-1 text-sm border rounded-md focus:outline-none focus:ring-1 focus:ring-red-500',
                editForm.formState.errors.nameAr ? 'border-red-400' : 'border-neutral-300',
              )}
              placeholder={t('documentTypes.nameAr')}
              dir="rtl"
            />
            <input
              {...editForm.register('nameEn')}
              className={cn(
                'w-full px-2 py-1 text-sm border rounded-md focus:outline-none focus:ring-1 focus:ring-red-500',
                editForm.formState.errors.nameEn ? 'border-red-400' : 'border-neutral-300',
              )}
              placeholder={t('documentTypes.nameEn')}
              dir="ltr"
            />
          </div>
        </td>
        <td className="px-4 py-2">
          <div className="space-y-1">
            <input
              {...editForm.register('descriptionAr')}
              className="w-full px-2 py-1 text-sm border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-red-500"
              placeholder={t('documentTypes.descriptionAr')}
              dir="rtl"
            />
            <input
              {...editForm.register('descriptionEn')}
              className="w-full px-2 py-1 text-sm border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-red-500"
              placeholder={t('documentTypes.descriptionEn')}
              dir="ltr"
            />
          </div>
        </td>
        <td className="px-4 py-2 text-neutral-400 text-xs">--</td>
        <td className="px-4 py-2 text-neutral-400 text-xs">--</td>
        <td className="px-4 py-2">
          <div className="flex items-center gap-1">
            <button
              onClick={onSaveEdit}
              disabled={isUpdating}
              className="p-1.5 text-green-600 hover:bg-green-50 rounded-lg transition-colors disabled:opacity-50"
              title={t('common.save')}
            >
              <Check size={16} />
            </button>
            <button
              onClick={onCancelEdit}
              className="p-1.5 text-neutral-500 hover:bg-neutral-100 rounded-lg transition-colors"
              title={t('common.cancel')}
            >
              <X size={16} />
            </button>
          </div>
        </td>
      </tr>
    );
  }

  return (
    <tr className={cn(
      'border-b border-neutral-100 hover:bg-neutral-50 transition-colors',
      !docType.isActive && 'opacity-60',
    )}>
      <td className="px-4 py-3">
        <div className="text-neutral-900 font-medium">{getDisplayName(docType)}</div>
        {language === 'ar' && docType.nameEn && (
          <div className="text-xs text-neutral-500 mt-0.5" dir="ltr">{docType.nameEn}</div>
        )}
        {language !== 'ar' && (
          <div className="text-xs text-neutral-500 mt-0.5" dir="rtl">{docType.nameAr}</div>
        )}
      </td>
      <td className="px-4 py-3 text-neutral-600 text-sm max-w-xs truncate">
        {getDisplayDescription(docType)}
      </td>
      <td className="px-4 py-3">
        <span
          className={cn(
            'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border',
            docType.isActive
              ? 'bg-green-50 text-green-800 border-green-200'
              : 'bg-neutral-100 text-neutral-600 border-neutral-200',
          )}
        >
          {docType.isActive ? t('users.active') : t('users.inactive')}
        </span>
      </td>
      <td className="px-4 py-3 text-neutral-500 text-sm">
        {formatDate(docType.createdAt, language)}
      </td>
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <button
            onClick={onStartEdit}
            className="p-1.5 text-neutral-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
            title={t('documentTypes.edit')}
          >
            <Pencil size={16} />
          </button>
          <button
            onClick={onToggleActive}
            disabled={isToggling}
            className={cn(
              'p-1.5 rounded-lg transition-colors disabled:opacity-50',
              docType.isActive
                ? 'text-green-600 hover:text-red-600 hover:bg-red-50'
                : 'text-neutral-400 hover:text-green-600 hover:bg-green-50',
            )}
            title={t('documentTypes.toggleActive')}
          >
            {docType.isActive ? <ToggleRight size={18} /> : <ToggleLeft size={18} />}
          </button>
        </div>
      </td>
    </tr>
  );
}
