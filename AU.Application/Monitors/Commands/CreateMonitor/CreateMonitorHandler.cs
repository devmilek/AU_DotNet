using AU.Application.Abstractions;
using AU.Domain.ValueObjects.Checks;

namespace AU.Application.Monitors.Commands.CreateMonitor;
using Monitor = AU.Domain.Entities.Monitor;

public sealed class CreateMonitorHandler
{
    public async Task<Guid> Handle(
        CreateMonitorCommand command,
        IMonitorRepository repository,
        ISecretProtector secretProtector,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var monitor = Monitor.Create(
            command.OrganizationId,
            command.Name,
            command.Target,
            command.Type,
            command.Http is { } http ? ToConfig(http, secretProtector) : null,
            command.IntervalSeconds,
            command.TimeoutMs,
            command.AlertThreshold,
            command.RecoveryThreshold);

        await repository.AddAsync(monitor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return monitor.Id;
    }

    private static HttpCheckConfig ToConfig(HttpCheckSettings settings, ISecretProtector secretProtector) => new()
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
                Password = secretProtector.Protect(basic.Password!)
            },
            { Type: HttpAuthType.Bearer } bearer => new BearerHttpAuth
            {
                Token = secretProtector.Protect(bearer.Token!)
            },
            _ => null
        }
    };
}
