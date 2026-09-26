using FluentValidation;
using AU.Domain.Entities;

namespace AU.Application.MaintenanceWindows.Commands.RenameMaintenanceWindow;

public sealed class RenameMaintenanceWindowValidator : AbstractValidator<RenameMaintenanceWindowCommand>
{
    public RenameMaintenanceWindowValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.WindowId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(MaintenanceWindow.MaxNameLength);

        RuleFor(x => x.Description)
            .MaximumLength(MaintenanceWindow.MaxDescriptionLength);
    }
}
