using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using AlBadour.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlBadour.Infrastructure.Persistence.Repositories;

public class IssuedDocumentRepository : IIssuedDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public IssuedDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IssuedDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IssuedDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<IssuedDocument?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IssuedDocuments
            .Include(d => d.Request).ThenInclude(r => r.DocumentType)
            .Include(d => d.Request).ThenInclude(r => r.CreatedBy)
            .Include(d => d.IssuedBy)
            .Include(d => d.RevokedBy)
            .Include(d => d.ReplacementDocument)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<bool> ExistsByDocumentNumberAsync(string documentNumber, CancellationToken ct = default)
    {
        return await _context.IssuedDocuments.AnyAsync(d => d.DocumentNumber == documentNumber && !d.IsDeleted && d.Status != DocumentStatus.Revoked, ct);
    }

    public async Task<(List<IssuedDocument> Items, int TotalCount)> GetAllAsync(
        DocumentStatus? status, string? search, Guid? documentTypeId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, bool? isAdministrativeLetter = null,
        string? requiredDocumentTypeName = null, string? excludedDocumentTypeName = null,
        CancellationToken ct = default)
    {
        var query = _context.IssuedDocuments
            .Include(d => d.Request).ThenInclude(r => r.DocumentType)
            .Include(d => d.Request).ThenInclude(r => r.CreatedBy)
            .Include(d => d.IssuedBy)
            .Include(d => d.RevokedBy)
            .Include(d => d.ReplacementDocument)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (documentTypeId.HasValue)
            query = query.Where(d => d.Request.DocumentTypeId == documentTypeId.Value);

        if (requiredDocumentTypeName != null)
        {
            query = query.Where(d => d.Request.DocumentType.NameEn.Contains(requiredDocumentTypeName));
        }
        else if (isAdministrativeLetter.HasValue)
        {
            query = isAdministrativeLetter.Value
                ? query.Where(d => d.Request.DocumentType.NameEn == "Administrative Letter")
                : query.Where(d => d.Request.DocumentType.NameEn != "Administrative Letter");
        }

        if (excludedDocumentTypeName != null)
            query = query.Where(d => d.Request.DocumentType.NameEn != excludedDocumentTypeName);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.DocumentNumber.Contains(search) ||
                d.Request.PatientName.Contains(search) ||
                (d.Request.PatientNameEn != null && d.Request.PatientNameEn.Contains(search)) ||
                d.Request.RecipientEntity.Contains(search));

        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(d => d.IssuedAt >= from);
        }

        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(d => d.IssuedAt < to);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(d => d.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<IssuedDocument>> GetByRequestIdAsync(Guid requestId, bool? isAdministrativeLetter = null, CancellationToken ct = default)
    {
        var query = _context.IssuedDocuments
            .Include(d => d.Request).ThenInclude(r => r.DocumentType)
            .Include(d => d.Request).ThenInclude(r => r.CreatedBy)
            .Include(d => d.IssuedBy)
            .Include(d => d.RevokedBy)
            .Include(d => d.ReplacementDocument)
            .Where(d => d.RequestId == requestId)
            .AsQueryable();

        if (isAdministrativeLetter.HasValue)
        {
            query = isAdministrativeLetter.Value
                ? query.Where(d => d.Request.DocumentType.NameEn == "Administrative Letter")
                : query.Where(d => d.Request.DocumentType.NameEn != "Administrative Letter");
        }

        return await query
            .OrderByDescending(d => d.IssuedAt)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        DocumentStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        bool? isAdministrativeLetter = null,
        string? requiredDocumentTypeName = null,
        string? excludedDocumentTypeName = null,
        CancellationToken ct = default)
    {
        var query = _context.IssuedDocuments
            .Include(d => d.Request).ThenInclude(r => r.DocumentType)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (requiredDocumentTypeName != null)
        {
            query = query.Where(d => d.Request.DocumentType.NameEn.Contains(requiredDocumentTypeName));
        }
        else if (isAdministrativeLetter.HasValue)
        {
            query = isAdministrativeLetter.Value
                ? query.Where(d => d.Request.DocumentType.NameEn == "Administrative Letter")
                : query.Where(d => d.Request.DocumentType.NameEn != "Administrative Letter");
        }

        if (excludedDocumentTypeName != null)
            query = query.Where(d => d.Request.DocumentType.NameEn != excludedDocumentTypeName);

        if (fromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(d => d.IssuedAt >= from);
        }

        if (toDate.HasValue)
        {
            var to = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(d => d.IssuedAt < to);
        }

        return await query.CountAsync(ct);
    }

    public async Task<int> CountArchivedInRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        bool? isAdministrativeLetter = null,
        string? requiredDocumentTypeName = null,
        string? excludedDocumentTypeName = null,
        CancellationToken ct = default)
    {
        var from = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(toDate.Date.AddDays(1), DateTimeKind.Utc);

        var query = _context.IssuedDocuments
            .Include(d => d.Request).ThenInclude(r => r.DocumentType)
            .Where(d => d.ArchivedAt.HasValue && d.ArchivedAt.Value >= from && d.ArchivedAt.Value < to)
            .AsQueryable();

        if (requiredDocumentTypeName != null)
        {
            query = query.Where(d => d.Request.DocumentType.NameEn.Contains(requiredDocumentTypeName));
        }
        else if (isAdministrativeLetter.HasValue)
        {
            query = isAdministrativeLetter.Value
                ? query.Where(d => d.Request.DocumentType.NameEn == "Administrative Letter")
                : query.Where(d => d.Request.DocumentType.NameEn != "Administrative Letter");
        }

        if (excludedDocumentTypeName != null)
            query = query.Where(d => d.Request.DocumentType.NameEn != excludedDocumentTypeName);

        return await query.CountAsync(ct);
    }

    public async Task AddAsync(IssuedDocument document, CancellationToken ct = default)
    {
        await _context.IssuedDocuments.AddAsync(document, ct);
    }

    public void Update(IssuedDocument document)
    {
        _context.IssuedDocuments.Update(document);
    }
}
