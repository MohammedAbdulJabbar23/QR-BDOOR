using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.DocumentTypes.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentTypes.Queries;

public record GetDocumentTypesQuery(bool ActiveOnly = true) : IRequest<Result<List<DocumentTypeDto>>>;

public class GetDocumentTypesQueryHandler : IRequestHandler<GetDocumentTypesQuery, Result<List<DocumentTypeDto>>>
{
    private readonly IDocumentTypeRepository _repo;

    public GetDocumentTypesQueryHandler(IDocumentTypeRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<List<DocumentTypeDto>>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Entities.DocumentType> items;
        if (request.ActiveOnly)
        {
            items = await _repo.GetAllActiveAsync(cancellationToken);
        }
        else
        {
            var (all, _) = await _repo.GetAllAsync(1, 1000, cancellationToken);
            items = all;
        }

        var dtos = items.Select(t => new DocumentTypeDto(
            t.Id, t.NameAr, t.NameEn, t.DescriptionAr, t.DescriptionEn, t.IsActive, t.CreatedAt
        )).ToList();

        return Result.Success(dtos);
    }
}
