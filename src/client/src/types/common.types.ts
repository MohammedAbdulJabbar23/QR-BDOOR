export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface DocumentType {
  id: string;
  nameAr: string;
  nameEn: string;
  descriptionAr: string | null;
  descriptionEn: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface UserDto {
  id: string;
  username: string;
  fullName: string;
  fullNameEn: string | null;
  role: string;
  department: string;
  languagePreference: string;
  isActive: boolean;
  createdAt: string;
}

export interface NotificationDto {
  id: number;
  titleAr: string;
  titleEn: string;
  messageAr: string;
  messageEn: string;
  entityType: string | null;
  entityId: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface AuditLogDto {
  id: number;
  userId: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: string;
  details: string | null;
  ipAddress: string | null;
  createdAt: string;
}

export interface ApiError {
  error: string;
  code: string;
  errors?: Array<{ propertyName: string; errorMessage: string }>;
}
