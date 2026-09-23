using US.Domain.Enums;

namespace US.Domain.Entities;

public class OrganizationMember
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public OrganizationRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    public Organization Organization { get; private set; } = null!;

    internal static OrganizationMember Create(Guid organizationId, Guid userId, OrganizationRole role)
        => new()
        {
            OrganizationId = organizationId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow
        };

    public void ChangeRole(OrganizationRole role) => Role = role;
}