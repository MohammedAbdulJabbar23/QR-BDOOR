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
        Guid? documentTypeId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, bool? isAdministrativeLetter = null,
        string? requiredDocumentTypeName = null, bool requiresAwaitingAccountStatement = false,
        CancellationToken ct = default)
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

        if (documentTypeId.HasValue)
            query = query.Where(r => r.DocumentTypeId == documentTypeId.Value);

        if (requiredDocumentTypeName != null)
        {
            query = query.Where(r => r.DocumentType.NameEn.Contains(requiredDocumentTypeName));
        }
        else if (isAdministrativeLetter.HasValue)
        {
            query = isAdministrativeLetter.Value
                ? query.Where(r => r.DocumentType.NameEn == "Administrative Letter")
                : query.Where(r => r.DocumentType.NameEn != "Administrative Letter");
        }

        if (requiresAwaitingAccountStatement)
        {
            query = query.Where(r => r.IssuedDocuments.Any(d =>
                d.Status == DocumentStatus.AwaitingAccountStatement && !d.IsDeleted));
        }

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r =>
                r.PatientName.Contains(search) ||
                (r.PatientNameEn != null && r.PatientNameEn.Contains(search)) ||
                r.RecipientEntity.Contains(search) ||
                (r.Notes != null && r.Notes.Contains(search)));

        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.CreatedAt >= from);
        }

        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(r => r.CreatedAt < to);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<DocumentRequest>> GetPendingAsync(Guid? documentTypeId = null, bool? isAdministrativeLetter = null, CancellationToken ct = default)
    {
        var query = _context.DocumentRequests
            .Include(r => r.DocumentType)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .Where(r => r.Status == RequestStatus.Pending)
            .AsQueryable();

        if (documentTypeId.HasValue)
            query = query.Where(r => r.DocumentTypeId == documentTypeId.Value);

        if (isAdministrativeLetter.HasValue)
        {
            query = isAdministrativeLetter.Value
                ? query.Where(r => r.DocumentType.NameEn == "Administrative Letter")
                : query.Where(r => r.DocumentType.NameEn != "Administrative Letter");
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        bool? isAdministrativeLetter = null,
        CancellationToken ct = default)
    {
        var query = _context.DocumentRequests
            .Include(r => r.DocumentType)
            .AsQueryable();

        if (isAdministrativeLetter.HasValue)
        {
            query = isAdministrativeLetter.Value
                ? query.Where(r => r.DocumentType.NameEn == "Administrative Letter")
                : query.Where(r => r.DocumentType.NameEn != "Administrative Letter");
        }

        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.CreatedAt >= from);
        }

        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(r => r.CreatedAt < to);
        }

        return await query
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
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
