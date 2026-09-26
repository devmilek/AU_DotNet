using FluentValidation;
using AU.Domain.Entities;

namespace AU.Application.MaintenanceWindows.Commands.UpdateOccurrenceContent;

public sealed class UpdateOccurrenceContentValidator : AbstractValidator<UpdateOccurrenceContentCommand>
{
    public UpdateOccurrenceContentValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.WindowId).NotEmpty();
        RuleFor(x => x.OccurrenceId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(MaintenanceWindow.MaxNameLength);

        RuleFor(x => x.Description)
            .MaximumLength(MaintenanceWindow.MaxDescriptionLength);
    }
}
