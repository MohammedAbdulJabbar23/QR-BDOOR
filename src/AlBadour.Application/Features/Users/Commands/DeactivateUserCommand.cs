using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Users.Commands;

public record DeactivateUserCommand(Guid Id) : IRequest<Result>;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public DeactivateUserCommandHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuditService auditService)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Result.Failure("Only admins can deactivate users.", "FORBIDDEN");

        var user = await _userRepo.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.", "NOT_FOUND");

        if (user.Id == _currentUser.UserId)
            return Result.Failure("You cannot deactivate your own account.", "SELF_DEACTIVATION");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("user.deactivated", "user", user.Id.ToString(), null, cancellationToken);

        return Result.Success();
    }
}
