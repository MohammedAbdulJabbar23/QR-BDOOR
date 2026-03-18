using AlBadour.Domain.Entities;
using AlBadour.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlBadour.Infrastructure.Persistence.Configurations;

public class IssuedDocumentConfiguration : IEntityTypeConfiguration<IssuedDocument>
{
    public void Configure(EntityTypeBuilder<IssuedDocument> builder)
    {
        builder.ToTable("issued_documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.DocumentNumber).HasColumnName("document_number").HasMaxLength(50).IsRequired();
        builder.HasIndex(d => d.DocumentNumber).IsUnique();
        builder.Property(d => d.RequestId).HasColumnName("request_id");
        builder.Property(d => d.QrCodeUrl).HasColumnName("qr_code_url").HasMaxLength(500).IsRequired();
        builder.HasIndex(d => d.QrCodeUrl).IsUnique();
        builder.Property(d => d.QrCodeImagePath).HasColumnName("qr_code_image_path").HasMaxLength(500);
        builder.Property(d => d.PdfFilePath).HasColumnName("pdf_file_path").HasMaxLength(500);
        builder.Property(d => d.Subject).HasColumnName("subject").HasMaxLength(500);
        builder.Property(d => d.DocumentBody).HasColumnName("document_body");
        builder.Property(d => d.Status).HasColumnName("status").HasMaxLength(30).HasDefaultValue(DocumentStatus.Draft)
            .HasConversion(v => v.ToString().ToLower(), v => Enum.Parse<DocumentStatus>(v, true));
        builder.Property(d => d.RevocationReason).HasColumnName("revocation_reason");
        builder.Property(d => d.ReplacementDocumentId).HasColumnName("replacement_document_id");
        builder.Property(d => d.QrExpiresAt).HasColumnName("qr_expires_at");
        builder.Property(d => d.PatientGender).HasColumnName("patient_gender").HasMaxLength(50);
        builder.Property(d => d.PatientProfession).HasColumnName("patient_profession").HasMaxLength(200);
        builder.Property(d => d.PatientAge).HasColumnName("patient_age").HasMaxLength(50);
        builder.Property(d => d.AdmissionDate).HasColumnName("admission_date").HasMaxLength(50);
        builder.Property(d => d.DischargeDate).HasColumnName("discharge_date").HasMaxLength(50);
        builder.Property(d => d.LeaveGranted).HasColumnName("leave_granted").HasMaxLength(500);
        builder.Property(d => d.AccountStatementPath).HasColumnName("account_statement_path").HasMaxLength(500);
        builder.Property(d => d.IssuedById).HasColumnName("issued_by");
        builder.Property(d => d.RevokedById).HasColumnName("revoked_by");
        builder.Property(d => d.ApprovedById).HasColumnName("approved_by");
        builder.Property(d => d.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(d => d.IssuedAt).HasColumnName("issued_at").HasDefaultValueSql("NOW()");
        builder.Property(d => d.ArchivedAt).HasColumnName("archived_at");
        builder.Property(d => d.RevokedAt).HasColumnName("revoked_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

        builder.HasOne(d => d.Request).WithMany(r => r.IssuedDocuments).HasForeignKey(d => d.RequestId);
        builder.HasOne(d => d.ReplacementDocument).WithOne().HasForeignKey<IssuedDocument>(d => d.ReplacementDocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.IssuedBy).WithMany(u => u.IssuedDocuments).HasForeignKey(d => d.IssuedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.RevokedBy).WithMany().HasForeignKey(d => d.RevokedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.ApprovedBy).WithMany().HasForeignKey(d => d.ApprovedById).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.Status).HasFilter("is_deleted = FALSE");
        builder.HasIndex(d => d.RequestId);
        builder.HasIndex(d => d.IssuedAt).IsDescending();

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
