using Microsoft.AspNetCore.DataProtection;
using AU.Application.Abstractions;
using AU.Domain.ValueObjects;

namespace AU.Infrastructure.Security;

public sealed class DataProtectionSecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    // purpose musi być identyczny w API (szyfruje) i CheckWorkerze (odszyfrowuje)
    private const string Purpose = "AU.MonitorSecrets.v1";

    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public ProtectedSecret Protect(string plaintext) => new(_protector.Protect(plaintext));

    public string Unprotect(ProtectedSecret secret) => _protector.Unprotect(secret.Ciphertext);
}
