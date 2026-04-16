using AlBadour.Application.Common.Models;
using AlBadour.Application.Common.Security;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using AlBadour.Application.Common.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Queries;

public record GetPendingRequestsQuery(Guid? DocumentTypeId = null) : IRequest<Result<List<RequestDto>>>;

public class GetPendingRequestsQueryHandler : IRequestHandler<GetPendingRequestsQuery, Result<List<RequestDto>>>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly ICurrentUserService _currentUser;

    public GetPendingRequestsQueryHandler(IDocumentRequestRepository requestRepo, ICurrentUserService currentUser)
    {
        _requestRepo = requestRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<RequestDto>>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
    {
        bool? isAdministrativeLetter = _currentUser.Department switch
        {
            Department.HR => true,
            Department.Statistics => false,
            _ => null
        };
        var requiredDocTypeName = DepartmentVisibility.GetRequiredDocumentTypeName(_currentUser.Department);
        var excludedDocTypeName = DepartmentVisibility.GetExcludedDocumentTypeName(_currentUser.Department);

        var items = await _requestRepo.GetPendingAsync(request.DocumentTypeId, isAdministrativeLetter, requiredDocTypeName, excludedDocTypeName, cancellationToken);

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
            e.UpdatedAt,
            e.Language
        )).ToList();

        return Result.Success(dtos);
    }
}
