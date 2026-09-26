using AU.Domain.Enums;

namespace AU.Domain.Entities;

public class Invitation
{
    private Invitation() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Email { get; private set; } = null!;
    public OrganizationRole Role { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public Guid InvitedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static Invitation Create(Guid organizationId, string email, OrganizationRole role, Guid invitedByUserId, string tokenHash, TimeSpan lifetime)
    {
        var now = DateTimeOffset.UtcNow;

        return new Invitation
        {
            Id = Guid.CreateVersion7(),
            OrganizationId = organizationId,
            Email = NormalizeEmail(email),
            Role = role,
            TokenHash = tokenHash,
            InvitedByUserId = invitedByUserId,
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime)
        };
    }

    public bool IsPending(DateTimeOffset now) => AcceptedAt is null && RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now)
    {
        if (!IsPending(now))
            throw new InvalidOperationException("Only a pending invitation can be revoked.");

        RevokedAt = now;
    }

    public void MarkAccepted(Guid userId, DateTimeOffset now)
    {
        if (!IsPending(now))
            throw new InvalidOperationException("Invitation is no longer valid.");

        AcceptedAt = now;
        AcceptedByUserId = userId;
    }
}