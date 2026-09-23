using System.Text.Json;
using System.Text.Json.Serialization;

namespace US.Domain.ValueObjects;

/// <summary>
/// Zaszyfrowany sekret (hasło, token). Domena nigdy nie trzyma go jawnie —
/// szyfruje i odszyfrowuje warstwa aplikacji przez ISecretProtector.
/// </summary>
[JsonConverter(typeof(ProtectedSecretJsonConverter))]
public sealed record ProtectedSecret
{
    public string Ciphertext { get; }

    public ProtectedSecret(string ciphertext)
    {
        if (string.IsNullOrWhiteSpace(ciphertext))
            throw new ArgumentException("Zaszyfrowany sekret nie może być pusty.", nameof(ciphertext));

        Ciphertext = ciphertext;
    }

    // żeby sekret nie wyciekł przypadkiem do logów
    public override string ToString() => "***";
}

internal sealed class ProtectedSecretJsonConverter : JsonConverter<ProtectedSecret>
{
    public override ProtectedSecret Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, ProtectedSecret value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Ciphertext);
}
