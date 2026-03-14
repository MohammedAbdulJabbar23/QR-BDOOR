using System.Reflection;
using AlBadour.Application.Common.Interfaces;
using AlBadour.Domain.Interfaces;
using AlBadour.Infrastructure.Persistence;
using AlBadour.Infrastructure.Persistence.Repositories;
using AlBadour.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace AlBadour.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // QuestPDF
        Settings.License = LicenseType.Community;

        var assembly = Assembly.GetExecutingAssembly();
        using (var fontStream = assembly.GetManifestResourceStream(
            "AlBadour.Infrastructure.Resources.Fonts.NotoSansArabic-Regular.ttf"))
        {
            if (fontStream is not null) FontManager.RegisterFont(fontStream);
        }
        using (var fontStream = assembly.GetManifestResourceStream(
            "AlBadour.Infrastructure.Resources.Fonts.NotoSansArabic-Bold.ttf"))
        {
            if (fontStream is not null) FontManager.RegisterFont(fontStream);
        }

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // MinIO
        services.AddSingleton<IMinioClient>(_ =>
            new MinioClient()
                .WithEndpoint(configuration["Minio:Endpoint"] ?? "localhost:9000")
                .WithCredentials(
                    configuration["Minio:AccessKey"] ?? "minioadmin",
                    configuration["Minio:SecretKey"] ?? "minioadmin")
                .WithSSL(configuration.GetValue<bool>("Minio:UseSSL"))
                .Build());

        // Repositories
        services.AddScoped<IDocumentRequestRepository, DocumentRequestRepository>();
        services.AddScoped<IIssuedDocumentRepository, IssuedDocumentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDocumentNumberService, DocumentNumberService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportGenerationService, ReportGenerationService>();
        services.AddSingleton<IDocumentGenerationService, PdfGenerationService>();

        return services;
    }
}
