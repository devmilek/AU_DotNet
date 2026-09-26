using AU.Domain.Enums;

namespace AU.Application.Organizations.Commands.CreateOrganization;

public record OrganizationResponse(Guid Id, string Name, string Slug, OrganizationRole Role, string? LogoUrl);