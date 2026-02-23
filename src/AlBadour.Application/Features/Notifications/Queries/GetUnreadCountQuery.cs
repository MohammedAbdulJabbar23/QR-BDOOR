using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Notifications.Queries;

public record GetUnreadCountQuery : IRequest<Result<int>>;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    private readonly INotificationRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadCountQueryHandler(INotificationRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _repo.GetUnreadCountAsync(_currentUser.UserId, cancellationToken);
        return Result.Success(count);
    }
}
