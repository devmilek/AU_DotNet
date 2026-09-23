using US.Domain.ValueObjects;

namespace US.Application.Abstractions;

/// <summary>
/// Szyfruje sekrety monitorów przed zapisem i odszyfrowuje je tuż przed użyciem (np. w checkerze).
/// </summary>
public interface ISecretProtector
{
    ProtectedSecret Protect(string plaintext);
    string Unprotect(ProtectedSecret secret);
}
