using System.Diagnostics;

namespace AU.Api;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new("AU.Api");
}