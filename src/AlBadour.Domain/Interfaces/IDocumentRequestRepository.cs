using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;

namespace AlBadour.Domain.Interfaces;

public interface IDocumentRequestRepository
{
    Task<DocumentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DocumentRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(List<DocumentRequest> Items, int TotalCount)> GetAllAsync(
        RequestStatus? status, Guid? createdById, string? search,
        Guid? documentTypeId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, bool? isAdministrativeLetter = null,
        string? requiredDocumentTypeName = null, bool requiresAwaitingAccountStatement = false,
        string? excludedDocumentTypeName = null,
        CancellationToken ct = default);
    Task<List<DocumentRequest>> GetPendingAsync(Guid? documentTypeId = null, bool? isAdministrativeLetter = null,
        string? requiredDocumentTypeName = null, string? excludedDocumentTypeName = null,
        CancellationToken ct = default);
    Task<Dictionary<string, int>> GetStatusCountsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        bool? isAdministrativeLetter = null,
        string? requiredDocumentTypeName = null,
        string? excludedDocumentTypeName = null,
        CancellationToken ct = default);
    Task AddAsync(DocumentRequest request, CancellationToken ct = default);
    void Update(DocumentRequest request);
}
