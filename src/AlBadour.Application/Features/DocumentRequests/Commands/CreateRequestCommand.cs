using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.DocumentRequests.DTOs;
using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.DocumentRequests.Commands;

public record CreateRequestCommand(CreateRequestDto Dto) : IRequest<Result<Guid>>;

public class CreateRequestCommandHandler : IRequestHandler<CreateRequestCommand, Result<Guid>>
{
    private readonly IDocumentRequestRepository _requestRepo;
    private readonly IDocumentTypeRepository _typeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public CreateRequestCommandHandler(
        IDocumentRequestRepository requestRepo,
        IDocumentTypeRepository typeRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _requestRepo = requestRepo;
        _typeRepo = typeRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Inquiry)
            return Result.Failure<Guid>("Only Inquiry department staff can create requests.", "FORBIDDEN");

        var docType = await _typeRepo.GetByIdAsync(request.Dto.DocumentTypeId, cancellationToken);
        if (docType is null || !docType.IsActive)
            return Result.Failure<Guid>("Invalid or inactive document type.", "INVALID_DOCUMENT_TYPE");

        var entity = new DocumentRequest
        {
            Id = Guid.NewGuid(),
            PatientName = request.Dto.PatientName,
            PatientNameEn = request.Dto.PatientNameEn,
            RecipientEntity = request.Dto.RecipientEntity,
            DocumentTypeId = request.Dto.DocumentTypeId,
            Notes = request.Dto.Notes,
            Status = RequestStatus.Pending,
            CreatedById = _currentUser.UserId
        };

        await _requestRepo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("request.created", "request", entity.Id.ToString(),
            new { entity.PatientName, entity.RecipientEntity, DocumentType = docType.NameAr }, cancellationToken);

        await _notificationService.SendToDepartmentAsync(
            Department.Statistics,
            "طلب وثيقة جديد",
            "New Document Request",
            "تم تقديم طلب وثيقة جديد",
            "A new document request has been submitted",
            "request", entity.Id.ToString(), cancellationToken);

        return Result.Success(entity.Id);
    }
}
