using US.Domain.Entities;
using US.Domain.Enums;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Application.Checks;

public interface IMonitorChecker
{
    MonitorType Type { get; }

    Task<Check> CheckAsync(Monitor monitor);
}