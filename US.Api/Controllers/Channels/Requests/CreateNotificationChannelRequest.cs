using US.Domain.Enums;

namespace US.Api.Controllers.Channels.Requests;

public sealed record CreateNotificationChannelRequest(
    string Name,
    ChannelType Type,
    List<string>? EmailTo);
