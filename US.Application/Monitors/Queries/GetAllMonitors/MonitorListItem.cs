using US.Application.Checks.Statistics;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Application.Monitors.Queries.GetAllMonitors;

public sealed record MonitorListItem(Monitor Monitor, IReadOnlyList<HourlyCheckSummary> Last24Hours);
