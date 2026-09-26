using AU.Domain.Entities;
using AU.Domain.Enums;

namespace AU.Application.Channels;

public sealed record ChannelListRow(NotificationChannel Channel, int MonitorCount);

public sealed record MonitorSummary(Guid Id, string Name, MonitorType Type, string Target, bool IsActive);
