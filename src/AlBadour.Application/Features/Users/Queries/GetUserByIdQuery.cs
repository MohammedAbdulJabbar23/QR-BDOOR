using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.Users.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepo;

    public GetUserByIdQueryHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.Failure<UserDto>("User not found.", "NOT_FOUND");

        return Result.Success(new UserDto(
            user.Id, user.Username, user.FullName, user.FullNameEn,
            user.Role.ToString(), user.Department.ToString(),
            user.LanguagePreference, user.IsActive, user.CreatedAt
        ));
    }
}
