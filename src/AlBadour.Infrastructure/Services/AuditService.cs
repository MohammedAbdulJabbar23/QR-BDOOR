using System.Text.Json;
using AlBadour.Application.Common.Interfaces;
using AlBadour.Domain.Entities;
using AlBadour.Domain.Interfaces;

namespace AlBadour.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IAuditLogRepository auditLogRepo, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _auditLogRepo = auditLogRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(string action, string entityType, string entityId, object? details = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details is not null ? JsonSerializer.Serialize(details) : null,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent,
            CreatedAt = DateTime.UtcNow
        };

        await _auditLogRepo.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
