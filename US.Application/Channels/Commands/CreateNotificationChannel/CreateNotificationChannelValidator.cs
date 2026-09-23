using FluentValidation;
using US.Domain.Enums;

namespace US.Application.Channels.Commands.CreateNotificationChannel;

public sealed class CreateNotificationChannelValidator : AbstractValidator<CreateNotificationChannelCommand>
{
    public const int MaxEmailRecipients = 20;

    // na razie tylko email — reszta typów istnieje w domenie, ale nie ma jeszcze wysyłki
    private static readonly ChannelType[] SupportedTypes = [ChannelType.Email];

    public CreateNotificationChannelValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum()
            .Must(type => SupportedTypes.Contains(type))
            .WithMessage(x => $"Typ kanału {x.Type} nie jest jeszcze wspierany.");

        RuleFor(x => x.Email)
            .Null()
            .When(x => x.Type != ChannelType.Email)
            .WithMessage("Ustawienia email są dozwolone tylko dla kanału typu Email.");

        When(x => x.Type == ChannelType.Email, () =>
        {
            RuleFor(x => x.Email)
                .NotNull()
                .WithMessage("Kanał email wymaga listy adresatów.");

            RuleFor(x => x.Email!.To)
                .NotEmpty()
                .WithMessage("Kanał email musi mieć co najmniej jednego adresata.")
                .Must(to => to.Count <= MaxEmailRecipients)
                .WithMessage($"Maksymalnie {MaxEmailRecipients} adresatów.")
                .Must(to => to.Distinct(StringComparer.OrdinalIgnoreCase).Count() == to.Count)
                .WithMessage("Adresy email nie mogą się powtarzać.")
                .When(x => x.Email is not null);

            RuleForEach(x => x.Email!.To)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(320)
                .When(x => x.Email is not null);
        });

        RuleFor(x => x.MonitorIds)
            .Must(ids => ids!.Count <= MonitorAssignment.MaxMonitorsPerRequest)
            .WithMessage($"Maksymalnie {MonitorAssignment.MaxMonitorsPerRequest} monitorów naraz.")
            .When(x => x.MonitorIds is not null);
    }
}
