namespace AlBadour.Domain.Services;

public static class DocumentNumberGenerator
{
    public static string Format(int year, int sequenceNumber)
    {
        return $"BD-{year}-{sequenceNumber:D5}";
    }
}
