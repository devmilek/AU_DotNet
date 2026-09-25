using FluentValidation;
using US.Application.Channels;
using US.Domain.Entities;

namespace US.Application.MaintenanceWindows.Commands.CreateMaintenanceWindow;

public sealed class CreateMaintenanceWindowValidator : AbstractValidator<CreateMaintenanceWindowCommand>
{
    public CreateMaintenanceWindowValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(MaintenanceWindow.MaxNameLength);

        RuleFor(x => x.Description)
            .MaximumLength(MaintenanceWindow.MaxDescriptionLength);

        RuleFor(x => x.Schedule)
            .NotNull()
            .SetValidator(new MaintenanceScheduleInputValidator());

        RuleFor(x => x.MonitorIds)
            .NotEmpty()
            .WithMessage("Okno serwisowe musi obejmować co najmniej jeden monitor.")
            .Must(ids => ids.Count <= MonitorAssignment.MaxMonitorsPerRequest)
            .WithMessage($"Maksymalnie {MonitorAssignment.MaxMonitorsPerRequest} monitorów naraz.");
    }
}
