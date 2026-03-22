using AlBadour.Application.Common.Models;
using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Security;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Queries;

public record GetAllDocumentsQuery(
    DocumentStatus? Status,
    string? Search,
    Guid? DocumentTypeId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<DocumentDto>>>;

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, Result<PaginatedList<DocumentDto>>>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly ICurrentUserService _currentUser;

    public GetAllDocumentsQueryHandler(IIssuedDocumentRepository documentRepo, ICurrentUserService currentUser)
    {
        _documentRepo = documentRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<DocumentDto>>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var isAdministrativeLetter = DepartmentVisibility.GetAdministrativeLetterFilter(_currentUser.Department);
        var requiredDocumentTypeName = DepartmentVisibility.GetRequiredDocumentTypeName(_currentUser.Department);
        var (items, totalCount) = await _documentRepo.GetAllAsync(
            request.Status, request.Search, request.DocumentTypeId,
            request.FromDate, request.ToDate,
            request.Page, request.PageSize, isAdministrativeLetter, requiredDocumentTypeName, cancellationToken);

        var dtos = items.Select(GetDocumentByIdQueryHandler.MapToDto).ToList();
        return Result.Success(new PaginatedList<DocumentDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
