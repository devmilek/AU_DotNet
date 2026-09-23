using FluentValidation;
using US.Domain.Enums;

namespace US.Application.Channels.Commands.CreateNotificationChannel;

public class CreateNotificationChannelValidator : AbstractValidator<CreateNotificationChannelCommand>
{
    public CreateNotificationChannelValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum();

        When(x => x.Type == ChannelType.Email, () =>
        {
            RuleFor(x => x.EmailTo)
                .NotNull()
                .Must(to => to!.Count > 0)
                .WithMessage("Kanał email musi mieć co najmniej jednego adresata.");

            RuleForEach(x => x.EmailTo)
                .EmailAddress()
                .When(x => x.EmailTo is not null);
        });
    }
}