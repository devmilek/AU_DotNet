using FluentValidation;
using AU.Application.Abstractions;
using AU.Application.Exceptions;
using AU.Domain.Enums;

namespace AU.Application.Organizations.Commands.ChangeMemberRole;

public sealed record ChangeMemberRoleCommand(Guid OrganizationId, Guid UserId, OrganizationRole Role);

public sealed class ChangeMemberRoleValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
    }
}

public sealed class ChangeMemberRoleHandler
{
    public async Task Handle(
        ChangeMemberRoleCommand command,
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

        OrganizationPermissions.EnsureCanManage(actorRole, member.Role);
        OrganizationPermissions.EnsureCanGrant(actorRole, command.Role);

        if (command.Role != OrganizationRole.Owner && organization.IsLastOwner(command.UserId))
            throw new ConflictException("The organization must keep at least one owner. Make someone else an owner first.");

        organization.ChangeMemberRole(command.UserId, command.Role);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
