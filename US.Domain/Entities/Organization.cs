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

    public const int MaxNameLength = 100;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required.", nameof(name));

        if (name.Trim().Length > MaxNameLength)
            throw new ArgumentException($"Organization name can be at most {MaxNameLength} characters.", nameof(name));

        Name = name.Trim();
    }

    public OrganizationMember? FindMember(Guid userId) =>
        _members.FirstOrDefault(m => m.UserId == userId);

    public bool IsLastOwner(Guid userId) =>
        FindMember(userId)?.Role == OrganizationRole.Owner
        && _members.Count(m => m.Role == OrganizationRole.Owner) == 1;

    public void ChangeMemberRole(Guid userId, OrganizationRole role)
    {
        var member = FindMember(userId)
                     ?? throw new InvalidOperationException("User is not a member of this organization.");

        if (role != OrganizationRole.Owner && IsLastOwner(userId))
            throw new InvalidOperationException("The organization must keep at least one owner.");

        member.ChangeRole(role);
    }

    public void RemoveMember(Guid userId)
    {
        var member = FindMember(userId)
                     ?? throw new InvalidOperationException("User is not a member of this organization.");

        if (IsLastOwner(userId))
            throw new InvalidOperationException("The organization must keep at least one owner.");

        _members.Remove(member);
    }
}
