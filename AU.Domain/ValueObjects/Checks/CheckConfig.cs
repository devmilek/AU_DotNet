using System.Text.Json.Serialization;

namespace AU.Domain.ValueObjects.Checks;

/// <summary>
/// Ustawienia checka specyficzne dla typu monitora (zapisywane jako jsonb).
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(HttpCheckConfig), "http")]
public abstract class CheckConfig
{
}
