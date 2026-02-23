using AlBadour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlBadour.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.UserName).HasColumnName("user_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityId).HasColumnName("entity_id").HasMaxLength(50).IsRequired();
        builder.Property(a => a.Details).HasColumnName("details").HasColumnType("jsonb");
        builder.Property(a => a.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasColumnName("user_agent");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.CreatedAt).IsDescending();
    }
}
