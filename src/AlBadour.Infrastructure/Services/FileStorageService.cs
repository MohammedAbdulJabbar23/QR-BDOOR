using AlBadour.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace AlBadour.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public FileStorageService(IMinioClient minioClient, IConfiguration configuration)
    {
        _minioClient = minioClient;
        _bucketName = configuration["Minio:BucketName"] ?? "albadour";
    }

    public async Task<string> SavePdfAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var objectKey = $"pdfs/{fileName}";
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType("application/pdf"), ct);
        return objectKey;
    }

    public async Task<string> SaveQrCodeAsync(byte[] imageBytes, string fileName, CancellationToken ct = default)
    {
        var objectKey = $"qrcodes/{fileName}";
        using var stream = new MemoryStream(imageBytes);
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType("image/png"), ct);
        return objectKey;
    }

    public async Task<Stream?> GetFileAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            var memoryStream = new MemoryStream();
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream)), ct);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteFileAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey), ct);
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            // Already deleted — no-op
        }
    }
}
