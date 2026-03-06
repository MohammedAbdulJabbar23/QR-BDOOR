namespace AlBadour.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, object? details = null, CancellationToken ct = default);
    Task LogAsync(Guid userId, string userName, string action, string entityType, string entityId, object? details = null, CancellationToken ct = default);
}
