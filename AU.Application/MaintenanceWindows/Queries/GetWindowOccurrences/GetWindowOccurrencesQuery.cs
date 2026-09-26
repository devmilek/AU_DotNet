using FluentValidation;
using AU.Application.Exceptions;

namespace AU.Application.MaintenanceWindows.Queries.GetWindowOccurrences;

public sealed record GetWindowOccurrencesQuery(Guid OrganizationId, Guid WindowId, DateTimeOffset From, DateTimeOffset To);

public sealed class GetWindowOccurrencesValidator : AbstractValidator<GetWindowOccurrencesQuery>
{
    public static readonly TimeSpan MaxRange = TimeSpan.FromDays(400);

    public GetWindowOccurrencesValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.WindowId).NotEmpty();

        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .WithMessage("Koniec zakresu musi być późniejszy niż początek.")
            .Must((x, to) => to - x.From <= MaxRange)
            .WithMessage($"Zakres nie może przekraczać {MaxRange.TotalDays:0} dni.");
    }
}

public sealed class GetWindowOccurrencesHandler
{
    public async Task<IReadOnlyList<WindowOccurrenceRow>> Handle(
        GetWindowOccurrencesQuery query,
        IMaintenanceWindowRepository windowRepository,
        CancellationToken cancellationToken)
    {
        return await windowRepository.ListWindowOccurrencesAsync(
                   query.OrganizationId, query.WindowId, query.From, query.To, cancellationToken)
               ?? throw new NotFoundException("Okno serwisowe", query.WindowId);
    }
}
