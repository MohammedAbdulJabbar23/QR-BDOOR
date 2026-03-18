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

    public static bool CanAccessDocumentType(Department department, string documentTypeNameEn)
    {
        var filter = GetAdministrativeLetterFilter(department);
        if (!filter.HasValue)
            return true;

        var isAdministrativeLetter = documentTypeNameEn.Equals("Administrative Letter", StringComparison.OrdinalIgnoreCase);
        return filter.Value == isAdministrativeLetter;
    }
}
