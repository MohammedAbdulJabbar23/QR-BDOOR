using AlBadour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlBadour.Infrastructure.Persistence.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("document_types");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.NameAr).HasColumnName("name_ar").HasMaxLength(255).IsRequired();
        builder.Property(t => t.NameEn).HasColumnName("name_en").HasMaxLength(255).IsRequired();
        builder.Property(t => t.TemplatePath).HasColumnName("template_path").HasMaxLength(500);
        builder.Property(t => t.DescriptionAr).HasColumnName("description_ar");
        builder.Property(t => t.DescriptionEn).HasColumnName("description_en");
        builder.Property(t => t.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

        builder.HasData(
            new DocumentType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                NameAr = "تقرير طبي مع جدول",
                NameEn = "Medical Report with Table",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new DocumentType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                NameAr = "كتاب إداري",
                NameEn = "Administrative Letter",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new DocumentType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                NameAr = "تقرير طبي مع جدول + كشف حساب",
                NameEn = "Medical Report with Table + Account Statement",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new DocumentType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                NameAr = "تقرير طبي بدون جدول",
                NameEn = "Medical Report without Table",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new DocumentType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000014"),
                NameAr = "تقرير طبي بدون جدول + كشف حساب",
                NameEn = "Medical Report without Table + Account Statement",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new DocumentType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000015"),
                NameAr = "كتاب ضمان وزارة الداخلية",
                NameEn = "MOI Insurance Letter",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
