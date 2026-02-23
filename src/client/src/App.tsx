import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';
import MainLayout from '@/components/layout/MainLayout';
import LoadingSpinner from '@/components/common/LoadingSpinner';

// Public pages
const LoginPage = lazy(() => import('@/pages/LoginPage'));
const VerificationPage = lazy(() => import('@/pages/VerificationPage'));

// Dashboard & Profile
const DashboardPage = lazy(() => import('@/pages/DashboardPage'));
const ProfilePage = lazy(() => import('@/pages/ProfilePage'));

// Requests
const RequestsListPage = lazy(() => import('@/pages/requests/RequestsListPage'));
const CreateRequestPage = lazy(() => import('@/pages/requests/CreateRequestPage'));
const PendingRequestsPage = lazy(() => import('@/pages/requests/PendingRequestsPage'));
const RequestDetailsPage = lazy(() => import('@/pages/requests/RequestDetailsPage'));

// Documents
const DocumentsListPage = lazy(() => import('@/pages/documents/DocumentsListPage'));
const PrepareDocumentPage = lazy(() => import('@/pages/documents/PrepareDocumentPage'));
const DocumentDetailsPage = lazy(() => import('@/pages/documents/DocumentDetailsPage'));

// Users
const UsersListPage = lazy(() => import('@/pages/users/UsersListPage'));
const CreateUserPage = lazy(() => import('@/pages/users/CreateUserPage'));

// Admin & Reports
const ReportsPage = lazy(() => import('@/pages/ReportsPage'));
const AuditLogsPage = lazy(() => import('@/pages/AuditLogsPage'));
const DocumentTypesPage = lazy(() => import('@/pages/DocumentTypesPage'));

// 404
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'));

function SuspenseFallback() {
  return (
    <div className="flex items-center justify-center min-h-screen">
      <LoadingSpinner />
    </div>
  );
}

export default function App() {
  return (
    <Suspense fallback={<SuspenseFallback />}>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/verify/:documentId" element={<VerificationPage />} />

        {/* Protected routes with layout */}
        <Route path="/" element={<MainLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="requests" element={<RequestsListPage />} />
          <Route path="requests/create" element={<CreateRequestPage />} />
          <Route path="requests/pending" element={<PendingRequestsPage />} />
          <Route path="requests/:id" element={<RequestDetailsPage />} />
          <Route path="documents" element={<DocumentsListPage />} />
          <Route path="documents/prepare/:requestId" element={<PrepareDocumentPage />} />
          <Route path="documents/:id" element={<DocumentDetailsPage />} />
          <Route path="users" element={<UsersListPage />} />
          <Route path="users/create" element={<CreateUserPage />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="audit-logs" element={<AuditLogsPage />} />
          <Route path="settings/document-types" element={<DocumentTypesPage />} />
          <Route path="profile" element={<ProfilePage />} />
        </Route>

        {/* 404 catch-all */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  );
}
