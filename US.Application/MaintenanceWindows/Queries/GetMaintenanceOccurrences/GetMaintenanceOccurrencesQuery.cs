using FluentValidation;

namespace US.Application.MaintenanceWindows.Queries.GetMaintenanceOccurrences;

public sealed record GetMaintenanceOccurrencesQuery(Guid OrganizationId, DateTimeOffset From, DateTimeOffset To);

public sealed class GetMaintenanceOccurrencesValidator : AbstractValidator<GetMaintenanceOccurrencesQuery>
{
    public static readonly TimeSpan MaxRange = TimeSpan.FromDays(100);

    public GetMaintenanceOccurrencesValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .WithMessage("Koniec zakresu musi być późniejszy niż początek.")
            .Must((x, to) => to - x.From <= MaxRange)
            .WithMessage($"Zakres nie może przekraczać {MaxRange.TotalDays:0} dni.");
    }
}

public sealed class GetMaintenanceOccurrencesHandler
{
    public Task<IReadOnlyList<MaintenanceOccurrenceRow>> Handle(
        GetMaintenanceOccurrencesQuery query,
        IMaintenanceWindowRepository windowRepository,
        CancellationToken cancellationToken)
    {
        return windowRepository.ListOccurrencesAsync(query.OrganizationId, query.From, query.To, cancellationToken);
    }
}
