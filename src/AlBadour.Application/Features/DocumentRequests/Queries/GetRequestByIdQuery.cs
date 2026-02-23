using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Queries;

public record GetRequestByIdQuery(Guid Id) : IRequest<Result<RequestDto>>;

public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, Result<RequestDto>>
{
    private readonly IDocumentRequestRepository _requestRepo;

    public GetRequestByIdQueryHandler(IDocumentRequestRepository requestRepo)
    {
        _requestRepo = requestRepo;
    }

    public async Task<Result<RequestDto>> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _requestRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return Result.Failure<RequestDto>("Request not found.", "NOT_FOUND");

        var dto = new RequestDto(
            entity.Id,
            entity.PatientName,
            entity.PatientNameEn,
            entity.RecipientEntity,
            entity.DocumentTypeId,
            entity.DocumentType.NameAr,
            entity.DocumentType.NameEn,
            entity.Notes,
            entity.Status.ToString(),
            entity.RejectionReason,
            entity.CreatedById,
            entity.CreatedBy.FullName,
            entity.AssignedToId,
            entity.AssignedTo?.FullName,
            entity.CreatedAt,
            entity.UpdatedAt
        );

        return Result.Success(dto);
    }
}
