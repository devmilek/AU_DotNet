namespace US.Domain.ValueObjects.Checks;

public sealed class HttpCheckConfig : CheckConfig
{
    public static readonly IReadOnlyList<StatusCodeRange> DefaultAcceptedStatusCodes = [new(200, 299)];

    public HttpCheckMethod Method { get; init; } = HttpCheckMethod.Get;
    public bool FollowRedirects { get; init; } = true;
    public IReadOnlyList<StatusCodeRange> AcceptedStatusCodes { get; init; } = DefaultAcceptedStatusCodes;
    public HttpAuth? Auth { get; init; }

    public static HttpCheckConfig Default => new();

    public bool IsAccepted(int statusCode) =>
        AcceptedStatusCodes.Any(range => range.Contains(statusCode));

    public void Validate()
    {
        if (!Enum.IsDefined(Method))
            throw new ArgumentOutOfRangeException(nameof(Method), "Nieznana metoda HTTP.");

        if (AcceptedStatusCodes.Count == 0)
            throw new ArgumentException("Wymagany jest co najmniej jeden akceptowany status code.", nameof(AcceptedStatusCodes));
    }
}
