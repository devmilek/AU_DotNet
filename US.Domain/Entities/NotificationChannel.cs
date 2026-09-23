using US.Domain.Enums;
using US.Domain.ValueObjects;

namespace US.Domain.Entities;

public class NotificationChannel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ChannelType Type { get; private set; }
    public ChannelConfig Config { get; private set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid OrganizationId { get; private set; }
    private NotificationChannel() { }

    public static NotificationChannel CreateEmail(Guid organizationId, string name, EmailChannelConfig config)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "OrganizationId nie może być pusty.",
                nameof(organizationId));
        
        var now = DateTimeOffset.UtcNow;
        return new NotificationChannel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = ChannelType.Email,
            Config = config,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            OrganizationId = organizationId
        };
    }
}