using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Commands;

public record UpdateRequestCommand(Guid Id, UpdateRequestDto Dto) : IRequest<Result>;

public class UpdateRequestCommandHandler : IRequestHandler<UpdateRequestCommand, Result>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IDocumentTypeRepository _typeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public UpdateRequestCommandHandler(
        IDocumentRequestRepository requestRepo,
        IDocumentTypeRepository typeRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService)
    {
        _requestRepo = requestRepo;
        _typeRepo = typeRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(UpdateRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _requestRepo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return Result.Failure("Request not found.", "NOT_FOUND");

        var isCreator = entity.CreatedById == _currentUser.UserId;
        var isHrOrStatistics = _currentUser.Department == Department.HR
            || _currentUser.Department == Department.Statistics;

        if (!isCreator && !isHrOrStatistics)
            return Result.Failure("You do not have permission to edit this request.", "FORBIDDEN");

        if (entity.Status == RequestStatus.Completed || entity.Status == RequestStatus.Rejected)
            return Result.Failure("Request cannot be edited after completion or rejection.", "INVALID_STATUS");

        var docType = await _typeRepo.GetByIdAsync(request.Dto.DocumentTypeId, cancellationToken);
        if (docType is null || !docType.IsActive)
            return Result.Failure("Invalid or inactive document type.", "INVALID_DOCUMENT_TYPE");

        var before = new { entity.PatientName, entity.RecipientEntity, entity.DocumentTypeId, entity.Notes };

        entity.PatientName = request.Dto.PatientName;
        entity.PatientNameEn = request.Dto.PatientNameEn;
        entity.RecipientEntity = request.Dto.RecipientEntity;
        entity.DocumentTypeId = request.Dto.DocumentTypeId;
        entity.Notes = request.Dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("request.updated", "request", entity.Id.ToString(),
            new { before, after = new { entity.PatientName, entity.RecipientEntity, entity.DocumentTypeId, entity.Notes } },
            cancellationToken);

        return Result.Success();
    }
}
