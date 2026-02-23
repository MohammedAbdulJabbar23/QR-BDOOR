using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;

namespace AlBadour.Domain.Interfaces;

public interface IDocumentRequestRepository
{
    Task<DocumentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DocumentRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(List<DocumentRequest> Items, int TotalCount)> GetAllAsync(
        RequestStatus? status, Guid? createdById, string? search,
        int page, int pageSize, CancellationToken ct = default);
    Task<List<DocumentRequest>> GetPendingAsync(CancellationToken ct = default);
    Task AddAsync(DocumentRequest request, CancellationToken ct = default);
    void Update(DocumentRequest request);
}
