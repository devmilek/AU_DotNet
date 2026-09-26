using AU.Application.Channels.Commands.CreateNotificationChannel;
using AU.Domain.Enums;

namespace AU.Api.Controllers.Channels.Requests;

/// <param name="Email">Ustawienia kanału typu Email — wymagane dla Email, zabronione dla innych typów.</param>
/// <param name="MonitorIds">Monitory, do których kanał zostanie od razu przypięty.</param>
public sealed record CreateNotificationChannelRequest(
    string Name,
    ChannelType Type,
    EmailChannelSettings? Email = null,
    IReadOnlyList<Guid>? MonitorIds = null);
