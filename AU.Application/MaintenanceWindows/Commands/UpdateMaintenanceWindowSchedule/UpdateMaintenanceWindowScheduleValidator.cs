using FluentValidation;

namespace AU.Application.MaintenanceWindows.Commands.UpdateMaintenanceWindowSchedule;

public sealed class UpdateMaintenanceWindowScheduleValidator : AbstractValidator<UpdateMaintenanceWindowScheduleCommand>
{
    public UpdateMaintenanceWindowScheduleValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.WindowId).NotEmpty();

        RuleFor(x => x.Schedule)
            .NotNull()
            .SetValidator(new MaintenanceScheduleInputValidator());
    }
}
