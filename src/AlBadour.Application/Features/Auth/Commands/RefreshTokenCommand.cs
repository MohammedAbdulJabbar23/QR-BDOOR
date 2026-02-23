using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.Auth.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Auth.Commands;

public record RefreshTokenCommand(Guid UserId, string RefreshToken) : IRequest<Result<AuthResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(IUserRepository userRepo, IJwtTokenService jwtService, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            return Result.Failure<AuthResponse>("Invalid refresh token.", "INVALID_REFRESH_TOKEN");

        if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Result.Failure<AuthResponse>("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponse(
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
        ));
    }
}
