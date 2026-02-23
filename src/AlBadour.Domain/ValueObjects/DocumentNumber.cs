namespace AlBadour.Domain.ValueObjects;

public sealed record DocumentNumber
{
    public string Value { get; }
    public int Year { get; }
    public int Sequence { get; }

    private DocumentNumber(string value, int year, int sequence)
    {
        Value = value;
        Year = year;
        Sequence = sequence;
    }

    public static DocumentNumber Create(int year, int sequence)
    {
        var value = $"BD-{year}-{sequence:D5}";
        return new DocumentNumber(value, year, sequence);
    }

    public static DocumentNumber? Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var parts = value.Split('-');
        if (parts.Length != 3 || parts[0] != "BD") return null;
        if (!int.TryParse(parts[1], out var year)) return null;
        if (!int.TryParse(parts[2], out var sequence)) return null;

        return new DocumentNumber(value, year, sequence);
    }

    public override string ToString() => Value;
}
