using US.Application.Abstractions;
using US.Application.Exceptions;
using US.Application.Organizations.Commands.CreateOrganization;

namespace US.Application.Organizations.Commands.AcceptInvitation;

public sealed record AcceptInvitationCommand(string Token);

public sealed class AcceptInvitationHandler
{
    public async Task<OrganizationResponse> Handle(
        AcceptInvitationCommand command,
        IInvitationRepository invitations,
        IOrganizationRepository organizations,
        IInvitationTokenService tokenService,
        IUserLookup userLookup,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var invitation = await invitations.GetByTokenHashAsync(tokenService.Hash(command.Token))
                         ?? throw new NotFoundException("Invitation", Guid.Empty);

        var organization = await organizations.GetWithMembersAsync(invitation.OrganizationId, cancellationToken)
                           ?? throw new NotFoundException("Organization", invitation.OrganizationId);

        var existing = organization.FindMember(currentUser.UserId);
        if (existing is not null)
            return new OrganizationResponse(organization.Id, organization.Name, organization.Slug, existing.Role);

        if (!invitation.IsPending(now))
            throw new ConflictException("This invitation has expired or is no longer valid. Ask for a new one.");

        var email = await userLookup.FindEmailByIdAsync(currentUser.UserId);
        if (email is null || !string.Equals(email.Trim(), invitation.Email, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException($"This invitation was sent to {invitation.Email}. Sign in with that account to accept it.");

        var member = organization.AddMember(currentUser.UserId, invitation.Role);
        invitation.MarkAccepted(currentUser.UserId, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrganizationResponse(organization.Id, organization.Name, organization.Slug, member.Role);
    }
}
