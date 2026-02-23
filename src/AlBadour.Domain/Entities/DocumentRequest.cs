using AlBadour.Domain.Enums;

namespace AlBadour.Domain.Entities;

public class DocumentRequest : BaseEntity
{
    public string PatientName { get; set; } = string.Empty;
    public string? PatientNameEn { get; set; }
    public string RecipientEntity { get; set; } = string.Empty;
    public Guid DocumentTypeId { get; set; }
    public string? Notes { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? RejectionReason { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? AssignedToId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DocumentType DocumentType { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public ICollection<IssuedDocument> IssuedDocuments { get; set; } = new List<IssuedDocument>();
}
