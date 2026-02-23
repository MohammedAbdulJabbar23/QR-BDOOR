using AlBadour.Application.Common.Interfaces;
using AlBadour.Domain.Services;
using AlBadour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlBadour.Infrastructure.Services;

public class DocumentNumberService : IDocumentNumberService
{
    private readonly ApplicationDbContext _context;

    public DocumentNumberService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;

        // Use raw SQL for atomic increment to ensure thread safety
        var result = await _context.Database.SqlQueryRaw<int>(
            @"INSERT INTO document_number_sequences (year, last_number)
              VALUES ({0}, 1)
              ON CONFLICT (year)
              DO UPDATE SET last_number = document_number_sequences.last_number + 1
              RETURNING last_number", year)
            .ToListAsync(ct);

        var sequenceNumber = result.First();
        return DocumentNumberGenerator.Format(year, sequenceNumber);
    }
}
