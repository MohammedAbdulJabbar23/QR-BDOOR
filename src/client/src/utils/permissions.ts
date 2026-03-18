export function canCreateRequest(department: string): boolean {
  return department === 'Inquiry' || department === 'HR';
}

function isAdministrativeLetter(documentTypeNameEn?: string): boolean {
  return documentTypeNameEn?.toLowerCase() === 'administrative letter';
}

export function canHandleRequestType(department: string, documentTypeNameEn?: string): boolean {
  if (!documentTypeNameEn) {
    return false;
  }

  if (department === 'HR') {
    return isAdministrativeLetter(documentTypeNameEn);
  }

  if (department === 'Inquiry' || department === 'Statistics') {
    return !isAdministrativeLetter(documentTypeNameEn);
  }

  return false;
}

export function canEditRequest(
  department: string,
  status: string,
  createdById: string,
  userId: string,
  documentTypeNameEn?: string,
): boolean {
  const isNotFinished = status !== 'Completed' && status !== 'Rejected';
  const isCreator = createdById === userId;
  const isResponsibleDepartment =
    (department === 'HR' || department === 'Statistics') && canHandleRequestType(department, documentTypeNameEn);
  const isAllowedCreator = isCreator && canHandleRequestType(department, documentTypeNameEn);
  return isNotFinished && (isAllowedCreator || isResponsibleDepartment);
}

export function canAcceptRejectRequest(department: string, documentTypeNameEn?: string): boolean {
  if (!documentTypeNameEn) {
    return department === 'Statistics' || department === 'HR';
  }

  return (department === 'Statistics' || department === 'HR')
    && canHandleRequestType(department, documentTypeNameEn);
}

export function canPrepareDocument(department: string, documentTypeNameEn?: string): boolean {
  if (!documentTypeNameEn) {
    return department === 'Statistics' || department === 'HR';
  }

  return (department === 'Statistics' || department === 'HR')
    && canHandleRequestType(department, documentTypeNameEn);
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
