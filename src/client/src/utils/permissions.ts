export function canCreateRequest(department: string): boolean {
  return department === 'Inquiry';
}

export function canEditRequest(department: string, status: string, createdById: string, userId: string): boolean {
  return department === 'Inquiry' && status === 'Pending' && createdById === userId;
}

export function canAcceptRejectRequest(department: string): boolean {
  return department === 'Statistics';
}

export function canPrepareDocument(department: string): boolean {
  return department === 'Statistics';
}

export function canUploadPdf(department: string): boolean {
  return department === 'Statistics';
}

export function canRevokeDocument(role: string): boolean {
  return role === 'Supervisor' || role === 'Admin';
}

export function canDeleteDocument(role: string): boolean {
  return role === 'Supervisor' || role === 'Admin';
}

export function canManageUsers(role: string): boolean {
  return role === 'Admin';
}

export function canViewAuditLogs(role: string): boolean {
  return role === 'Admin';
}

export function canViewReports(role: string, department?: string): boolean {
  return role === 'Supervisor' || role === 'Admin' || department === 'Statistics';
}
