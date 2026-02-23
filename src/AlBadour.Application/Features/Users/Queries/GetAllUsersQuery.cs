using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.Users.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Application.Common.Interfaces;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Users.Queries;

public record GetAllUsersQuery(int Page = 1, int PageSize = 50) : IRequest<Result<PaginatedList<UserDto>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedList<UserDto>>>
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;

    public GetAllUsersQueryHandler(IUserRepository userRepo, ICurrentUserService currentUser)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Result.Failure<PaginatedList<UserDto>>("Only admins can view users.", "FORBIDDEN");

        var (items, totalCount) = await _userRepo.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var dtos = items.Select(u => new UserDto(
            u.Id, u.Username, u.FullName, u.FullNameEn,
            u.Role.ToString(), u.Department.ToString(),
            u.LanguagePreference, u.IsActive, u.CreatedAt
        )).ToList();

        return Result.Success(new PaginatedList<UserDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
