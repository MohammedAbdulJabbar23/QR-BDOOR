using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
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
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin)
            return Result.Failure<List<StatusBreakdownDto>>("Only supervisors and admins can view reports.", "FORBIDDEN");

        var (allRequests, _) = await _requestRepo.GetAllAsync(null, null, null, 1, 10000, cancellationToken);
        var filtered = allRequests.Where(r => r.CreatedAt >= request.From && r.CreatedAt <= request.To).ToList();

        var breakdown = filtered
            .GroupBy(r => r.Status.ToString())
            .Select(g => new StatusBreakdownDto(g.Key, g.Count()))
            .ToList();

        return Result.Success(breakdown);
    }
}
