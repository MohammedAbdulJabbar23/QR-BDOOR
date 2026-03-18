import type { AxiosError } from 'axios';
import type { ApiError } from '@/types/common.types';

const messages = {
  ar: {
    INVALID_CREDENTIALS: 'اسم المستخدم أو كلمة المرور غير صحيحة.',
    INVALID_REFRESH_TOKEN: 'انتهت الجلسة. يرجى تسجيل الدخول مرة أخرى.',
    DUPLICATE_USERNAME: 'اسم المستخدم مستخدم بالفعل.',
    INVALID_DOCUMENT_TYPE: 'نوع الوثيقة غير صالح.',
    DUPLICATE_DOCUMENT_NUMBER: 'رقم الوثيقة مستخدم بالفعل.',
    INVALID_STATUS: 'لا يمكن تنفيذ هذا الإجراء في الحالة الحالية.',
    FORBIDDEN: 'ليس لديك صلاحية لتنفيذ هذا الإجراء.',
    NOT_FOUND: 'العنصر المطلوب غير موجود.',
    VALIDATION_ERROR: 'يرجى التحقق من الحقول المطلوبة.',
    INVALID_ROLE: 'الدور المحدد غير صالح.',
    INVALID_DEPARTMENT: 'القسم المحدد غير صالح.',
    INVALID_FILE: 'الملف المرفوع غير صالح.',
    FILE_TOO_LARGE: 'حجم الملف أكبر من الحد المسموح.',
    INVALID_FILE_TYPE: 'نوع الملف غير مدعوم.',
    MISSING_QR: 'رمز التحقق غير متوفر حالياً.',
    PDF_REQUIRED: 'يجب إنشاء ملف الوثيقة أولاً.',
    UNKNOWN: 'حدث خطأ غير متوقع.'
  },
  en: {
    INVALID_CREDENTIALS: 'Invalid username or password.',
    INVALID_REFRESH_TOKEN: 'Session expired. Please log in again.',
    DUPLICATE_USERNAME: 'Username already exists.',
    INVALID_DOCUMENT_TYPE: 'Invalid document type.',
    DUPLICATE_DOCUMENT_NUMBER: 'Document number already exists.',
    INVALID_STATUS: 'This action is not allowed in the current status.',
    FORBIDDEN: 'You do not have permission to perform this action.',
    NOT_FOUND: 'The requested item was not found.',
    VALIDATION_ERROR: 'Please review the required fields.',
    INVALID_ROLE: 'Invalid role.',
    INVALID_DEPARTMENT: 'Invalid department.',
    INVALID_FILE: 'Invalid file.',
    FILE_TOO_LARGE: 'File size exceeds the allowed limit.',
    INVALID_FILE_TYPE: 'Unsupported file type.',
    MISSING_QR: 'QR code is not available right now.',
    PDF_REQUIRED: 'The document PDF must be generated first.',
    UNKNOWN: 'An unexpected error occurred.'
  }
} as const;

export function getApiErrorMessage(error: unknown): string {
  const lang = localStorage.getItem('lang') === 'en' ? 'en' : 'ar';
  const apiError = (error as AxiosError<ApiError>)?.response?.data;

  if (!apiError) return messages[lang].UNKNOWN;

  if (apiError.code && apiError.code in messages[lang]) {
    return messages[lang][apiError.code as keyof typeof messages.ar];
  }

  if (lang === 'ar') {
    return messages.ar.UNKNOWN;
  }

  return apiError.error || messages.en.UNKNOWN;
}
