namespace AlBadour.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, object? details = null, CancellationToken ct = default);
}
