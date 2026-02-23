using AlBadour.Application.Common.Models;
using AlBadour.Application.Features.IssuedDocuments.DTOs;
using AlBadour.Domain.Interfaces;
using MediatR;

namespace AlBadour.Application.Features.IssuedDocuments.Queries;

public record GetDocumentByIdQuery(Guid Id) : IRequest<Result<DocumentDto>>;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>
{
    private readonly IIssuedDocumentRepository _documentRepo;

    public GetDocumentByIdQueryHandler(IIssuedDocumentRepository documentRepo)
    {
        _documentRepo = documentRepo;
    }

    public async Task<Result<DocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var doc = await _documentRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (doc is null || doc.IsDeleted)
            return Result.Failure<DocumentDto>("Document not found.", "NOT_FOUND");

        return Result.Success(MapToDto(doc));
    }

    internal static DocumentDto MapToDto(Domain.Entities.IssuedDocument doc)
    {
        return new DocumentDto(
            doc.Id,
            doc.DocumentNumber,
            doc.RequestId,
            doc.Request.PatientName,
            doc.Request.PatientNameEn,
            doc.Request.RecipientEntity,
            doc.Request.DocumentType.NameAr,
            doc.Request.DocumentType.NameEn,
            doc.QrCodeUrl,
            doc.QrCodeImagePath,
            !string.IsNullOrEmpty(doc.PdfFilePath),
            doc.DocumentBody,
            doc.Status.ToString(),
            doc.RevocationReason,
            doc.ReplacementDocumentId,
            doc.ReplacementDocument?.DocumentNumber,
            doc.QrExpiresAt,
            doc.IssuedById,
            doc.IssuedBy.FullName,
            doc.RevokedById,
            doc.RevokedBy?.FullName,
            doc.IssuedAt,
            doc.ArchivedAt,
            doc.RevokedAt
        );
    }
}
