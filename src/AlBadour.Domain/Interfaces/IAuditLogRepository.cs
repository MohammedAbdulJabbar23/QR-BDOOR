using AlBadour.Domain.Entities;

namespace AlBadour.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<(List<AuditLog> Items, int TotalCount)> GetAllAsync(
        Guid? userId, string? action, string? entityType,
        DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken ct = default);
}
