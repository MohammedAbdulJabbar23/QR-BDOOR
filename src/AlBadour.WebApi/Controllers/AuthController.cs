using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Features.Auth.Commands;
using AlBadour.Application.Features.Auth.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlBadour.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(IMediator mediator, ICurrentUserService currentUser, IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request.Username, request.Password));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.UserId, request.RefreshToken));
        if (!result.IsSuccess) return Unauthorized(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _mediator.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword));
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(new { message = "Password changed successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId);
        if (user is null) return NotFound();
        return Ok(new
        {
            user.Id, user.Username, user.FullName, user.FullNameEn,
            Role = user.Role.ToString(), Department = user.Department.ToString(),
            user.LanguagePreference, user.IsActive
        });
    }

    [HttpPut("me/language")]
    [Authorize]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId);
        if (user is null) return NotFound();
        if (request.Language != "ar" && request.Language != "en")
            return BadRequest(new { error = "Language must be 'ar' or 'en'." });

        user.LanguagePreference = request.Language;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { message = "Language updated." });
    }
}

public record RefreshTokenRequest(Guid UserId, string RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateLanguageRequest(string Language);
