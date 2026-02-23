using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentTypes.Commands;

public record UpdateDocumentTypeCommand(Guid Id, string NameAr, string NameEn, string? DescriptionAr, string? DescriptionEn, bool IsActive) : IRequest<Result>;

public class UpdateDocumentTypeCommandHandler : IRequestHandler<UpdateDocumentTypeCommand, Result>
{
    private readonly IDocumentTypeRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public UpdateDocumentTypeCommandHandler(IDocumentTypeRepository repo, IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuditService auditService)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(UpdateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin)
            return Result.Failure("Only admins can update document types.", "FORBIDDEN");

        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Document type not found.", "NOT_FOUND");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.DescriptionAr = request.DescriptionAr;
        entity.DescriptionEn = request.DescriptionEn;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("documenttype.updated", "documenttype", entity.Id.ToString(), null, cancellationToken);

        return Result.Success();
    }
}
