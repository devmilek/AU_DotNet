using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using US.Application.Organizations;

namespace US.Infrastructure.Identity;

public class InvitationTokenService : IInvitationTokenService
{
    public (string Token, string Hash) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = WebEncoders.Base64UrlEncode(bytes);
        return (token, Hash(token));
    }

    public string Hash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}