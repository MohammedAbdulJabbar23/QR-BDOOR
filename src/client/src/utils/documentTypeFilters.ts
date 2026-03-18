import type { DocumentType } from '@/types/common.types';

export function isAdministrativeLetterType(documentType?: Pick<DocumentType, 'nameEn'> | null): boolean {
  return documentType?.nameEn.toLowerCase() === 'administrative letter';
}

export function filterDocumentTypesForDepartment(
  documentTypes: DocumentType[] | undefined,
  department?: string,
): DocumentType[] {
  if (!documentTypes) {
    return [];
  }

  return documentTypes.filter((documentType) => {
    const isAdministrativeLetter = isAdministrativeLetterType(documentType);

    if (department === 'HR') {
      return isAdministrativeLetter;
    }

    if (department === 'Inquiry' || department === 'Statistics') {
      return !isAdministrativeLetter;
    }

    return true;
  });
}
