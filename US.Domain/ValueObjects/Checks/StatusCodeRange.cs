namespace US.Domain.ValueObjects.Checks;

public sealed record StatusCodeRange
{
    public int From { get; }
    public int To { get; }

    public StatusCodeRange(int from, int to)
    {
        if (from is < 100 or > 599 || to is < 100 or > 599)
            throw new ArgumentOutOfRangeException(nameof(from), "Status code musi być w zakresie 100-599.");

        if (from > to)
            throw new ArgumentException("Początek zakresu nie może być większy niż koniec.", nameof(from));

        From = from;
        To = to;
    }

    public bool Contains(int statusCode) => statusCode >= From && statusCode <= To;
}
