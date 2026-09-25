using FluentValidation;
using US.Application.Channels;

namespace US.Application.MaintenanceWindows.Commands.SetMaintenanceWindowMonitors;

public sealed class SetMaintenanceWindowMonitorsValidator : AbstractValidator<SetMaintenanceWindowMonitorsCommand>
{
    public SetMaintenanceWindowMonitorsValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.WindowId).NotEmpty();

        RuleFor(x => x.MonitorIds)
            .NotNull()
            .Must(ids => ids.Count <= MonitorAssignment.MaxMonitorsPerRequest)
            .WithMessage($"Maksymalnie {MonitorAssignment.MaxMonitorsPerRequest} monitorów naraz.");
    }
}
