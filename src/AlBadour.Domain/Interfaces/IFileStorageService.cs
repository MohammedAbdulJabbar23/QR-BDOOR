namespace AlBadour.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SavePdfAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<string> SaveQrCodeAsync(byte[] imageBytes, string fileName, CancellationToken ct = default);
    Task<Stream?> GetFileAsync(string filePath, CancellationToken ct = default);
    Task DeleteFileAsync(string filePath, CancellationToken ct = default);
}
