using AU.Application.Abstractions;
using AU.Application.Exceptions;

namespace AU.Application.Incidents.Queries.GetIncident;

public sealed record GetIncidentQuery(Guid OrganizationId, Guid IncidentId);

public sealed class GetIncidentHandler
{
    public async Task<IncidentResponse> Handle(
        GetIncidentQuery query,
        IIncidentReader incidents,
        IUserLookup userLookup,
        CancellationToken cancellationToken)
    {
        var row = await incidents.GetAsync(query.OrganizationId, query.IncidentId, cancellationToken)
                  ?? throw new NotFoundException("Incident", query.IncidentId);

        var responses = await IncidentResponse.FromRowsAsync([row], userLookup, cancellationToken);
        return responses[0];
    }
}
