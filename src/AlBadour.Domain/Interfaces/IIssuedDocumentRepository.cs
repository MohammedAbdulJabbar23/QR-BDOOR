using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;

namespace AlBadour.Domain.Interfaces;

public interface IIssuedDocumentRepository
{
    Task<IssuedDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IssuedDocument?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(List<IssuedDocument> Items, int TotalCount)> GetAllAsync(
        DocumentStatus? status, string? search,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default);
    Task<List<IssuedDocument>> GetByRequestIdAsync(Guid requestId, CancellationToken ct = default);
    Task<int> CountAsync(DocumentStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<int> CountArchivedInRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task AddAsync(IssuedDocument document, CancellationToken ct = default);
    void Update(IssuedDocument document);
}
