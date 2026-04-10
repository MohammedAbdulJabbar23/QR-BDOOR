import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { requestsApi } from '@/api/requests.api';
import { documentTypesApi } from '@/api/documentTypes.api';
import { useAuthStore } from '@/stores/authStore';
import { useUiStore } from '@/stores/uiStore';
import PageHeader from '@/components/common/PageHeader';
import LoadingSpinner from '@/components/common/LoadingSpinner';
import type { CreateRequestDto } from '@/types/request.types';
import { getApiErrorMessage } from '@/utils/apiErrors';
import { getGenericDocTypeName } from '@/utils/documentTypeFilters';
import { filterDocumentTypesForDepartment, isAdministrativeLetterType } from '@/utils/documentTypeFilters';

const requestSchema = z.object({
  patientName: z.string().optional(),
  recipientEntity: z.string().min(1, 'required'),
  documentTypeId: z.string().min(1, 'required'),
  notes: z.string().optional(),
  language: z.string().optional(),
});

type RequestFormValues = z.infer<typeof requestSchema>;

export default function CreateRequestPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [searchParams] = useSearchParams();
  const language = useUiStore((s) => s.language);
  const isArabic = language === 'ar';
  const user = useAuthStore((s) => s.user);
  const department = user?.department;

  const editId = searchParams.get('edit');
  const isEditMode = !!editId;

  const [submitError, setSubmitError] = useState('');

  // Fetch document types for dropdown
  const { data: documentTypes, isLoading: loadingTypes } = useQuery({
    queryKey: ['documentTypes', true],
    queryFn: () => documentTypesApi.getAll(true),
  });

  // Fetch existing request for edit mode
  const { data: existingRequest, isLoading: loadingRequest } = useQuery({
    queryKey: ['request', editId],
    queryFn: () => requestsApi.getById(editId!),
    enabled: isEditMode,
  });

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<RequestFormValues>({
    resolver: zodResolver(requestSchema),
    defaultValues: {
      patientName: '',
      recipientEntity: '',
      documentTypeId: '',
      notes: '',
      language: 'Arabic',
    },
  });

  // Pre-fill form in edit mode once data is loaded
  useEffect(() => {
    if (existingRequest) {
      reset({
        patientName: existingRequest.patientName,
        recipientEntity: existingRequest.recipientEntity,
        documentTypeId: existingRequest.documentTypeId,
        notes: existingRequest.notes || '',
        language: existingRequest.language || 'Arabic',
      });
    }
  }, [existingRequest, reset]);

  const selectedDocumentTypeId = watch('documentTypeId');
  const selectedDocumentType = useMemo(
    () => documentTypes?.find((dt) => dt.id === selectedDocumentTypeId),
    [documentTypes, selectedDocumentTypeId]
  );
  const availableDocumentTypes = useMemo(
    () => filterDocumentTypesForDepartment(documentTypes, department),
    [department, documentTypes]
  );
  const isAdministrativeLetter = isAdministrativeLetterType(selectedDocumentType)
    || (!isEditMode && department === 'HR');

  useEffect(() => {
    if (!isEditMode && department === 'HR' && availableDocumentTypes.length === 1) {
      setValue('documentTypeId', availableDocumentTypes[0].id, { shouldValidate: true });
    }
  }, [availableDocumentTypes, department, isEditMode, setValue]);

  const createMutation = useMutation({
    mutationFn: (data: CreateRequestDto) => requestsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['requests'] });
      navigate('/requests');
    },
    onError: (error) => {
      setSubmitError(getApiErrorMessage(error));
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: CreateRequestDto) => requestsApi.update(editId!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['requests'] });
      queryClient.invalidateQueries({ queryKey: ['request', editId] });
      navigate('/requests');
    },
    onError: (error) => {
      setSubmitError(getApiErrorMessage(error));
    },
  });

  const isSubmitting = createMutation.isPending || updateMutation.isPending;

  const onSubmit = (values: RequestFormValues) => {
    setSubmitError('');
    if (!isAdministrativeLetter && !(values.patientName || '').trim()) {
      setSubmitError(t('requests.patientNameRequired'));
      return;
    }
    if (isAdministrativeLetter && !(values.notes || '').trim()) {
      setSubmitError(t('requests.topicRequired'));
      return;
    }

    const payload: CreateRequestDto = {
      patientName: isAdministrativeLetter ? '' : (values.patientName || '').trim(),
      recipientEntity: values.recipientEntity,
      documentTypeId: values.documentTypeId,
      notes: values.notes?.trim() || undefined,
      language: values.language || 'Arabic',
    };

    if (isEditMode) {
      updateMutation.mutate(payload);
    } else {
      createMutation.mutate(payload);
    }
  };

  if (isEditMode && loadingRequest) {
    return <LoadingSpinner />;
  }

  return (
    <div>
      <PageHeader title={isEditMode ? t('requests.edit') : t('requests.create')} />

      <div className="max-w-2xl">
        <div className="bg-white rounded-xl border border-neutral-200 p-6">
          {submitError && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
              {submitError}
            </div>
          )}

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            {!isAdministrativeLetter && (
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1.5">
                  {t('requests.patientName')} <span className="text-red-500">*</span>
                </label>
                <input
                  {...register('patientName')}
                  type="text"
                  className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
                  dir="rtl"
                />
              </div>
            )}

            {/* Language */}
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1.5">
                {t('requests.language')}
              </label>
              <select
                {...register('language')}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary bg-white"
              >
                <option value="Arabic">{t('requests.languageArabic')}</option>
                <option value="English">{t('requests.languageEnglish')}</option>
              </select>
            </div>

            {/* Recipient Entity */}
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1.5">
                {t('requests.recipientEntity')} <span className="text-red-500">*</span>
              </label>
              <input
                {...register('recipientEntity')}
                type="text"
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
              />
              {errors.recipientEntity && (
                <p className="mt-1 text-xs text-red-600">{t('requests.recipientEntity')}</p>
              )}
            </div>

            {/* Document Type */}
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1.5">
                {t('requests.documentType')} <span className="text-red-500">*</span>
              </label>
              {loadingTypes ? (
                <div className="py-2 text-sm text-neutral-400">{t('common.loading')}</div>
              ) : (
                <select
                  {...register('documentTypeId')}
                  className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary bg-white"
                >
                  <option value="">--</option>
                  {availableDocumentTypes.map((dt) => (
                    <option key={dt.id} value={dt.id}>
                      {department === 'Inquiry'
                        ? getGenericDocTypeName(dt.nameAr, dt.nameEn, isArabic)
                        : (isArabic ? dt.nameAr : dt.nameEn)}
                    </option>
                  ))}
                </select>
              )}
              {errors.documentTypeId && (
                <p className="mt-1 text-xs text-red-600">{t('requests.documentType')}</p>
              )}
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1.5">
                {isAdministrativeLetter ? t('requests.topic') : t('requests.notes')}
                {isAdministrativeLetter && <span className="text-red-500"> *</span>}
              </label>
              <textarea
                {...register('notes')}
                rows={4}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary resize-none"
                placeholder={isAdministrativeLetter ? t('requests.topic') : undefined}
              />
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3 pt-2">
              <button
                type="submit"
                disabled={isSubmitting}
                className="px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-700 transition-colors text-sm font-medium disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {isSubmitting
                  ? t('common.loading')
                  : t('common.save')}
              </button>
              <button
                type="button"
                onClick={() => navigate('/requests')}
                className="px-6 py-2 border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors text-sm font-medium text-neutral-700"
              >
                {t('common.cancel')}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
