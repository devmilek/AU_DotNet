using System.Diagnostics;

namespace US.Api;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new("US.Api");
}