using AU.Application.Organizations;
using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Application.Notifications;

/// <summary>Linki do frontendu umieszczane w mailach.</summary>
public static class FrontendLinks
{
    /// <summary>Szczegóły monitora: <c>{frontend}/{orgSlug}/monitors/{id}</c>; bez organizacji — lista organizacji.</summary>
    public static async Task<string> MonitorAsync(
        string frontendUrl,
        Monitor monitor,
        IOrganizationRepository organizationRepository)
    {
        var baseUrl = frontendUrl.TrimEnd('/');
        var organization = await organizationRepository.GetAsync(monitor.OrganizationId);

        return organization is null
            ? $"{baseUrl}/organizations"
            : $"{baseUrl}/{organization.Slug}/monitors/{monitor.Id}";
    }
}
