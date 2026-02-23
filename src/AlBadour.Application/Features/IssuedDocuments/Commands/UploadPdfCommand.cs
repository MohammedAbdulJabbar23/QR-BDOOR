using AlBadour.Application.Common.Interfaces;
using AlBadour.Application.Common.Models;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Commands;

public record UploadPdfCommand(Guid DocumentId, Stream FileStream, string FileName) : IRequest<Result>;

public class UploadPdfCommandHandler : IRequestHandler<UploadPdfCommand, Result>
{
    private readonly IIssuedDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorage;

    public UploadPdfCommandHandler(
        IIssuedDocumentRepository documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFileStorageService fileStorage)
    {
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _auditService = auditService;
        _fileStorage = fileStorage;
    }

    public async Task<Result> Handle(UploadPdfCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Department != Department.Statistics)
            return Result.Failure("Only Statistics department staff can upload PDFs.", "FORBIDDEN");

        var document = await _documentRepo.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null || document.IsDeleted)
            return Result.Failure("Document not found.", "NOT_FOUND");

        if (document.Status != DocumentStatus.Draft)
            return Result.Failure("PDF can only be uploaded for draft documents.", "INVALID_STATUS");

        if (!request.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return Result.Failure("Only PDF files are allowed.", "INVALID_FILE_TYPE");

        var pdfPath = await _fileStorage.SavePdfAsync(request.FileStream, $"{document.Id}.pdf", cancellationToken);

        document.PdfFilePath = pdfPath;
        document.Status = DocumentStatus.Archived;
        document.ArchivedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepo.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("document.archived", "document", document.Id.ToString(),
            new { document.DocumentNumber }, cancellationToken);

        return Result.Success();
    }
}
