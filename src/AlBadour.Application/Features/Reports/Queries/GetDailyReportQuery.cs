using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.Reports.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Reports.Queries;

public record GetDailyReportQuery(DateTime Date) : IRequest<Result<DailyReportDto>>;

public class GetDailyReportQueryHandler : IRequestHandler<GetDailyReportQuery, Result<DailyReportDto>>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly ICurrentUserService _currentUser;

    public GetDailyReportQueryHandler(IDocumentRequestRepository requestRepo, IIssuedDocumentRepository documentRepo, ICurrentUserService currentUser)
    {
        _requestRepo = requestRepo;
        _documentRepo = documentRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<DailyReportDto>> Handle(GetDailyReportQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin)
            return Result.Failure<DailyReportDto>("Only supervisors and admins can view reports.", "FORBIDDEN");

        var date = request.Date.Date;
        var nextDate = date.AddDays(1);

        // Get all requests for the day (using GetAllAsync with broad filter)
        var (allRequests, _) = await _requestRepo.GetAllAsync(null, null, null, 1, 10000, cancellationToken);
        var dayRequests = allRequests.Where(r => r.CreatedAt >= date && r.CreatedAt < nextDate).ToList();

        var (allDocs, _) = await _documentRepo.GetAllAsync(null, null, 1, 10000, cancellationToken);
        var dayDocs = allDocs.Where(d => d.IssuedAt >= date && d.IssuedAt < nextDate).ToList();
        var dayArchived = allDocs.Where(d => d.ArchivedAt.HasValue && d.ArchivedAt.Value >= date && d.ArchivedAt.Value < nextDate).ToList();

        return Result.Success(new DailyReportDto(
            date,
            dayRequests.Count,
            dayRequests.Count(r => r.Status == RequestStatus.Pending),
            dayRequests.Count(r => r.Status == RequestStatus.Completed),
            dayRequests.Count(r => r.Status == RequestStatus.Rejected),
            dayDocs.Count,
            dayArchived.Count
        ));
    }
}
