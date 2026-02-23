using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Queries;

public record GetDocumentsByRequestQuery(Guid RequestId) : IRequest<Result<List<DocumentDto>>>;

public class GetDocumentsByRequestQueryHandler : IRequestHandler<GetDocumentsByRequestQuery, Result<List<DocumentDto>>>
{
    private readonly IIssuedDocumentRepository _documentRepo;

    public GetDocumentsByRequestQueryHandler(IIssuedDocumentRepository documentRepo)
    {
        _documentRepo = documentRepo;
    }

    public async Task<Result<List<DocumentDto>>> Handle(GetDocumentsByRequestQuery request, CancellationToken cancellationToken)
    {
        var items = await _documentRepo.GetByRequestIdAsync(request.RequestId, cancellationToken);
        var dtos = items.Select(GetDocumentByIdQueryHandler.MapToDto).ToList();
        return Result.Success(dtos);
    }
}
