using FluentValidation;

namespace AU.Application.Monitors.Queries.GetAllMonitors;

public sealed class GetAllMonitorsValidator : AbstractValidator<GetAllMonitorsQuery>
{
    public const int MaxPageSize = 100;

    public GetAllMonitorsValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty();

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize);

        RuleFor(x => x.SortBy)
            .IsInEnum();

        RuleFor(x => x.SortOrder)
            .IsInEnum();

        RuleFor(x => x.Search)
            .MaximumLength(200);
    }
}
