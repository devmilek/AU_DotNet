using System.Security.Claims;
using System.Threading.RateLimiting;

namespace US.Api.RateLimiting;

public static class RateLimitingSetup
{
    public const string AuthPolicy = "auth";
    public const string InvitePolicy = "invite";

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // partycjonowane po użytkowniku: przejęte konto Admina nie rozsyła tysięcy
            // maili z naszej domeny i nie pali jej reputacji
            options.AddPolicy(InvitePolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                  ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
