using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Queries;

public record GetPendingRequestsQuery : IRequest<Result<List<RequestDto>>>;

public class GetPendingRequestsQueryHandler : IRequestHandler<GetPendingRequestsQuery, Result<List<RequestDto>>>
{
    private readonly IDocumentRequestRepository _requestRepo;

    public GetPendingRequestsQueryHandler(IDocumentRequestRepository requestRepo)
    {
        _requestRepo = requestRepo;
    }

    public async Task<Result<List<RequestDto>>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
    {
        var items = await _requestRepo.GetPendingAsync(cancellationToken);

        var dtos = items.Select(e => new RequestDto(
            e.Id,
            e.PatientName,
            e.PatientNameEn,
            e.RecipientEntity,
            e.DocumentTypeId,
            e.DocumentType.NameAr,
            e.DocumentType.NameEn,
            e.Notes,
            e.Status.ToString(),
            e.RejectionReason,
            e.CreatedById,
            e.CreatedBy.FullName,
            e.AssignedToId,
            e.AssignedTo?.FullName,
            e.CreatedAt,
            e.UpdatedAt
        )).ToList();

        return Result.Success(dtos);
    }
}
