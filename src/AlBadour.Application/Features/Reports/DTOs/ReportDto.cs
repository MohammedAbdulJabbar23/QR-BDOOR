namespace AlBadour.Application.Features.Reports.DTOs;

public record DailyReportDto(
    DateTime Date,
    int TotalRequests,
    int PendingRequests,
    int CompletedRequests,
    int RejectedRequests,
    int DocumentsIssued,
    int DocumentsArchived
);

public record StatusBreakdownDto(
    string Status,
    int Count
);

public record CancelledDocumentDto(
    string DocumentNumber,
    string PatientName,
    string? RevocationReason,
    DateTime? RevokedAt,
    string? ReplacementDocumentNumber
);
