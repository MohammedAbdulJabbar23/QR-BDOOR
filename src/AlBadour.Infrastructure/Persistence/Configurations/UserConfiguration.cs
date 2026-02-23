using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlBadour.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
        builder.Property(u => u.FullNameEn).HasColumnName("full_name_en").HasMaxLength(255);
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(u => u.Role).HasColumnName("role").HasMaxLength(20).IsRequired()
            .HasConversion(v => v.ToString().ToLower(), v => Enum.Parse<UserRole>(v, true));
        builder.Property(u => u.Department).HasColumnName("department").HasMaxLength(20).IsRequired()
            .HasConversion(v => v.ToString().ToLower(), v => Enum.Parse<Department>(v, true));
        builder.Property(u => u.LanguagePreference).HasColumnName("language_preference").HasMaxLength(5).HasDefaultValue("ar");
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(u => u.RefreshToken).HasColumnName("refresh_token").HasMaxLength(500);
        builder.Property(u => u.RefreshTokenExpiry).HasColumnName("refresh_token_expiry");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

        // Seed default admin user (password: Admin@123)
        builder.HasData(new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Username = "admin",
            FullName = "مدير النظام",
            FullNameEn = "System Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            Department = Department.Management,
            LanguagePreference = "ar",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
