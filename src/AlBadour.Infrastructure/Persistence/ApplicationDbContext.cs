using AlBadour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlBadour.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<DocumentRequest> DocumentRequests => Set<DocumentRequest>();
    public DbSet<IssuedDocument> IssuedDocuments => Set<IssuedDocument>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DocumentNumberSequence> DocumentNumberSequences => Set<DocumentNumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
