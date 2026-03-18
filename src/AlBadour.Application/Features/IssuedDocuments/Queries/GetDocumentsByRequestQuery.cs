using AlBadour.Application.Common.Models;
using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Security;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Queries;

public record GetDocumentsByRequestQuery(Guid RequestId) : IRequest<Result<List<DocumentDto>>>;

public class GetDocumentsByRequestQueryHandler : IRequestHandler<GetDocumentsByRequestQuery, Result<List<DocumentDto>>>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly ICurrentUserService _currentUser;

    public GetDocumentsByRequestQueryHandler(IIssuedDocumentRepository documentRepo, ICurrentUserService currentUser)
    {
        _documentRepo = documentRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<DocumentDto>>> Handle(GetDocumentsByRequestQuery request, CancellationToken cancellationToken)
    {
        var isAdministrativeLetter = DepartmentVisibility.GetAdministrativeLetterFilter(_currentUser.Department);
        var items = await _documentRepo.GetByRequestIdAsync(request.RequestId, isAdministrativeLetter, cancellationToken);
        var dtos = items.Select(GetDocumentByIdQueryHandler.MapToDto).ToList();
        return Result.Success(dtos);
    }
}
