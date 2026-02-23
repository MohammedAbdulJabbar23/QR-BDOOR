using AlBadour.Domain.Entities;
using AlBadour.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlBadour.Infrastructure.Persistence.Repositories;

public class DocumentTypeRepository : IDocumentTypeRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentType?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DocumentTypes.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<List<DocumentType>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.DocumentTypes.Where(t => t.IsActive).OrderBy(t => t.NameAr).ToListAsync(ct);
    }

    public async Task<(List<DocumentType> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var totalCount = await _context.DocumentTypes.CountAsync(ct);
        var items = await _context.DocumentTypes
            .OrderBy(t => t.NameAr)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(DocumentType type, CancellationToken ct = default)
    {
        await _context.DocumentTypes.AddAsync(type, ct);
    }

    public void Update(DocumentType type)
    {
        _context.DocumentTypes.Update(type);
    }
}
