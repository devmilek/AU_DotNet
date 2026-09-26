using AU.Application.Checks.Statistics;
using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Application.Monitors.Queries.GetAllMonitors;

public sealed record MonitorListItem(Monitor Monitor, IReadOnlyList<HourlyCheckSummary> Last24Hours);
