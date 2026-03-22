using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Common.Security;
using AlBadour.Application.Features.Reports.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.Reports.Queries;

public record GetCancelledReportQuery(DateTime From, DateTime To) : IRequest<Result<List<CancelledDocumentDto>>>;

public class GetCancelledReportQueryHandler : IRequestHandler<GetCancelledReportQuery, Result<List<CancelledDocumentDto>>>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly ICurrentUserService _currentUser;

    public GetCancelledReportQueryHandler(IIssuedDocumentRepository documentRepo, ICurrentUserService currentUser)
    {
        _documentRepo = documentRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CancelledDocumentDto>>> Handle(GetCancelledReportQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin
            && _currentUser.Department != Department.Statistics)
            return Result.Failure<List<CancelledDocumentDto>>("Only supervisors and admins can view reports.", "FORBIDDEN");

        var isAdministrativeLetter = DepartmentVisibility.GetAdministrativeLetterFilter(_currentUser.Department);
        const int pageSize = 500;
        var page = 1;
        var allDocs = new List<Domain.Entities.IssuedDocument>();

        while (true)
        {
            var (items, totalCount) = await _documentRepo.GetAllAsync(
                DocumentStatus.Revoked,
                null,
                null,
                request.From,
                request.To,
                page,
                pageSize,
                isAdministrativeLetter,
                null,
                cancellationToken);

            if (items.Count == 0)
                break;

            allDocs.AddRange(items);

            if (allDocs.Count >= totalCount)
                break;

            page++;
        }

        var dtos = allDocs.Select(d => new CancelledDocumentDto(
            d.DocumentNumber,
            d.Request.PatientName,
            d.RevocationReason,
            d.RevokedAt,
            d.ReplacementDocument?.DocumentNumber
        )).ToList();

        return Result.Success(dtos);
    }
}
