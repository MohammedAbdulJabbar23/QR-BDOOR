using AlBadour.Domain.Enums;

namespace AlBadour.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? FullNameEn { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Department Department { get; set; }
    public string LanguagePreference { get; set; } = "ar";
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public ICollection<DocumentRequest> CreatedRequests { get; set; } = new List<DocumentRequest>();
    public ICollection<DocumentRequest> AssignedRequests { get; set; } = new List<DocumentRequest>();
    public ICollection<IssuedDocument> IssuedDocuments { get; set; } = new List<IssuedDocument>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
