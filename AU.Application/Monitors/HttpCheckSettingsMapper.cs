using FluentValidation;
using FluentValidation.Results;
using AU.Application.Abstractions;
using AU.Application.Monitors.Commands.CreateMonitor;
using AU.Domain.ValueObjects;
using AU.Domain.ValueObjects.Checks;

namespace AU.Application.Monitors;

public static class HttpCheckSettingsMapper
{
    public static HttpCheckConfig ToConfig(
        HttpCheckSettings settings,
        ISecretProtector secretProtector,
        HttpCheckConfig? current = null) => new()
    {
        Method = settings.Method,
        FollowRedirects = settings.FollowRedirects,
        AcceptedStatusCodes = settings.AcceptedStatusCodes is { Count: > 0 } ranges
            ? ranges.Select(r => new StatusCodeRange(r.From, r.To)).ToList()
            : HttpCheckConfig.DefaultAcceptedStatusCodes,
        Auth = settings.Auth switch
        {
            { Type: HttpAuthType.Basic } basic => new BasicHttpAuth
            {
                Username = basic.Username!,
                Password = ProtectOrKeep(basic.Password, (current?.Auth as BasicHttpAuth)?.Password,
                    "Http.Auth.Password", "Password is required.", secretProtector)
            },
            { Type: HttpAuthType.Bearer } bearer => new BearerHttpAuth
            {
                Token = ProtectOrKeep(bearer.Token, (current?.Auth as BearerHttpAuth)?.Token,
                    "Http.Auth.Token", "Token is required.", secretProtector)
            },
            _ => null
        }
    };

    private static ProtectedSecret ProtectOrKeep(
        string? secret,
        ProtectedSecret? currentProtected,
        string propertyName,
        string message,
        ISecretProtector secretProtector)
    {
        if (!string.IsNullOrEmpty(secret))
            return secretProtector.Protect(secret);

        return currentProtected
               ?? throw new ValidationException([new ValidationFailure(propertyName, message)]);
    }
}
