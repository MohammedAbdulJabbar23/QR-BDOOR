using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlBadour.Infrastructure.Persistence.Repositories;

public class DocumentRequestRepository : IDocumentRequestRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DocumentRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<DocumentRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DocumentRequests
            .Include(r => r.DocumentType)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<(List<DocumentRequest> Items, int TotalCount)> GetAllAsync(
        RequestStatus? status, Guid? createdById, string? search,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.DocumentRequests
            .Include(r => r.DocumentType)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (createdById.HasValue)
            query = query.Where(r => r.CreatedById == createdById.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.PatientName.Contains(search) || (r.PatientNameEn != null && r.PatientNameEn.Contains(search)));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<DocumentRequest>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _context.DocumentRequests
            .Include(r => r.DocumentType)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(DocumentRequest request, CancellationToken ct = default)
    {
        await _context.DocumentRequests.AddAsync(request, ct);
    }

    public void Update(DocumentRequest request)
    {
        _context.DocumentRequests.Update(request);
    }
}
