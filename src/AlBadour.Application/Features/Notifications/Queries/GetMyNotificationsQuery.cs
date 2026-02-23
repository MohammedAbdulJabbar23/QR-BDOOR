using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.Notifications.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Notifications.Queries;

public record GetMyNotificationsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PaginatedList<NotificationDto>>>;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, Result<PaginatedList<NotificationDto>>>
{
    private readonly INotificationRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(INotificationRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repo.GetByRecipientAsync(
            _currentUser.UserId, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(n => new NotificationDto(
            n.Id, n.TitleAr, n.TitleEn, n.MessageAr, n.MessageEn,
            n.EntityType, n.EntityId, n.IsRead, n.CreatedAt
        )).ToList();

        return Result.Success(new PaginatedList<NotificationDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
