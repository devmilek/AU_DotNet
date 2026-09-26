namespace AU.Application.Organizations.Commands.CreateOrganization;

public record CreateOrganizationCommand(string Name, string? Slug = null);
