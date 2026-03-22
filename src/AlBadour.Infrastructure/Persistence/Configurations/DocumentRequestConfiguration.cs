using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlBadour.Infrastructure.Persistence.Configurations;

public class DocumentRequestConfiguration : IEntityTypeConfiguration<DocumentRequest>
{
    public void Configure(EntityTypeBuilder<DocumentRequest> builder)
    {
        builder.ToTable("document_requests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.PatientName).HasColumnName("patient_name").HasMaxLength(255).IsRequired();
        builder.Property(r => r.PatientNameEn).HasColumnName("patient_name_en").HasMaxLength(255);
        builder.Property(r => r.RecipientEntity).HasColumnName("recipient_entity").HasMaxLength(255).IsRequired();
        builder.Property(r => r.DocumentTypeId).HasColumnName("document_type_id");
        builder.Property(r => r.Notes).HasColumnName("notes");
        builder.Property(r => r.Language).HasColumnName("language").HasMaxLength(20);
        builder.Property(r => r.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue(RequestStatus.Pending)
            .HasConversion(v => v.ToString().ToLower(), v => Enum.Parse<RequestStatus>(v, true));
        builder.Property(r => r.RejectionReason).HasColumnName("rejection_reason");
        builder.Property(r => r.CreatedById).HasColumnName("created_by");
        builder.Property(r => r.AssignedToId).HasColumnName("assigned_to");
        builder.Property(r => r.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

        builder.HasOne(r => r.DocumentType).WithMany(t => t.Requests).HasForeignKey(r => r.DocumentTypeId);
        builder.HasOne(r => r.CreatedBy).WithMany(u => u.CreatedRequests).HasForeignKey(r => r.CreatedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.AssignedTo).WithMany(u => u.AssignedRequests).HasForeignKey(r => r.AssignedToId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Status).HasFilter("is_deleted = FALSE");
        builder.HasIndex(r => r.CreatedById);
        builder.HasIndex(r => r.AssignedToId);
        builder.HasIndex(r => r.CreatedAt).IsDescending();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
