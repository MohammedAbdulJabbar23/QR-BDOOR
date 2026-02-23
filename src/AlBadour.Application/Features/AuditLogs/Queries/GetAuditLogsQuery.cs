using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.AuditLogs.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.AuditLogs.Queries;

public record GetAuditLogsQuery(
    Guid? UserId,
    string? Action,
    string? EntityType,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<PaginatedList<AuditLogDto>>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
{
    private readonly IAuditLogRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetAuditLogsQueryHandler(IAuditLogRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Result.Failure<PaginatedList<AuditLogDto>>("Only admins can view audit logs.", "FORBIDDEN");

        var (items, totalCount) = await _repo.GetAllAsync(
            request.UserId, request.Action, request.EntityType,
            request.From, request.To,
            request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(a => new AuditLogDto(
            a.Id, a.UserId, a.UserName, a.Action, a.EntityType,
            a.EntityId, a.Details, a.IpAddress, a.CreatedAt
        )).ToList();

        return Result.Success(new PaginatedList<AuditLogDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
