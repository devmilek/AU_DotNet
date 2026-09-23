using US.Domain.Enums;

namespace US.Application.Organizations.Commands.CreateOrganization;

public record OrganizationResponse(Guid Id, string Name, string Slug, OrganizationRole Role);