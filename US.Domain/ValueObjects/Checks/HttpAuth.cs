using System.Text.Json.Serialization;

namespace US.Domain.ValueObjects.Checks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BasicHttpAuth), "basic")]
[JsonDerivedType(typeof(BearerHttpAuth), "bearer")]
public abstract class HttpAuth
{
}

public sealed class BasicHttpAuth : HttpAuth
{
    public required string Username { get; init; }
    public required ProtectedSecret Password { get; init; }
}

public sealed class BearerHttpAuth : HttpAuth
{
    public required ProtectedSecret Token { get; init; }
}
