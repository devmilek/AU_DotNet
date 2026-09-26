namespace US.Application.Checks.ReadModels;

public sealed class MonitorCheckPhasesHourly
{
    public DateTimeOffset Bucket { get; private set; }
    public Guid MonitorId { get; private set; }

    public long DnsCount { get; private set; }
    public long? DnsSumMs { get; private set; }
    public long ConnectCount { get; private set; }
    public long? ConnectSumMs { get; private set; }
    public long TlsCount { get; private set; }
    public long? TlsSumMs { get; private set; }
    public long TtfbCount { get; private set; }
    public long? TtfbSumMs { get; private set; }
    public long TransferCount { get; private set; }
    public long? TransferSumMs { get; private set; }
}
