using AU.Domain.Enums;

namespace AU.Application.Channels.Commands.CreateNotificationChannel;

/// <param name="Email">Ustawienia dla kanału typu Email; dla innych typów musi być null.</param>
/// <param name="MonitorIds">Monitory, do których kanał zostanie od razu przypięty (opcjonalnie).</param>
public sealed record CreateNotificationChannelCommand(
    Guid OrganizationId,
    string Name,
    ChannelType Type,
    EmailChannelSettings? Email = null,
    IReadOnlyList<Guid>? MonitorIds = null);

public sealed record EmailChannelSettings(IReadOnlyList<string> To);
