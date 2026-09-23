using Microsoft.AspNetCore.DataProtection;
using US.Application.Abstractions;
using US.Domain.ValueObjects;

namespace US.Infrastructure.Security;

public sealed class DataProtectionSecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    // purpose musi być identyczny w API (szyfruje) i CheckWorkerze (odszyfrowuje)
    private const string Purpose = "US.MonitorSecrets.v1";

    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public ProtectedSecret Protect(string plaintext) => new(_protector.Protect(plaintext));

    public string Unprotect(ProtectedSecret secret) => _protector.Unprotect(secret.Ciphertext);
}
