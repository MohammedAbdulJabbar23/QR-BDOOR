export function canCreateRequest(department: string): boolean {
  return department === 'Inquiry' || department === 'HR';
}

export function canEditRequest(department: string, status: string, createdById: string, userId: string): boolean {
  const isNotFinished = status !== 'Completed' && status !== 'Rejected';
  const isCreator = createdById === userId;
  const isHrOrStatistics = department === 'HR' || department === 'Statistics';
  return isNotFinished && (isCreator || isHrOrStatistics);
}

export function canAcceptRejectRequest(department: string): boolean {
  return department === 'Statistics' || department === 'HR';
}

export function canPrepareDocument(department: string): boolean {
  return department === 'Statistics' || department === 'HR';
}

export function canUploadPdf(department: string): boolean {
  return department === 'Statistics' || department === 'HR';
}

export function canRevokeDocument(role: string): boolean {
  return role === 'Supervisor' || role === 'Admin';
}

export function canDeleteDocument(role: string): boolean {
  return role === 'Supervisor' || role === 'Admin';
}

export function canTransferToAccounts(department: string): boolean {
  return department === 'Statistics';
}

export function canUploadAccountStatement(department: string): boolean {
  return department === 'Accounts';
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
