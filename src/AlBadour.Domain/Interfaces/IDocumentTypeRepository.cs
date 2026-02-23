using AlBadour.Domain.Entities;

namespace AlBadour.Domain.Interfaces;

public interface IDocumentTypeRepository
{
    Task<DocumentType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<DocumentType>> GetAllActiveAsync(CancellationToken ct = default);
    Task<(List<DocumentType> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(DocumentType type, CancellationToken ct = default);
    void Update(DocumentType type);
}
