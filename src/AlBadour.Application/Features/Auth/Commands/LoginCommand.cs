using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.Auth.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<Result<AuthResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public LoginCommandHandler(
        IUserRepository userRepo,
        IJwtTokenService jwtService,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !user.IsActive)
            return Result.Failure<AuthResponse>("Invalid username or password.", "INVALID_CREDENTIALS");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>("Invalid username or password.", "INVALID_CREDENTIALS");

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("user.login", "user", user.Id.ToString(), null, cancellationToken);

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1),
            new UserInfo(
                user.Id,
                user.Username,
                user.FullName,
                user.FullNameEn,
                user.Role.ToString(),
                user.Department.ToString(),
                user.LanguagePreference
            )
        );

        return Result.Success(response);
    }
}
