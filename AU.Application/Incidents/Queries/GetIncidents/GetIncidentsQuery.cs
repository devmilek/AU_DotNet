using FluentValidation;
using AU.Application.Abstractions;
using AU.Application.Common;

namespace AU.Application.Incidents.Queries.GetIncidents;

public sealed record GetIncidentsQuery(
    Guid OrganizationId,
    IncidentStatusFilter Status = IncidentStatusFilter.All,
    int Page = 1,
    int PageSize = 20,
    Guid? MonitorId = null);

public sealed class GetIncidentsValidator : AbstractValidator<GetIncidentsQuery>
{
    public const int MaxPageSize = 100;

    public GetIncidentsValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}

public sealed class GetIncidentsHandler
{
    public async Task<PagedResult<IncidentResponse>> Handle(
        GetIncidentsQuery query,
        IIncidentReader incidents,
        IUserLookup userLookup,
        CancellationToken cancellationToken)
    {
        var page = await incidents.GetPagedAsync(
            query.OrganizationId, query.Status, query.MonitorId, query.Page, query.PageSize, cancellationToken);

        var items = await IncidentResponse.FromRowsAsync(page.Items, userLookup, cancellationToken);

        return new PagedResult<IncidentResponse>(items, page.Page, page.PageSize, page.TotalCount);
    }
}
