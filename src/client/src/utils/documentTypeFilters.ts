import type { DocumentType } from '@/types/common.types';

export function isAdministrativeLetterType(documentType?: Pick<DocumentType, 'nameEn'> | null): boolean {
  return documentType?.nameEn.toLowerCase() === 'administrative letter';
}

function isWithTableType(documentType: Pick<DocumentType, 'nameEn'>): boolean {
  const name = documentType.nameEn.toLowerCase();
  return name.includes('with table') && !name.includes('without table');
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

    if (department === 'Inquiry') {
      // Inquiry only sees "with Table" variants as generic options
      return !isAdministrativeLetter && isWithTableType(documentType);
    }

    if (department === 'Statistics') {
      return !isAdministrativeLetter;
    }

    return true;
  });
}

/**
 * For Inquiry department: get display name without "with Table" / "مع جدول"
 */
export function getGenericDocTypeName(nameAr: string, nameEn: string, isArabic: boolean): string {
  if (isArabic) {
    return nameAr.replace(' مع جدول', '').replace(' بدون جدول', '');
  }
  return nameEn.replace(' with Table', '').replace(' without Table', '');
}

/**
 * Returns the two table-variant options (with/without table) that match the
 * Account Statement status of the current document type.
 */
export function getTableVariantOptions(
  documentTypes: DocumentType[],
  currentDocumentTypeNameEn: string,
): DocumentType[] {
  const hasAccountStatement = currentDocumentTypeNameEn.toLowerCase().includes('account statement');
  return documentTypes.filter((dt) => {
    const name = dt.nameEn.toLowerCase();
    if (!name.includes('medical report')) return false;
    const dtHasAccountStatement = name.includes('account statement');
    return dtHasAccountStatement === hasAccountStatement;
  });
}
