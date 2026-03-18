using AlBadour.Domain.Enums;

namespace AlBadour.Domain.Entities;

public class IssuedDocument : BaseEntity
{
    public string DocumentNumber { get; set; } = string.Empty;
    public Guid RequestId { get; set; }
    public string QrCodeUrl { get; set; } = string.Empty;
    public string? QrCodeImagePath { get; set; }
    public string? PdfFilePath { get; set; }
    public string? Subject { get; set; }
    public string? DocumentBody { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public string? RevocationReason { get; set; }
    public Guid? ReplacementDocumentId { get; set; }
    public DateTime? QrExpiresAt { get; set; }
    public string? PatientGender { get; set; }
    public string? PatientProfession { get; set; }
    public string? PatientAge { get; set; }
    public string? AdmissionDate { get; set; }
    public string? DischargeDate { get; set; }
    public string? LeaveGranted { get; set; }
    public string? AccountStatementPath { get; set; }
    public Guid IssuedById { get; set; }
    public Guid? RevokedById { get; set; }
    public Guid? ApprovedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ArchivedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DocumentRequest Request { get; set; } = null!;
    public IssuedDocument? ReplacementDocument { get; set; }
    public User IssuedBy { get; set; } = null!;
    public User? RevokedBy { get; set; }
    public User? ApprovedBy { get; set; }
}
