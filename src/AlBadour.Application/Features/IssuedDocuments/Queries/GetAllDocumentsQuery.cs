using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Queries;

public record GetAllDocumentsQuery(
    DocumentStatus? Status,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<DocumentDto>>>;

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, Result<PaginatedList<DocumentDto>>>
{
    private readonly IIssuedDocumentRepository _documentRepo;

    public GetAllDocumentsQueryHandler(IIssuedDocumentRepository documentRepo)
    {
        _documentRepo = documentRepo;
    }

    public async Task<Result<PaginatedList<DocumentDto>>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _documentRepo.GetAllAsync(
            request.Status, request.Search,
            request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(GetDocumentByIdQueryHandler.MapToDto).ToList();
        return Result.Success(new PaginatedList<DocumentDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
