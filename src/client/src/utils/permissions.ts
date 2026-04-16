export function canCreateRequest(department: string): boolean {
  return department === 'Inquiry' || department === 'HR' || department === 'MoiInsurance';
}

function isAdministrativeLetter(documentTypeNameEn?: string): boolean {
  return documentTypeNameEn?.toLowerCase() === 'administrative letter';
}

function isMoiInsuranceLetter(documentTypeNameEn?: string): boolean {
  return documentTypeNameEn?.toLowerCase() === 'moi insurance letter';
}

export function canHandleRequestType(department: string, documentTypeNameEn?: string): boolean {
  if (!documentTypeNameEn) {
    return false;
  }

  if (department === 'MoiInsurance') {
    return isMoiInsuranceLetter(documentTypeNameEn);
  }

  if (department === 'HR') {
    return isAdministrativeLetter(documentTypeNameEn);
  }

  if (department === 'Inquiry' || department === 'Statistics') {
    return !isAdministrativeLetter(documentTypeNameEn) && !isMoiInsuranceLetter(documentTypeNameEn);
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
    (department === 'HR' || department === 'Statistics' || department === 'MoiInsurance') && canHandleRequestType(department, documentTypeNameEn);
  const isAllowedCreator = isCreator && canHandleRequestType(department, documentTypeNameEn);
  return isNotFinished && (isAllowedCreator || isResponsibleDepartment);
}

export function canAcceptRejectRequest(department: string, documentTypeNameEn?: string): boolean {
  if (!documentTypeNameEn) {
    return department === 'Statistics' || department === 'HR' || department === 'MoiInsurance';
  }

  return (department === 'Statistics' || department === 'HR' || department === 'MoiInsurance')
    && canHandleRequestType(department, documentTypeNameEn);
}

export function canPrepareDocument(department: string, documentTypeNameEn?: string): boolean {
  if (!documentTypeNameEn) {
    return department === 'Statistics' || department === 'HR' || department === 'MoiInsurance';
  }

  return (department === 'Statistics' || department === 'HR' || department === 'MoiInsurance')
    && canHandleRequestType(department, documentTypeNameEn);
}

export function canUploadPdf(department: string): boolean {
  return department === 'Statistics' || department === 'HR' || department === 'MoiInsurance';
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
