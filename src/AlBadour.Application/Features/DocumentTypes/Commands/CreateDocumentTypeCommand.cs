using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentTypes.Commands;

public record CreateDocumentTypeCommand(string NameAr, string NameEn, string? DescriptionAr, string? DescriptionEn) : IRequest<Result<Guid>>;

public class CreateDocumentTypeCommandHandler : IRequestHandler<CreateDocumentTypeCommand, Result<Guid>>
{
    private readonly IDocumentTypeRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public CreateDocumentTypeCommandHandler(IDocumentTypeRepository repo, IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuditService auditService)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(CreateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Result.Failure<Guid>("Only admins can create document types.", "FORBIDDEN");

        var entity = new DocumentType
        {
            Id = Guid.NewGuid(),
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn
        };

        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("documenttype.created", "documenttype", entity.Id.ToString(),
            new { entity.NameAr, entity.NameEn }, cancellationToken);

        return Result.Success(entity.Id);
    }
}
