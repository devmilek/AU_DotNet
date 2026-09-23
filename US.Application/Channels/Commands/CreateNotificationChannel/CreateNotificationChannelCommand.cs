using US.Domain.Enums;

namespace US.Application.Channels.Commands.CreateNotificationChannel;

public record CreateNotificationChannelCommand(Guid OrganizationId, string Name, ChannelType Type, List<string>? EmailTo);
