using US.Application.Abstractions;
using US.Application.Exceptions;

namespace US.Application.Organizations.Commands.RevokeInvitation;

public sealed record RevokeInvitationCommand(Guid OrganizationId, Guid InvitationId);

public sealed class RevokeInvitationHandler
{
    public async Task Handle(
        RevokeInvitationCommand command,
        IInvitationRepository invitations,
        IOrganizationMemberRepository members,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var actorRole = await OrganizationPermissions.RequireRoleAsync(
            members, command.OrganizationId, currentUser.UserId);

        var invitation = await invitations.GetAsync(command.OrganizationId, command.InvitationId, cancellationToken)
                         ?? throw new NotFoundException("Invitation", command.InvitationId);

        OrganizationPermissions.EnsureCanManage(actorRole, invitation.Role);

        var now = timeProvider.GetUtcNow();
        if (!invitation.IsPending(now))
            throw new ConflictException("This invitation is no longer pending.");

        invitation.Revoke(now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
