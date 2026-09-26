using AU.Domain.Entities;
using AU.Domain.Enums;
using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Application.Checks;

public interface IMonitorChecker
{
    MonitorType Type { get; }

    Task<Check> CheckAsync(Monitor monitor);
}