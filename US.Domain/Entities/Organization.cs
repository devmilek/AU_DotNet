using US.Domain.Enums;

namespace US.Domain.Entities;

public class Organization
{
    private readonly List<OrganizationMember> _members = [];

    private Organization() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    
    public IReadOnlyCollection<OrganizationMember> Members => _members.AsReadOnly();

    public static Organization Create(string name, string slug, Guid ownerUserId)
    {
        var organization = new Organization
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Slug = slug,
            CreatedAt = DateTimeOffset.UtcNow
        };

        organization._members.Add(
            OrganizationMember.Create(organization.Id, ownerUserId, OrganizationRole.Owner));

        return organization;
    }

    public OrganizationMember AddMember(Guid userId, OrganizationRole role)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a member of this organization.");

        var member = OrganizationMember.Create(Id, userId, role);
        _members.Add(member);
        return member;
    }
}