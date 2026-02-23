using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.Users.DTOs;
using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Users.Commands;

public record CreateUserCommand(CreateUserDto Dto) : IRequest<Result<Guid>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public CreateUserCommandHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuditService auditService)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Result.Failure<Guid>("Only admins can create users.", "FORBIDDEN");

        var existing = await _userRepo.GetByUsernameAsync(request.Dto.Username, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>("Username already exists.", "DUPLICATE_USERNAME");

        if (!Enum.TryParse<UserRole>(request.Dto.Role, true, out var role))
            return Result.Failure<Guid>("Invalid role.", "INVALID_ROLE");

        if (!Enum.TryParse<Department>(request.Dto.Department, true, out var department))
            return Result.Failure<Guid>("Invalid department.", "INVALID_DEPARTMENT");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Dto.Username,
            FullName = request.Dto.FullName,
            FullNameEn = request.Dto.FullNameEn,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Dto.Password),
            Role = role,
            Department = department
        };

        await _userRepo.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("user.created", "user", user.Id.ToString(),
            new { user.Username, Role = user.Role.ToString(), Department = user.Department.ToString() }, cancellationToken);

        return Result.Success(user.Id);
    }
}
