using AlBadour.Application.Common.Interfaces;
using AlBadour.Domain.Interfaces;
using AlBadour.Infrastructure.Persistence;
using AlBadour.Infrastructure.Persistence.Repositories;
using AlBadour.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlBadour.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

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

        return services;
    }
}
