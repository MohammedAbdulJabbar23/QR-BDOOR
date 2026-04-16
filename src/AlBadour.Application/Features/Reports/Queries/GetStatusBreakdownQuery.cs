using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Common.Security;
using AlBadour.Application.Features.Reports.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Reports.Queries;

public record GetStatusBreakdownQuery(DateTime From, DateTime To) : IRequest<Result<List<StatusBreakdownDto>>>;

public class GetStatusBreakdownQueryHandler : IRequestHandler<GetStatusBreakdownQuery, Result<List<StatusBreakdownDto>>>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly ICurrentUserService _currentUser;

    public GetStatusBreakdownQueryHandler(IDocumentRequestRepository requestRepo, ICurrentUserService currentUser)
    {
        _requestRepo = requestRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<StatusBreakdownDto>>> Handle(GetStatusBreakdownQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin
            && _currentUser.Department != Department.Statistics)
            return Result.Failure<List<StatusBreakdownDto>>("Only supervisors and admins can view reports.", "FORBIDDEN");

        var isAdministrativeLetter = DepartmentVisibility.GetAdministrativeLetterFilter(_currentUser.Department);
        var requiredDocTypeName = DepartmentVisibility.GetRequiredDocumentTypeName(_currentUser.Department);
        var excludedDocTypeName = DepartmentVisibility.GetExcludedDocumentTypeName(_currentUser.Department);
        var statusCounts = await _requestRepo.GetStatusCountsAsync(request.From, request.To, isAdministrativeLetter, requiredDocTypeName, excludedDocTypeName, cancellationToken);

        var breakdown = statusCounts
            .Select(kv => new StatusBreakdownDto(kv.Key, kv.Value))
            .ToList();

        return Result.Success(breakdown);
    }
}
