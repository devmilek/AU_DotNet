using System.Text.Json;

namespace AU.Infrastructure.Persistence;

/// <summary>Opcje serializacji dla polimorficznych kolumn jsonb (config monitorów i kanałów).</summary>
internal static class JsonColumnOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // jsonb sortuje klucze, więc "$type" nie musi być pierwszy
        AllowOutOfOrderMetadataProperties = true
    };
}
