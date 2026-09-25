using US.Application.Abstractions;
using US.Application.Exceptions;
using US.Domain.Enums;

namespace US.Application.Organizations.Commands.RemoveMember;

public sealed record RemoveMemberCommand(Guid OrganizationId, Guid UserId);

public sealed class RemoveMemberHandler
{
    public async Task Handle(
        RemoveMemberCommand command,
        IOrganizationRepository organizations,
        IOrganizationMemberRepository members,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var actorRole = await OrganizationPermissions.RequireRoleAsync(
            members, command.OrganizationId, currentUser.UserId);

        var organization = await organizations.GetWithMembersAsync(command.OrganizationId, cancellationToken)
                           ?? throw new NotFoundException("Organization", command.OrganizationId);

        var member = organization.FindMember(command.UserId)
                     ?? throw new NotFoundException("Member", command.UserId);

        var leaving = command.UserId == currentUser.UserId;

        if (!leaving)
        {
            if (actorRole < OrganizationRole.Admin)
                throw new ForbiddenException("Only admins can remove members.");

            OrganizationPermissions.EnsureCanManage(actorRole, member.Role);
        }

        if (organization.IsLastOwner(command.UserId))
            throw new ConflictException(leaving
                ? "You’re the last owner. Make someone else an owner before leaving, or delete the organization."
                : "The organization must keep at least one owner.");

        organization.RemoveMember(command.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
