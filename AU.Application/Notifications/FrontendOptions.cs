namespace AU.Application.Notifications;

/// <summary>
/// Sekcja "Auth" — potrzebna też w workerze, który buduje linki do frontendu.
/// </summary>
public sealed class FrontendOptions
{
    public const string SectionName = "Auth";

    public string FrontendUrl { get; set; } = null!;
}
