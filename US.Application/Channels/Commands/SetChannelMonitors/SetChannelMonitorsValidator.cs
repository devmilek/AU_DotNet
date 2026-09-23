using FluentValidation;

namespace US.Application.Channels.Commands.SetChannelMonitors;

public sealed class SetChannelMonitorsValidator : AbstractValidator<SetChannelMonitorsCommand>
{
    public SetChannelMonitorsValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.ChannelId).NotEmpty();

        RuleFor(x => x.MonitorIds)
            .NotNull()
            .Must(ids => ids.Count <= MonitorAssignment.MaxMonitorsPerRequest)
            .WithMessage($"Maksymalnie {MonitorAssignment.MaxMonitorsPerRequest} monitorów naraz.");
    }
}
