namespace US.Application.Checks.ReadModels;

/// <summary>
/// Wiersz agregatu checków jednego monitora w jednym kubełku czasu.
/// Trzymamy sumy i liczniki (nie średnie ani procenty), żeby dało się je poprawnie
/// łączyć w większe okresy — uptime = UpChecks / TotalChecks, średni czas = ResponseTimeSumMs / ResponseTimeCount.
/// </summary>
public abstract class MonitorCheckRollup
{
    public DateTimeOffset Bucket { get; private set; }
    public Guid MonitorId { get; private set; }

    public long TotalChecks { get; private set; }
    public long UpChecks { get; private set; }

    public long ResponseTimeCount { get; private set; }
    public long? ResponseTimeSumMs { get; private set; }
    public int? MinResponseTimeMs { get; private set; }
    public int? MaxResponseTimeMs { get; private set; }

    public double? UptimeRatio => TotalChecks == 0 ? null : (double)UpChecks / TotalChecks;

    public double? AverageResponseTimeMs =>
        ResponseTimeCount == 0 || ResponseTimeSumMs is null ? null : (double)ResponseTimeSumMs.Value / ResponseTimeCount;
}

/// <summary>Agregat godzinowy (continuous aggregate <c>monitor_checks_hourly</c>), retencja 2 lata.</summary>
public sealed class MonitorCheckHourly : MonitorCheckRollup;

/// <summary>Agregat dzienny (continuous aggregate <c>monitor_checks_daily</c>, liczony z godzinowego), bez retencji.</summary>
public sealed class MonitorCheckDaily : MonitorCheckRollup;
