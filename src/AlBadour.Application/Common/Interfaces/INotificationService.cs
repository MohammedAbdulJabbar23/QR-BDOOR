namespace AlBadour.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, string titleAr, string titleEn, string messageAr, string messageEn, string? entityType = null, string? entityId = null, CancellationToken ct = default);
    Task SendToDepartmentAsync(Domain.Enums.Department department, string titleAr, string titleEn, string messageAr, string messageEn, string? entityType = null, string? entityId = null, CancellationToken ct = default);
    Task SendToRoleAsync(Domain.Enums.UserRole role, string titleAr, string titleEn, string messageAr, string messageEn, string? entityType = null, string? entityId = null, CancellationToken ct = default);
}
