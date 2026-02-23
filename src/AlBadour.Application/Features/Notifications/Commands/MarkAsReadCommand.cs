using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Notifications.Commands;

public record MarkAsReadCommand(long? Id) : IRequest<Result>;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly INotificationRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkAsReadCommandHandler(INotificationRepository repo, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            var notification = await _repo.GetByIdAsync(request.Id.Value, cancellationToken);
            if (notification is null)
                return Result.Failure("Notification not found.", "NOT_FOUND");

            if (notification.RecipientUserId != _currentUser.UserId)
                return Result.Failure("You can only mark your own notifications.", "FORBIDDEN");

            notification.IsRead = true;
        }
        else
        {
            await _repo.MarkAllAsReadAsync(_currentUser.UserId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
