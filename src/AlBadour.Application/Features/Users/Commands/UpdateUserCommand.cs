using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Users.Commands;

public record UpdateUserCommand(Guid Id, string FullName, string? FullNameEn, string Role, string Department) : IRequest<Result>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public UpdateUserCommandHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuditService auditService)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Result.Failure("Only admins can update users.", "FORBIDDEN");

        var user = await _userRepo.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.", "NOT_FOUND");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return Result.Failure("Invalid role.", "INVALID_ROLE");

        if (!Enum.TryParse<Department>(request.Department, true, out var department))
            return Result.Failure("Invalid department.", "INVALID_DEPARTMENT");

        var before = new { user.FullName, Role = user.Role.ToString(), Department = user.Department.ToString() };

        user.FullName = request.FullName;
        user.FullNameEn = request.FullNameEn;
        user.Role = role;
        user.Department = department;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("user.updated", "user", user.Id.ToString(),
            new { before, after = new { user.FullName, Role = user.Role.ToString(), Department = user.Department.ToString() } },
            cancellationToken);

        return Result.Success();
    }
}
