using US.Domain.Enums;
using US.Domain.ValueObjects;

namespace US.Domain.Entities;

public class NotificationChannel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ChannelType Type { get; private set; }
    public ChannelConfig Config { get; private set; } = null!;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid OrganizationId { get; private set; }
    private NotificationChannel() { }

    public static NotificationChannel Create(Guid organizationId, string name, ChannelConfig config)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "OrganizationId nie może być pusty.",
                nameof(organizationId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa kanału nie może być pusta.", nameof(name));

        ArgumentNullException.ThrowIfNull(config);

        var now = DateTimeOffset.UtcNow;
        return new NotificationChannel
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            // typ wynika z configu, więc nie da się utworzyć kanału "Email" z configiem webhooka
            Type = TypeOf(config),
            Config = config,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            OrganizationId = organizationId
        };
    }

    private static ChannelType TypeOf(ChannelConfig config) => config switch
    {
        EmailChannelConfig => ChannelType.Email,
        DiscordChannelConfig => ChannelType.Discord,
        WebhookChannelConfig => ChannelType.Webhook,
        _ => throw new ArgumentException($"Nieobsługiwany config kanału: {config.GetType().Name}.", nameof(config))
    };
}