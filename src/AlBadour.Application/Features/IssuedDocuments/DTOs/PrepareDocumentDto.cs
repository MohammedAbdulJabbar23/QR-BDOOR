namespace AlBadour.Application.Features.IssuedDocuments.DTOs;

public record PrepareDocumentDto(
    Guid RequestId,
    string? DocumentBody
);
