using AlBadour.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AlBadour.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
        Directory.CreateDirectory(Path.Combine(_basePath, "pdfs"));
        Directory.CreateDirectory(Path.Combine(_basePath, "qrcodes"));
    }

    public async Task<string> SavePdfAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_basePath, "pdfs", fileName);
        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream, ct);
        return filePath;
    }

    public async Task<string> SaveQrCodeAsync(byte[] imageBytes, string fileName, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_basePath, "qrcodes", fileName);
        await File.WriteAllBytesAsync(filePath, imageBytes, ct);
        return filePath;
    }

    public Task<Stream?> GetFileAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken ct = default)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }
}
