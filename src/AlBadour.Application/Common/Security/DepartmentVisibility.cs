using AlBadour.Domain.Enums;

namespace AlBadour.Application.Common.Security;

public static class DepartmentVisibility
{
    public static bool? GetAdministrativeLetterFilter(Department department) => department switch
    {
        Department.HR => true,
        Department.Inquiry or Department.Statistics => false,
        _ => null
    };

    // Returns a fragment to match against document type name (Contains), or null for no restriction.
    public static string? GetRequiredDocumentTypeName(Department department) => department switch
    {
        Department.Accounts => "Account Statement",
        Department.MoiInsurance => "MOI Insurance Letter",
        _ => null
    };

    // Returns a document type name to exclude, or null if nothing to exclude.
    public static string? GetExcludedDocumentTypeName(Department department) => department switch
    {
        Department.MoiInsurance => null,
        _ => "MOI Insurance Letter"
    };

    public static bool CanAccessDocumentType(Department department, string documentTypeNameEn)
    {
        if (department == Department.MoiInsurance)
            return documentTypeNameEn.Equals("MOI Insurance Letter", StringComparison.OrdinalIgnoreCase);

        if (documentTypeNameEn.Equals("MOI Insurance Letter", StringComparison.OrdinalIgnoreCase))
            return false;

        if (department == Department.Accounts)
            return documentTypeNameEn.Contains("Account Statement", StringComparison.OrdinalIgnoreCase);

        var filter = GetAdministrativeLetterFilter(department);
        if (!filter.HasValue)
            return true;

        var isAdministrativeLetter = documentTypeNameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);
        return filter.Value == isAdministrativeLetter;
    }
}
