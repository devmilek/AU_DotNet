using US.Domain.Entities;
using US.Domain.Enums;

namespace US.Application.Channels;

public sealed record ChannelListRow(NotificationChannel Channel, int MonitorCount);

public sealed record MonitorSummary(Guid Id, string Name, MonitorType Type, string Target, bool IsActive);
