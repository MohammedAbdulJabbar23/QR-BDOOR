using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Common.Security;
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
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin
            && _currentUser.Department != Department.Statistics)
            return Result.Failure<DailyReportDto>("Only supervisors and admins can view reports.", "FORBIDDEN");

        var date = request.Date.Date;
        var isAdministrativeLetter = DepartmentVisibility.GetAdministrativeLetterFilter(_currentUser.Department);
        var requiredDocTypeName = DepartmentVisibility.GetRequiredDocumentTypeName(_currentUser.Department);
        var excludedDocTypeName = DepartmentVisibility.GetExcludedDocumentTypeName(_currentUser.Department);

        var statusCounts = await _requestRepo.GetStatusCountsAsync(date, date, isAdministrativeLetter, requiredDocTypeName, excludedDocTypeName, cancellationToken);
        var totalRequests = statusCounts.Values.Sum();
        var pending = statusCounts.GetValueOrDefault(RequestStatus.Pending.ToString(), 0);
        var completed = statusCounts.GetValueOrDefault(RequestStatus.Completed.ToString(), 0);
        var rejected = statusCounts.GetValueOrDefault(RequestStatus.Rejected.ToString(), 0);

        var docsIssued = await _documentRepo.CountAsync(null, date, date, isAdministrativeLetter, requiredDocTypeName, excludedDocTypeName, cancellationToken);
        var docsArchived = await _documentRepo.CountArchivedInRangeAsync(date, date, isAdministrativeLetter, requiredDocTypeName, excludedDocTypeName, cancellationToken);

        return Result.Success(new DailyReportDto(
            date,
            totalRequests,
            pending,
            completed,
            rejected,
            docsIssued,
            docsArchived
        ));
    }
}
