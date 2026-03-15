using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Queries;

public record GetAllRequestsQuery(
    RequestStatus? Status,
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<RequestDto>>>;

public class GetAllRequestsQueryHandler : IRequestHandler<GetAllRequestsQuery, Result<PaginatedList<RequestDto>>>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly AlBadour.Application.Common.Interfaces.ICurrentUserService _currentUser;

    public GetAllRequestsQueryHandler(IDocumentRequestRepository requestRepo, AlBadour.Application.Common.Interfaces.ICurrentUserService currentUser)
    {
        _requestRepo = requestRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<RequestDto>>> Handle(GetAllRequestsQuery request, CancellationToken cancellationToken)
    {
        // Inquiry and HR staff only see their own requests
        Guid? createdById = _currentUser.Department == Department.Inquiry
            || _currentUser.Department == Department.HR
            ? _currentUser.UserId : null;

        var (items, totalCount) = await _requestRepo.GetAllAsync(
            request.Status, createdById, request.Search,
            request.FromDate, request.ToDate,
            request.Page, request.PageSize, cancellationToken);

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

        return Result.Success(new PaginatedList<RequestDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
