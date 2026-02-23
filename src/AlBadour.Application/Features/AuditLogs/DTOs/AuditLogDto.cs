namespace AlBadour.Application.Features.AuditLogs.DTOs;

public record AuditLogDto(
    long Id,
    Guid UserId,
    string UserName,
    string Action,
    string EntityType,
    string EntityId,
    string? Details,
    string? IpAddress,
    DateTime CreatedAt
);
