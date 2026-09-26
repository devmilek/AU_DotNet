using Microsoft.Extensions.Options;
using AU.Application.Abstractions;
using AU.Application.Exceptions;
using AU.Application.Notifications.Events.MemberInvited;
using AU.Domain.Entities;

namespace AU.Application.Organizations.Commands.InviteMemberCommand;

public class InviteMemberHandler(
    IOrganizationRepository organizations,
    IOrganizationMemberRepository members,
    IInvitationRepository invitations,
    IUserLookup userLookup,
    IInvitationTokenService tokenService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IOptions<InvitationOptions> options)
{
    public async Task<(InvitationResponse, MemberInvitedEvent)> Handle(
        InviteMemberCommand command, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var email = Invitation.NormalizeEmail(command.Email);

        var inviterRole = await members.GetRoleAsync(command.OrganizationId, currentUser.UserId)
            ?? throw new ForbiddenException("You are not a member of this organization.");

        // brak eskalacji uprawnień
        if (command.Role > inviterRole)
            throw new ForbiddenException("Cannot grant a role higher than your own.");

        // czy ta osoba już jest członkiem?
        var existingUserId = await userLookup.FindIdByEmailAsync(email);
        if (existingUserId is not null &&
            await members.GetRoleAsync(command.OrganizationId, existingUserId.Value) is not null)
            throw new ConflictException("This user is already a member.");

        // rotacja: stare zaproszenie przestaje działać.
        // Zapis osobno, bo filtrowany unikat (OrganizationId, Email) WHERE AcceptedAt IS NULL
        // AND RevokedAt IS NULL pęknie, jeśli EF wyśle INSERT przed UPDATE w jednej partii.
        var pending = await invitations.GetPendingByEmailAsync(command.OrganizationId, email);
        if (pending is not null)
        {
            pending.Revoke(now);
            await unitOfWork.SaveChangesAsync(ct);
        }

        var organization = await organizations.GetAsync(command.OrganizationId)
            ?? throw new NotFoundException("Organizacja", command.OrganizationId);

        var (token, hash) = tokenService.Generate();

        var invitation = Invitation.Create(
            command.OrganizationId, email, command.Role,
            currentUser.UserId, hash, options.Value.Lifetime);

        await invitations.AddAsync(invitation);
        await unitOfWork.SaveChangesAsync(ct);

        var invitedByName = await userLookup.FindDisplayNameByIdAsync(currentUser.UserId);

        var @event = new MemberInvitedEvent(
            Email: email,
            OrganizationName: organization.Name,
            InvitedByName: invitedByName ?? "Administrator",
            Role: command.Role.ToString(),
            Token: token,
            ExpiresAt: invitation.ExpiresAt);

        return (new InvitationResponse(invitation.Id, email, command.Role, invitation.ExpiresAt), @event);
    }
}
