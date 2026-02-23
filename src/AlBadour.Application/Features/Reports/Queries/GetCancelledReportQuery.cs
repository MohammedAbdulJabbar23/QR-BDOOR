using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
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
        if (_currentUser.Role != UserRole.Supervisor && _currentUser.Role != UserRole.Admin)
            return Result.Failure<List<CancelledDocumentDto>>("Only supervisors and admins can view reports.", "FORBIDDEN");

        var (allDocs, _) = await _documentRepo.GetAllAsync(DocumentStatus.Revoked, null, 1, 10000, cancellationToken);
        var filtered = allDocs.Where(d => d.RevokedAt.HasValue && d.RevokedAt.Value >= request.From && d.RevokedAt.Value <= request.To).ToList();

        var dtos = filtered.Select(d => new CancelledDocumentDto(
            d.DocumentNumber,
            d.Request.PatientName,
            d.RevocationReason,
            d.RevokedAt,
            d.ReplacementDocument?.DocumentNumber
        )).ToList();

        return Result.Success(dtos);
    }
}
