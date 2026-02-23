# Al-Badour Hospital — Document Issuance & QR Verification System

## Complete Implementation Specification

**Version:** 2.0 — Ready for Development
**Date:** February 2026

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | .NET 9 (C#), ASP.NET Core Web API |
| **Architecture** | Clean Architecture + Domain-Driven Design (DDD) |
| **Database** | PostgreSQL 16+ |
| **ORM** | Entity Framework Core 9 |
| **Frontend** | React 18+ with TypeScript |
| **UI Library** | Tailwind CSS + shadcn/ui components |
| **State Management** | TanStack Query (React Query) for server state, Zustand for client state |
| **Routing** | React Router v6 |
| **Forms** | React Hook Form + Zod validation |
| **Internationalization** | react-i18next (Arabic RTL + English LTR) |
| **QR Code Generation** | QRCoder (.NET library) |
| **PDF Viewer** | react-pdf or pdf.js for verification page |
| **Authentication** | JWT (access + refresh tokens) |
| **API Documentation** | Swagger / OpenAPI via Swashbuckle |
| **File Storage** | Local disk initially (abstracted behind interface for future cloud migration) |
| **Notifications** | SignalR (real-time in-system notifications) |
| **Report Generation** | DocumentFormat.OpenXml (for .docx export) |
| **Logging** | Serilog with structured logging |
| **Validation** | FluentValidation |
| **Mapping** | Mapster or AutoMapper |
| **Testing** | xUnit + FluentAssertions + Moq |

---

## 1. Executive Summary

Al-Badour Hospital requires a web-based Document Issuance and Verification System that enables the hospital to issue official documents (medical reports, administrative letters, leave certificates, and other document types) to patients intended for external entities, with a built-in mechanism to verify document authenticity electronically via QR codes.

The system supports a multi-stage workflow involving two internal departments (Inquiry Department and Statistics Department), digital archiving of finalized documents, and a public-facing verification portal. The system will initially serve one hospital location with plans for multi-branch expansion.

---

## 2. System Objectives

- Prevent document forgery and unauthorized reproduction of hospital-issued documents.
- Enable instant electronic verification of document authenticity by any external entity via QR code scanning.
- Establish a permanent, centralized digital archive of all officially issued documents.
- Streamline the internal document request and issuance workflow between Inquiry and Statistics departments.
- Provide full audit trails and traceability for every action performed in the system.
- Support multiple document types with individual templates, and allow future addition of new types.
- Support Arabic and English languages with user-selectable interface language.

---

## 3. Confirmed System Parameters

| Parameter | Confirmed Value |
|-----------|----------------|
| Expected daily requests | 15–20 per day |
| Maximum concurrent users | 4 |
| Total system users (initial) | 5 |
| User accounts | One account per employee (individual) |
| Session limit per user | Not required |
| SMS / Email notifications | Not required |
| Patient SMS notifications | Not required |
| Internal system notifications | Required (rejection alerts, new requests, revocations) |
| System language | Arabic + English, user-selectable |
| Document content authoring | Manually written by Statistics staff |
| Templates per document type | Each document type has its own unique template |
| Data retention | Permanent (no auto-deletion), deletable with admin approval |
| Legal retention requirements | None currently specified |
| QR code expiry | Not decided yet — design DB field for future use |
| Report export format | Word (.docx) |
| Approval after PDF upload | Not required — PDF upload directly activates QR verification |
| Multi-branch plans | Yes — future expansion |
| Audit logging | Required — all actions logged per user |
| Permission levels | Employee / Supervisor / Admin |

---

## 4. Solution Architecture (Clean Architecture + DDD)

```
src/
├── AlBadour.Domain/                    # Domain Layer (innermost)
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── DocumentRequest.cs
│   │   ├── IssuedDocument.cs
│   │   ├── DocumentType.cs
│   │   ├── AuditLog.cs
│   │   └── Notification.cs
│   ├── Enums/
│   │   ├── RequestStatus.cs            # Pending, InProgress, Rejected, Completed
│   │   ├── DocumentStatus.cs           # Draft, Archived, Revoked
│   │   ├── UserRole.cs                 # Employee, Supervisor, Admin
│   │   └── Department.cs               # Inquiry, Statistics, Management
│   ├── ValueObjects/
│   │   ├── DocumentNumber.cs
│   │   └── QrCodeData.cs
│   ├── Events/
│   │   ├── RequestCreatedEvent.cs
│   │   ├── RequestRejectedEvent.cs
│   │   ├── DocumentArchivedEvent.cs
│   │   ├── DocumentRevokedEvent.cs
│   │   └── ModificationRequestedEvent.cs
│   ├── Interfaces/
│   │   ├── IDocumentRequestRepository.cs
│   │   ├── IIssuedDocumentRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── IDocumentTypeRepository.cs
│   │   ├── IAuditLogRepository.cs
│   │   ├── INotificationRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── IFileStorageService.cs
│   ├── Services/
│   │   └── DocumentNumberGenerator.cs
│   └── Exceptions/
│       ├── DomainException.cs
│       ├── UnauthorizedActionException.cs
│       └── DocumentNotFoundException.cs
│
├── AlBadour.Application/               # Application Layer
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── ICurrentUserService.cs
│   │   │   ├── IQrCodeService.cs
│   │   │   ├── IReportGenerationService.cs
│   │   │   ├── IJwtTokenService.cs
│   │   │   └── INotificationService.cs
│   │   ├── Behaviors/
│   │   │   ├── ValidationBehavior.cs
│   │   │   ├── LoggingBehavior.cs
│   │   │   └── AuditBehavior.cs
│   │   ├── Models/
│   │   │   ├── PaginatedList.cs
│   │   │   └── Result.cs
│   │   └── Mappings/
│   │       └── MappingProfile.cs
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── RefreshTokenCommand.cs
│   │   │   │   └── ChangePasswordCommand.cs
│   │   │   └── DTOs/
│   │   │       ├── LoginRequest.cs
│   │   │       └── AuthResponse.cs
│   │   ├── DocumentRequests/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateRequestCommand.cs
│   │   │   │   ├── UpdateRequestCommand.cs
│   │   │   │   ├── RejectRequestCommand.cs
│   │   │   │   ├── AcceptRequestCommand.cs
│   │   │   │   └── DeleteRequestCommand.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetRequestByIdQuery.cs
│   │   │   │   ├── GetPendingRequestsQuery.cs
│   │   │   │   ├── GetMyRequestsQuery.cs
│   │   │   │   └── GetAllRequestsQuery.cs
│   │   │   ├── Validators/
│   │   │   │   ├── CreateRequestValidator.cs
│   │   │   │   └── UpdateRequestValidator.cs
│   │   │   └── DTOs/
│   │   │       ├── RequestDto.cs
│   │   │       ├── CreateRequestDto.cs
│   │   │       └── UpdateRequestDto.cs
│   │   ├── IssuedDocuments/
│   │   │   ├── Commands/
│   │   │   │   ├── PrepareDocumentCommand.cs
│   │   │   │   ├── UploadPdfCommand.cs
│   │   │   │   ├── RevokeDocumentCommand.cs
│   │   │   │   ├── ModifyDocumentCommand.cs
│   │   │   │   └── DeleteDocumentCommand.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetDocumentByIdQuery.cs
│   │   │   │   ├── VerifyDocumentQuery.cs       # Public verification
│   │   │   │   ├── GetAllDocumentsQuery.cs
│   │   │   │   └── GetDocumentsByRequestQuery.cs
│   │   │   ├── Validators/
│   │   │   │   └── PrepareDocumentValidator.cs
│   │   │   └── DTOs/
│   │   │       ├── DocumentDto.cs
│   │   │       ├── VerificationResultDto.cs
│   │   │       └── PrepareDocumentDto.cs
│   │   ├── Users/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateUserCommand.cs
│   │   │   │   ├── UpdateUserCommand.cs
│   │   │   │   └── DeactivateUserCommand.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllUsersQuery.cs
│   │   │   │   └── GetUserByIdQuery.cs
│   │   │   └── DTOs/
│   │   │       ├── UserDto.cs
│   │   │       └── CreateUserDto.cs
│   │   ├── DocumentTypes/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateDocumentTypeCommand.cs
│   │   │   │   └── UpdateDocumentTypeCommand.cs
│   │   │   ├── Queries/
│   │   │   │   └── GetDocumentTypesQuery.cs
│   │   │   └── DTOs/
│   │   │       └── DocumentTypeDto.cs
│   │   ├── Notifications/
│   │   │   ├── Queries/
│   │   │   │   ├── GetMyNotificationsQuery.cs
│   │   │   │   └── GetUnreadCountQuery.cs
│   │   │   ├── Commands/
│   │   │   │   └── MarkAsReadCommand.cs
│   │   │   └── DTOs/
│   │   │       └── NotificationDto.cs
│   │   ├── AuditLogs/
│   │   │   ├── Queries/
│   │   │   │   └── GetAuditLogsQuery.cs
│   │   │   └── DTOs/
│   │   │       └── AuditLogDto.cs
│   │   └── Reports/
│   │       ├── Queries/
│   │       │   ├── GetDailyReportQuery.cs
│   │       │   ├── GetMedicalReportsSummaryQuery.cs
│   │       │   └── GetStatusBreakdownQuery.cs
│   │       └── DTOs/
│   │           └── ReportDto.cs
│   └── DependencyInjection.cs
│
├── AlBadour.Infrastructure/            # Infrastructure Layer
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/             # EF Core entity configurations
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── DocumentRequestConfiguration.cs
│   │   │   ├── IssuedDocumentConfiguration.cs
│   │   │   ├── DocumentTypeConfiguration.cs
│   │   │   ├── AuditLogConfiguration.cs
│   │   │   └── NotificationConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── DocumentRequestRepository.cs
│   │   │   ├── IssuedDocumentRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── DocumentTypeRepository.cs
│   │   │   ├── AuditLogRepository.cs
│   │   │   └── NotificationRepository.cs
│   │   ├── UnitOfWork.cs
│   │   ├── Migrations/
│   │   └── Interceptors/
│   │       └── AuditableEntityInterceptor.cs
│   ├── Services/
│   │   ├── QrCodeService.cs
│   │   ├── FileStorageService.cs
│   │   ├── JwtTokenService.cs
│   │   ├── CurrentUserService.cs
│   │   ├── ReportGenerationService.cs   # Generates .docx reports
│   │   └── NotificationService.cs       # SignalR-based
│   ├── Hubs/
│   │   └── NotificationHub.cs           # SignalR hub
│   └── DependencyInjection.cs
│
├── AlBadour.WebApi/                     # Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── DocumentRequestsController.cs
│   │   ├── IssuedDocumentsController.cs
│   │   ├── VerificationController.cs    # Public - no auth required
│   │   ├── UsersController.cs
│   │   ├── DocumentTypesController.cs
│   │   ├── NotificationsController.cs
│   │   ├── AuditLogsController.cs
│   │   └── ReportsController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   ├── AuditLoggingMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── Filters/
│   │   └── PermissionAuthorizationFilter.cs
│   ├── Program.cs
│   └── appsettings.json
│
└── client/                              # React Frontend
    ├── public/
    ├── src/
    │   ├── api/                         # API client layer
    │   │   ├── client.ts                # Axios instance with interceptors
    │   │   ├── auth.api.ts
    │   │   ├── requests.api.ts
    │   │   ├── documents.api.ts
    │   │   ├── users.api.ts
    │   │   ├── documentTypes.api.ts
    │   │   ├── notifications.api.ts
    │   │   ├── auditLogs.api.ts
    │   │   └── reports.api.ts
    │   ├── components/
    │   │   ├── ui/                      # shadcn/ui components
    │   │   ├── layout/
    │   │   │   ├── Sidebar.tsx
    │   │   │   ├── Header.tsx
    │   │   │   ├── MainLayout.tsx
    │   │   │   └── NotificationBell.tsx
    │   │   ├── common/
    │   │   │   ├── DataTable.tsx
    │   │   │   ├── StatusBadge.tsx
    │   │   │   ├── ConfirmDialog.tsx
    │   │   │   ├── LoadingSpinner.tsx
    │   │   │   ├── EmptyState.tsx
    │   │   │   └── PageHeader.tsx
    │   │   ├── requests/
    │   │   │   ├── CreateRequestForm.tsx
    │   │   │   ├── RequestCard.tsx
    │   │   │   ├── RequestDetails.tsx
    │   │   │   └── RequestStatusTimeline.tsx
    │   │   ├── documents/
    │   │   │   ├── DocumentCard.tsx
    │   │   │   ├── DocumentDetails.tsx
    │   │   │   ├── PdfUploader.tsx
    │   │   │   ├── QrCodeDisplay.tsx
    │   │   │   ├── RevokeDocumentDialog.tsx
    │   │   │   └── DocumentPrintView.tsx
    │   │   └── verification/
    │   │       ├── VerificationResult.tsx
    │   │       ├── VerifiedBadge.tsx
    │   │       ├── RevokedBadge.tsx
    │   │       └── InvalidBadge.tsx
    │   ├── pages/
    │   │   ├── auth/
    │   │   │   └── LoginPage.tsx
    │   │   ├── dashboard/
    │   │   │   └── DashboardPage.tsx
    │   │   ├── requests/
    │   │   │   ├── RequestsListPage.tsx
    │   │   │   ├── CreateRequestPage.tsx
    │   │   │   ├── RequestDetailsPage.tsx
    │   │   │   └── PendingRequestsPage.tsx   # Statistics view
    │   │   ├── documents/
    │   │   │   ├── DocumentsListPage.tsx
    │   │   │   ├── PrepareDocumentPage.tsx
    │   │   │   └── DocumentDetailsPage.tsx
    │   │   ├── verification/
    │   │   │   └── VerificationPage.tsx      # Public page - no auth
    │   │   ├── users/
    │   │   │   ├── UsersListPage.tsx
    │   │   │   └── CreateUserPage.tsx
    │   │   ├── reports/
    │   │   │   └── ReportsPage.tsx
    │   │   ├── audit/
    │   │   │   └── AuditLogsPage.tsx
    │   │   └── settings/
    │   │       ├── DocumentTypesPage.tsx
    │   │       └── ProfilePage.tsx
    │   ├── hooks/
    │   │   ├── useAuth.ts
    │   │   ├── useNotifications.ts       # SignalR connection
    │   │   ├── useRequests.ts
    │   │   ├── useDocuments.ts
    │   │   └── useLanguage.ts
    │   ├── stores/
    │   │   ├── authStore.ts
    │   │   └── uiStore.ts                # sidebar state, language, theme
    │   ├── i18n/
    │   │   ├── i18n.ts
    │   │   ├── ar.json                   # Arabic translations
    │   │   └── en.json                   # English translations
    │   ├── types/
    │   │   ├── auth.types.ts
    │   │   ├── request.types.ts
    │   │   ├── document.types.ts
    │   │   ├── user.types.ts
    │   │   └── common.types.ts
    │   ├── utils/
    │   │   ├── formatters.ts
    │   │   ├── constants.ts
    │   │   └── permissions.ts
    │   ├── router.tsx
    │   ├── App.tsx
    │   └── main.tsx
    ├── tailwind.config.ts
    ├── tsconfig.json
    ├── vite.config.ts
    └── package.json
```

---

## 5. Database Schema (PostgreSQL)

### 5.1 Users Table

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(100) NOT NULL UNIQUE,
    full_name VARCHAR(255) NOT NULL,
    full_name_en VARCHAR(255),
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('employee', 'supervisor', 'admin')),
    department VARCHAR(20) NOT NULL CHECK (department IN ('inquiry', 'statistics', 'management')),
    language_preference VARCHAR(5) NOT NULL DEFAULT 'ar' CHECK (language_preference IN ('ar', 'en')),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    refresh_token VARCHAR(500),
    refresh_token_expiry TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

### 5.2 Document Types Table

```sql
CREATE TABLE document_types (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name_ar VARCHAR(255) NOT NULL,
    name_en VARCHAR(255) NOT NULL,
    template_path VARCHAR(500),
    description_ar TEXT,
    description_en TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Seed initial types
INSERT INTO document_types (name_ar, name_en) VALUES
    ('تقرير طبي', 'Medical Report'),
    ('كتاب إداري', 'Administrative Letter');
```

### 5.3 Document Requests Table

```sql
CREATE TABLE document_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_name VARCHAR(255) NOT NULL,
    patient_name_en VARCHAR(255),
    recipient_entity VARCHAR(255) NOT NULL,
    document_type_id UUID NOT NULL REFERENCES document_types(id),
    notes TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'pending'
        CHECK (status IN ('pending', 'in_progress', 'rejected', 'completed')),
    rejection_reason TEXT,
    created_by UUID NOT NULL REFERENCES users(id),
    assigned_to UUID REFERENCES users(id),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_requests_status ON document_requests(status) WHERE is_deleted = FALSE;
CREATE INDEX idx_requests_created_by ON document_requests(created_by);
CREATE INDEX idx_requests_assigned_to ON document_requests(assigned_to);
CREATE INDEX idx_requests_created_at ON document_requests(created_at DESC);
```

### 5.4 Issued Documents Table

```sql
CREATE TABLE issued_documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_number VARCHAR(50) NOT NULL UNIQUE,
    request_id UUID NOT NULL REFERENCES document_requests(id),
    qr_code_url VARCHAR(500) NOT NULL UNIQUE,
    qr_code_image_path VARCHAR(500),
    pdf_file_path VARCHAR(500),
    document_body TEXT,                          -- The manually written content
    status VARCHAR(20) NOT NULL DEFAULT 'draft'
        CHECK (status IN ('draft', 'archived', 'revoked')),
    revocation_reason TEXT,
    replacement_document_id UUID REFERENCES issued_documents(id),
    qr_expires_at TIMESTAMPTZ,                  -- Reserved for future use
    issued_by UUID NOT NULL REFERENCES users(id),
    revoked_by UUID REFERENCES users(id),
    approved_by UUID REFERENCES users(id),      -- Admin who approved modifications
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    issued_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    archived_at TIMESTAMPTZ,
    revoked_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_documents_status ON issued_documents(status) WHERE is_deleted = FALSE;
CREATE INDEX idx_documents_number ON issued_documents(document_number);
CREATE INDEX idx_documents_request ON issued_documents(request_id);
CREATE INDEX idx_documents_issued_at ON issued_documents(issued_at DESC);
```

### 5.5 Audit Logs Table

```sql
CREATE TABLE audit_logs (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    user_name VARCHAR(255) NOT NULL,            -- Denormalized for fast reads
    action VARCHAR(100) NOT NULL,               -- e.g., 'request.created', 'document.archived', 'document.revoked'
    entity_type VARCHAR(50) NOT NULL,           -- 'request', 'document', 'user'
    entity_id VARCHAR(50) NOT NULL,
    details JSONB,                              -- { before: {...}, after: {...} }
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Audit logs are append-only — no UPDATE or DELETE should ever be performed
CREATE INDEX idx_audit_user ON audit_logs(user_id);
CREATE INDEX idx_audit_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_action ON audit_logs(action);
CREATE INDEX idx_audit_created ON audit_logs(created_at DESC);
```

### 5.6 Notifications Table

```sql
CREATE TABLE notifications (
    id BIGSERIAL PRIMARY KEY,
    recipient_user_id UUID NOT NULL REFERENCES users(id),
    title_ar VARCHAR(255) NOT NULL,
    title_en VARCHAR(255) NOT NULL,
    message_ar TEXT NOT NULL,
    message_en TEXT NOT NULL,
    entity_type VARCHAR(50),
    entity_id VARCHAR(50),
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_notifications_recipient ON notifications(recipient_user_id, is_read, created_at DESC);
```

---

## 6. API Endpoints

### 6.1 Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | No | Login, returns JWT + refresh token |
| POST | `/api/auth/refresh` | No | Refresh access token |
| POST | `/api/auth/change-password` | Yes | Change own password |
| GET | `/api/auth/me` | Yes | Get current user profile |
| PUT | `/api/auth/me/language` | Yes | Update language preference |

### 6.2 Document Requests

| Method | Endpoint | Auth | Roles | Description |
|--------|----------|------|-------|-------------|
| POST | `/api/requests` | Yes | Inquiry | Create new request |
| GET | `/api/requests` | Yes | All staff | List requests (filtered by role) |
| GET | `/api/requests/{id}` | Yes | All staff | Get request details |
| PUT | `/api/requests/{id}` | Yes | Inquiry | Update request (only if status=pending) |
| DELETE | `/api/requests/{id}` | Yes | Supervisor/Admin | Soft-delete request |
| POST | `/api/requests/{id}/accept` | Yes | Statistics | Accept and start processing |
| POST | `/api/requests/{id}/reject` | Yes | Statistics | Reject with reason |

### 6.3 Issued Documents

| Method | Endpoint | Auth | Roles | Description |
|--------|----------|------|-------|-------------|
| POST | `/api/documents` | Yes | Statistics | Prepare document (generates ref number + QR) |
| GET | `/api/documents` | Yes | All staff | List all documents |
| GET | `/api/documents/{id}` | Yes | All staff | Get document details |
| POST | `/api/documents/{id}/upload-pdf` | Yes | Statistics | Upload scanned PDF → status becomes 'archived' |
| PUT | `/api/documents/{id}/replace-pdf` | Yes | Supervisor | Replace PDF (requires admin approval) |
| POST | `/api/documents/{id}/revoke` | Yes | Supervisor | Revoke document (requires admin approval) |
| POST | `/api/documents/{id}/modify` | Yes | Supervisor | Request modification (requires admin approval) |
| DELETE | `/api/documents/{id}` | Yes | Supervisor/Admin | Soft-delete (requires admin approval) |
| GET | `/api/documents/{id}/qr-image` | Yes | Statistics | Download QR code as image for embedding |
| GET | `/api/documents/{id}/print` | Yes | Statistics | Get print-ready document view |

### 6.4 Public Verification (NO authentication)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/verify/{documentId}` | No | Verify document, returns status + metadata + PDF URL |
| GET | `/api/verify/{documentId}/pdf` | No | Stream the archived PDF (only if status=archived or show replacement if revoked) |

### 6.5 Users (Admin only)

| Method | Endpoint | Auth | Roles | Description |
|--------|----------|------|-------|-------------|
| GET | `/api/users` | Yes | Admin | List all users |
| POST | `/api/users` | Yes | Admin | Create new user |
| PUT | `/api/users/{id}` | Yes | Admin | Update user |
| PUT | `/api/users/{id}/deactivate` | Yes | Admin | Deactivate user account |

### 6.6 Document Types (Admin only)

| Method | Endpoint | Auth | Roles | Description |
|--------|----------|------|-------|-------------|
| GET | `/api/document-types` | Yes | All staff | List active document types |
| POST | `/api/document-types` | Yes | Admin | Create new type |
| PUT | `/api/document-types/{id}` | Yes | Admin | Update type |

### 6.7 Notifications

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | Yes | Get my notifications (paginated) |
| GET | `/api/notifications/unread-count` | Yes | Get unread count |
| PUT | `/api/notifications/{id}/read` | Yes | Mark as read |
| PUT | `/api/notifications/read-all` | Yes | Mark all as read |
| **SignalR** | `/hubs/notifications` | Yes | Real-time notification push |

### 6.8 Audit Logs (Admin only)

| Method | Endpoint | Auth | Roles | Description |
|--------|----------|------|-------|-------------|
| GET | `/api/audit-logs` | Yes | Admin | Query audit logs (filterable, paginated) |

### 6.9 Reports

| Method | Endpoint | Auth | Roles | Description |
|--------|----------|------|-------|-------------|
| GET | `/api/reports/daily?date={date}` | Yes | Supervisor/Admin | Daily request count |
| GET | `/api/reports/medical-summary?from={date}&to={date}` | Yes | Supervisor/Admin | Medical reports with leave days |
| GET | `/api/reports/status-breakdown?from={date}&to={date}` | Yes | Supervisor/Admin | Status breakdown |
| GET | `/api/reports/cancelled?from={date}&to={date}` | Yes | Supervisor/Admin | Cancelled/revoked documents |
| GET | `/api/reports/export/{reportType}?from={date}&to={date}` | Yes | Supervisor/Admin | Download report as .docx |

---

## 7. Workflow — Detailed Business Logic

### 7.1 Phase 1: Request Creation (Inquiry Department)

**Actor:** Employee (Inquiry Department)

1. Inquiry staff logs in and navigates to "New Request" page.
2. Fills in: Patient Name, Recipient Entity, Document Type (dropdown), Notes (optional).
3. Submits the form.
4. **System actions:**
   - Creates a `document_requests` record with `status = 'pending'`.
   - Logs audit: `request.created`.
   - Sends in-system notification to all Statistics Department users: "New document request submitted."
   - Pushes real-time notification via SignalR.
5. **Editing rule:** Inquiry staff CAN edit the request ONLY while `status = 'pending'`. Once Statistics accepts it (`status = 'in_progress'`), it becomes locked for Inquiry.

### 7.2 Phase 2: Document Preparation (Statistics Department)

**Actor:** Employee (Statistics Department)

1. Statistics staff sees pending requests in their dashboard.
2. Opens the request and reviews it.

**Path A — Accept:**
1. Clicks "Accept" → `status` changes to `'in_progress'`, `assigned_to` is set to this user.
2. System logs audit: `request.accepted`.
3. Staff opens the document preparation interface.
4. Staff selects/views the appropriate template for the document type.
5. Staff manually writes/types the document body content in a rich text editor.
6. Clicks "Prepare Document."
7. **System actions:**
   - Generates a unique `document_number` (format: `BD-YYYY-NNNNN`, e.g., `BD-2026-00042`).
   - Generates a UUID-based `document_id`.
   - Generates QR code encoding: `https://{hospital-domain}/verify/{document_id}`.
   - Creates `issued_documents` record with `status = 'draft'`.
   - Updates `document_requests.status` to `'completed'`.
   - Logs audit: `document.prepared`.
8. Staff can now view a print preview showing the document with embedded QR code.
9. Staff prints the document, signs it, and stamps it physically.
10. Staff makes a photocopy for internal records.

**Path B — Reject:**
1. Clicks "Reject" → must provide a written rejection reason.
2. `status` changes to `'rejected'`, `rejection_reason` is saved.
3. System logs audit: `request.rejected`.
4. System sends in-system notification to the original Inquiry staff: "Your request has been rejected. Reason: {reason}."
5. The rejected request remains in the system (viewable, deletable with permission).

### 7.3 Phase 3: Digital Archiving

**Actor:** Employee (Statistics Department)

1. Staff scans the signed/stamped photocopy to produce a PDF.
2. Opens the document record in the system and clicks "Upload PDF."
3. Selects the scanned PDF file.
4. **System actions:**
   - Validates file is a PDF and within size limits.
   - Stores the PDF file (initially on local disk, abstracted behind `IFileStorageService`).
   - Updates `issued_documents.pdf_file_path`.
   - Updates `issued_documents.status` to `'archived'`.
   - Sets `issued_documents.archived_at` to current timestamp.
   - **QR verification is now active immediately** — no approval step needed.
   - Logs audit: `document.archived`.
5. The document is now fully live and verifiable.

### 7.4 Phase 4: External Verification

**Actor:** Any external party (no login needed)

1. External party scans QR code on the physical document.
2. QR directs to: `https://{hospital-domain}/verify/{document_id}`.
3. **System checks:**

**If document exists AND status = 'archived':**
- Display green "✓ Verified Document" badge.
- Show: Document Number, Issue Date, Patient Name, Recipient Entity.
- Show embedded PDF viewer or download link.

**If document exists AND status = 'revoked':**
- Display orange/red "⚠ Document Revoked" badge.
- Show revocation date.
- If a replacement document exists (`replacement_document_id` is set), display and link to the updated approved version.
- Show hospital contact information.

**If document not found OR status = 'draft' OR is_deleted = true:**
- Display red "✗ Document Not Verified" badge.
- Message: "This document could not be verified. Please contact Al-Badour Hospital."
- Show hospital contact information.

---

## 8. Editing, Modification & Revocation Rules

### 8.1 Request Editing (Inquiry Department)

- Inquiry staff can edit their own requests **only** when `status = 'pending'`.
- Once the Statistics Department accepts the request (`status = 'in_progress'`), the request is locked.
- System must enforce this at the API level (not just UI).

### 8.2 Document Modification After QR Issuance

- Modifications after the QR code and reference number have been generated require **Admin/Manager approval**.
- The Supervisor from Statistics initiates the modification request.
- An Admin receives a notification and approves/denies.
- If approved, the Statistics staff can edit the document content and re-upload a new PDF.
- System logs all changes in the audit trail with before/after values.

### 8.3 Document Revocation

- Documents CAN be revoked after issuance and delivery to the patient.
- Revocation requires **Admin/Manager approval** with a documented reason.
- Upon revocation:
  - `status` → `'revoked'`, `revocation_reason` is saved, `revoked_at` is set.
  - The QR code **remains functional** but displays "Document Revoked."
  - If a corrected/replacement document is issued, `replacement_document_id` links to the new one.
  - The revoked document's QR verification page shows the revocation notice AND the updated version.

### 8.4 Deletion

- Archived documents can be soft-deleted (set `is_deleted = TRUE`).
- Deletion requires **Admin/Manager approval**.
- Soft-deleted records are hidden from normal views but retained in the database for audit.
- Audit logs are **never** deletable.

---

## 9. Document Number Format

Format: `BD-YYYY-NNNNN`

- `BD` = Al-Badour (hospital prefix)
- `YYYY` = 4-digit year
- `NNNNN` = 5-digit sequential number, zero-padded, resets annually

Example: `BD-2026-00001`, `BD-2026-00002`, ..., `BD-2026-00142`

Implementation: Use a PostgreSQL sequence or a counter table per year. Ensure thread-safe generation.

```sql
CREATE TABLE document_number_sequences (
    year INT PRIMARY KEY,
    last_number INT NOT NULL DEFAULT 0
);

-- To generate: UPDATE ... SET last_number = last_number + 1 RETURNING last_number
-- Wrap in a transaction for thread safety
```

---

## 10. QR Code Specification

### Content

Each QR code encodes a direct URL:
```
https://{hospital-domain}/verify/{document_id}
```

Where `{document_id}` is a UUID v4 (e.g., `a7f3b2c1-4d5e-6f78-9a0b-1c2d3e4f5a6b`). UUIDs are not guessable or enumerable.

### Generation

Use the QRCoder .NET library to generate QR codes as PNG images:
- Error correction level: **M** (Medium, 15% recovery)
- Module size: 10 pixels
- Quiet zone: 4 modules
- Output: PNG image stored on disk, path saved in `qr_code_image_path`

### Security

- UUIDs v4 prevent enumeration attacks.
- Rate-limit the `/api/verify/{id}` endpoint (e.g., 30 requests/minute per IP).
- HTTPS mandatory.
- Consider adding an HMAC signature to the QR URL for additional tamper-proofing in the future.

### QR Expiry

- Not currently required.
- `qr_expires_at` field is reserved in the database.
- Verification logic should check this field and return "expired" if set and past.

---

## 11. Notification Events

All notifications are in-system only (no SMS, no email). Delivered via SignalR for real-time push.

| Event | Recipients | Message (AR) | Message (EN) |
|-------|-----------|-------------|-------------|
| New request submitted | All Statistics dept. users | تم تقديم طلب وثيقة جديد | A new document request has been submitted |
| Request rejected | Original Inquiry submitter | تم رفض طلبك. السبب: {reason} | Your request was rejected. Reason: {reason} |
| Modification approval needed | All Admin users | يوجد طلب تعديل على وثيقة بانتظار الموافقة | A document modification request is pending approval |
| Document revoked | Relevant staff + Admins | تم إلغاء الوثيقة رقم {number} | Document {number} has been revoked |
| Modification approved | Requesting staff | تمت الموافقة على طلب التعديل | Your modification request has been approved |
| Modification denied | Requesting staff | تم رفض طلب التعديل | Your modification request has been denied |

---

## 12. Reports

All reports must be exportable as Word (.docx) files. Use `DocumentFormat.OpenXml` (Open XML SDK) for generation.

### 12.1 Daily Request Count
- Total requests submitted on a given date.
- Breakdown by status (pending, completed, rejected).

### 12.2 Medical Reports Summary
- Medical reports issued within a date range.
- Includes leave/absence days information if captured in document body.

### 12.3 Cancelled/Revoked Documents
- All documents with `status = 'revoked'` within a date range.
- Include revocation reason and replacement document number if exists.

### 12.4 Rejected Requests
- All requests with `status = 'rejected'` within a date range.
- Include rejection reason.
- Clearly tagged/labeled as "Rejected" in reports.

### 12.5 Status Breakdown
- Total count of requests/documents grouped by status.
- Filterable by date range.

---

## 13. Archiving & Data Retention

- All records are permanently stored by default.
- Soft-delete mechanism: `is_deleted` boolean flag. Deleted records are excluded from normal queries but remain in the database.
- No legal retention requirements currently exist.
- Design the system to support configurable retention policies in the future.
- PDF files stored on disk with daily automated backups.
- Database backed up daily.

---

## 14. Language & Localization

- System interface supports **Arabic** (primary) and **English**.
- Language is selectable per-user in their profile settings.
- Full RTL layout support for Arabic.
- Use `react-i18next` with JSON translation files (`ar.json`, `en.json`).
- All user-facing text (labels, buttons, messages, notifications, status names, error messages) must have both Arabic and English translations.
- Document templates are primarily Arabic with optional English fields.
- The public verification page supports both languages (auto-detect browser language, with manual toggle).

### RTL Implementation Notes
- Use Tailwind's `dir` and `rtl:` modifier classes.
- Set `dir="rtl"` on the `<html>` tag when Arabic is selected.
- Flip all directional padding/margins using `ltr:` and `rtl:` prefixes.
- Test all pages in both directions.

---

## 15. Frontend UI Specification

### 15.1 Design Philosophy

- **Clean, modern, professional** — medical/institutional feel.
- **Minimal and focused** — no visual clutter; generous whitespace.
- **Card-based layout** for content sections.
- **Consistent color scheme** based on hospital branding (blues and whites recommended).

### 15.2 Color Palette (suggested)

| Token | Color | Usage |
|-------|-------|-------|
| Primary | `#1B4F72` (dark blue) | Headers, sidebar, primary buttons |
| Primary Light | `#2E86C1` (medium blue) | Links, active states, highlights |
| Primary Lighter | `#EBF5FB` (light blue) | Backgrounds, hover states |
| Success | `#27AE60` | Verified badges, success states |
| Warning | `#F39C12` | Pending status, caution |
| Danger | `#E74C3C` | Rejected, revoked, errors |
| Neutral | `#F8F9FA` | Page backgrounds |
| Text Primary | `#2C3E50` | Body text |
| Text Secondary | `#7F8C8D` | Muted text, labels |

### 15.3 Layout Structure

```
┌────────────────────────────────────────────────────────────┐
│  Header (hospital logo, user name, language toggle,        │
│          notification bell with unread count, logout)       │
├──────────┬─────────────────────────────────────────────────┤
│          │                                                 │
│ Sidebar  │  Main Content Area                              │
│          │                                                 │
│ - Dash   │  ┌─────────────────────────────────────────┐   │
│ - Reqs   │  │ Page Header (title + action buttons)     │   │
│ - Docs   │  ├─────────────────────────────────────────┤   │
│ - Reports│  │                                         │   │
│ - Users  │  │  Content (tables, forms, cards)          │   │
│ - Audit  │  │                                         │   │
│ - Types  │  │                                         │   │
│          │  └─────────────────────────────────────────┘   │
│          │                                                 │
└──────────┴─────────────────────────────────────────────────┘
```

- Sidebar is collapsible.
- Sidebar items are role-filtered (Inquiry users don't see Users, Audit, Types).
- Header sticks to top.
- Notification bell shows red badge with unread count; dropdown shows recent notifications.

### 15.4 Page Specifications

#### Login Page
- Centered card on a clean background with hospital logo.
- Username + password fields.
- "Remember me" checkbox.
- Clean error messages for invalid credentials.
- Language toggle (AR/EN) in the corner.

#### Dashboard
- Summary statistics cards at the top:
  - Total Requests Today
  - Pending Requests
  - Documents Issued Today
  - Archived Documents
- Recent activity feed (latest 10 actions).
- Role-specific content:
  - **Inquiry:** "My Recent Requests" table + "Create New Request" CTA button.
  - **Statistics:** "Pending Requests Awaiting Processing" table + quick-action buttons.
  - **Admin:** All stats + system health overview.

#### Requests List Page
- Data table with columns: Request #, Patient Name, Recipient, Type, Status, Date, Actions.
- Status shown as colored badges (green=completed, yellow=pending, red=rejected, blue=in_progress).
- Filter bar: status filter, date range picker, search by patient name.
- Pagination.
- "Create New Request" button (Inquiry only).

#### Create/Edit Request Form
- Clean card-based form.
- Fields: Patient Name (text), Recipient Entity (text/autocomplete), Document Type (dropdown), Notes (textarea, optional).
- Real-time validation with clear error messages below fields.
- Submit + Cancel buttons.

#### Pending Requests Page (Statistics view)
- Cards or table showing all `status = 'pending'` requests.
- Each row/card has "Accept" (green) and "Reject" (red) action buttons.
- Reject opens a modal requiring a reason text.

#### Document Preparation Page
- Shows request details at the top (read-only card).
- Rich text editor for writing the document body.
- Template selection (auto-selected based on document type).
- "Prepare & Generate QR" button.
- After preparation: shows the generated document number, QR code preview, and "Print" button.

#### Document Details Page
- Document metadata card (number, status, dates, staff who issued).
- QR code display.
- PDF viewer (if uploaded).
- Action buttons based on status:
  - Draft: "Upload PDF" button.
  - Archived: "Revoke" button (Supervisor), "Replace PDF" (Supervisor).
  - Revoked: shows revocation reason + link to replacement if exists.
- Status timeline showing the document's lifecycle history.

#### PDF Upload Interface
- Drag-and-drop zone + file picker.
- Shows file name and size after selection.
- Upload progress indicator.
- Confirmation dialog: "Uploading this PDF will activate QR verification. Continue?"

#### Public Verification Page
- **Mobile-first** — designed primarily for smartphone screens.
- Hospital logo at top.
- Language toggle (AR/EN).

**Verified state:**
- Large green checkmark icon with "✓ Document Verified" text.
- Card showing: Document Number, Issue Date, Patient Name, Recipient Entity.
- PDF viewer or download button.

**Revoked state:**
- Large orange/red warning icon with "⚠ Document Revoked" text.
- Revocation date and reason (if public).
- If replacement exists: "An updated document has been issued" with link/view.

**Invalid state:**
- Large red X icon with "✗ Document Not Found" text.
- "This document could not be verified."
- Hospital contact information.

#### Users Management Page (Admin)
- Data table: Name, Username, Role, Department, Status, Actions.
- Create User form: username, full name (AR + EN), password, role dropdown, department dropdown.
- Edit / Deactivate actions.

#### Audit Logs Page (Admin)
- Data table: Timestamp, User, Action, Entity, Details.
- Filters: user, action type, entity type, date range.
- Expandable rows showing full details JSON.
- Immutable — no edit/delete actions.

#### Reports Page
- Date range picker.
- Report type selection (tabs or cards).
- Preview table on screen.
- "Export as Word (.docx)" button.

#### Settings — Document Types Page (Admin)
- List of document types with name (AR/EN), status toggle.
- "Add New Type" button.
- Edit type name/description.

---

## 16. Non-Functional Requirements

| Category | Requirement |
|----------|------------|
| Security | Role-based access control (3 levels); bcrypt password hashing; JWT authentication; HTTPS; CORS configured for frontend domain only; rate limiting on verification endpoint |
| Performance | Verification page < 3 seconds; internal pages < 2 seconds; support 4 concurrent users |
| Availability | Verification portal: 24/7; internal system: 24/7 preferred |
| Scalability | 15-20 documents/day; architecture supports multi-branch expansion |
| Data Retention | Permanent by default; soft-delete with admin approval |
| Backup | Daily automated backups (database + files); tested restoration |
| Language | Arabic (primary) + English; full RTL; user-selectable |
| Browser | Chrome, Firefox, Safari, Edge (latest 2 versions) |
| Mobile | Verification page fully responsive; internal dashboard desktop-optimized |

---

## 17. Audit Trail

Every action is logged. Audit logs are **append-only** and **immutable** (no update/delete, even for admins).

### Logged Actions

| Action Code | Trigger |
|------------|---------|
| `user.login` | User logs in |
| `user.logout` | User logs out |
| `user.created` | Admin creates a user |
| `user.updated` | Admin updates a user |
| `user.deactivated` | Admin deactivates a user |
| `request.created` | Inquiry creates a request |
| `request.updated` | Inquiry edits a request |
| `request.accepted` | Statistics accepts a request |
| `request.rejected` | Statistics rejects a request |
| `request.deleted` | Request soft-deleted |
| `document.prepared` | Statistics generates document + QR |
| `document.archived` | PDF uploaded, verification activated |
| `document.pdf_replaced` | PDF replaced on existing document |
| `document.revoked` | Document revoked |
| `document.modified` | Document content modified |
| `document.deleted` | Document soft-deleted |
| `modification.requested` | Supervisor requests modification approval |
| `modification.approved` | Admin approves modification |
| `modification.denied` | Admin denies modification |

### Log Entry Structure

```json
{
  "user_id": "uuid",
  "user_name": "Ahmed Hassan",
  "action": "document.revoked",
  "entity_type": "document",
  "entity_id": "uuid",
  "details": {
    "document_number": "BD-2026-00042",
    "before": { "status": "archived" },
    "after": { "status": "revoked", "revocation_reason": "Incorrect patient data" },
    "approved_by": "admin-user-uuid"
  },
  "ip_address": "192.168.1.100",
  "created_at": "2026-02-23T14:30:00Z"
}
```

---

## 18. Future Expansion (Design For, Don't Implement Yet)

The architecture should accommodate these future features without major refactoring:

1. **Multi-branch support:** Add a `branch_id` foreign key to users, requests, and documents. Separate document number sequences per branch. Branch-based data isolation.
2. **HIS/EHR integration:** Patient data lookup via an external API. Abstract behind an `IPatientService` interface.
3. **QR code expiry:** `qr_expires_at` field already in database. Add expiry check logic to verification endpoint (behind a feature flag).
4. **SMS/Email notifications:** Abstract notification dispatch behind `INotificationChannel` interface. Current implementation uses in-system only. Add SMS/Email channels later.
5. **Digital signatures:** Replace physical stamp/signature with cryptographic signatures embedded in the PDF.
6. **External API access:** RESTful API for external systems (e.g., Ministry of Health) to verify documents programmatically. Add API key authentication.

---

## 19. Acceptance Criteria

1. Inquiry staff can create and submit document requests with all required fields.
2. Inquiry staff can edit their requests before Statistics processing begins.
3. Statistics staff can view, accept, or reject incoming requests.
4. Rejection triggers an in-system real-time notification to Inquiry with the rejection reason.
5. Statistics staff can prepare documents using type-specific templates with manually authored content.
6. System auto-generates unique document reference numbers (BD-YYYY-NNNNN) and QR codes.
7. Scanned PDFs can be uploaded and linked to document records.
8. PDF upload immediately activates QR verification (no approval step).
9. Scanning a valid QR code displays the correct PDF and metadata on a mobile-friendly page.
10. Scanning a revoked document's QR shows "Revoked" status plus the updated version if one exists.
11. Scanning an invalid QR shows an appropriate error message.
12. Documents can be modified after issuance with admin approval.
13. Documents can be revoked with admin approval and documented reason.
14. All actions are logged in an immutable audit trail with user identity and IP.
15. Role-based access control enforces correct permissions for all 3 role levels.
16. Reports can be generated and exported as Word (.docx) files.
17. Interface supports Arabic + English with user-selectable language and full RTL.
18. Rejected and cancelled documents appear correctly labeled in reports.
19. Real-time notifications are delivered via SignalR.
20. The system handles 15-20 daily documents with 4 concurrent users without performance degradation.

---

## 20. Remaining Open Questions

These need client confirmation before finalizing:

1. Does the hospital have an existing domain/website for hosting the verification page?
2. Hosting preference: cloud or on-premise?
3. Should the public verification page show the patient's full name, or should it be partially masked for privacy?
4. Can the hospital provide the official letterhead design, logo, and template samples?
5. Should the revocation reason be visible on the public verification page, or only internally?
6. For multi-branch expansion: shared database or separate instances per branch?
7. Preferred password policy (minimum length, complexity)?
8. Is the document number format `BD-YYYY-NNNNN` acceptable, or do they prefer a different format?
