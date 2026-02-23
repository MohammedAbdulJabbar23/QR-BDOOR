namespace AlBadour.Domain.ValueObjects;

public sealed record QrCodeData
{
    public Guid DocumentId { get; }
    public string VerificationUrl { get; }

    private QrCodeData(Guid documentId, string verificationUrl)
    {
        DocumentId = documentId;
        VerificationUrl = verificationUrl;
    }

    public static QrCodeData Create(Guid documentId, string baseUrl)
    {
        var url = $"{baseUrl.TrimEnd('/')}/verify/{documentId}";
        return new QrCodeData(documentId, url);
    }
}
