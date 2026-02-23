namespace AlBadour.Application.Common.Interfaces;

public interface IDocumentNumberService
{
    Task<string> GenerateNextAsync(CancellationToken ct = default);
}
