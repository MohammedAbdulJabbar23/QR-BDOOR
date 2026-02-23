namespace AlBadour.Domain.Entities;

public class DocumentType : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? TemplatePath { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DocumentRequest> Requests { get; set; } = new List<DocumentRequest>();
}
