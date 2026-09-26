"use client";

import { EllipsisIcon, LogOutIcon, UserMinusIcon } from "lucide-react";
import { useState } from "react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Frame, FrameHeader, FramePanel } from "@/components/ui/frame";
import { Menu, MenuItem, MenuPopup, MenuTrigger } from "@/components/ui/menu";
import { Skeleton } from "@/components/ui/skeleton";
import type { components } from "@/lib/api/schema";
import { useChangeMemberRole } from "../../hooks/use-organization-settings";
import { formatShortDate, initialsOf } from "../../lib/format";
import { canManage, type OrganizationRole, ROLE } from "../../lib/roles";
import { organizationRoleLabel } from "../../lib/role-label";
import { RemoveMemberDialog } from "./remove-member-dialog";
import { RoleSelect } from "./role-select";

type Member = components["schemas"]["MemberRow"];

export function MembersList({
  organizationId,
  organizationName,
  actorRole,
  members,
}: {
  organizationId: string;
  organizationName: string;
  actorRole: OrganizationRole;
  members: Member[] | undefined;
}) {
  const [removing, setRemoving] = useState<Member | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const ownerCount = members?.filter((m) => m.role === ROLE.Owner).length ?? 0;

  return (
    <section className="flex flex-col gap-3">
      <h2 className="font-heading text-lg">
        Members
        {members ? (
          <span className="ms-2 font-normal text-muted-foreground text-sm tabular-nums">
            {members.length}
          </span>
        ) : null}
      </h2>
      <Frame>
        <FrameHeader className="grid grid-cols-[minmax(0,1fr)_10rem_2rem] gap-4 py-2.5 font-medium text-muted-foreground text-xs max-sm:hidden">
          <span>Member</span>
          <span>Role</span>
          <span className="sr-only">Actions</span>
        </FrameHeader>
        <FramePanel className="p-0">
          <ul className="divide-y" aria-busy={!members || undefined}>
            {members
              ? members.map((member) => (
                  <MemberRow
                    key={member.userId}
                    organizationId={organizationId}
                    actorRole={actorRole}
                    member={member}
                    isLastOwner={member.role === ROLE.Owner && ownerCount === 1}
                    onRemove={() => {
                      setRemoving(member);
                      setDialogOpen(true);
                    }}
                  />
                ))
              : Array.from({ length: 3 }, (_, index) => (
                  <li key={index} className="flex items-center gap-3 px-4 py-3">
                    <Skeleton className="size-8 rounded-full" />
                    <Skeleton className="h-4 w-48" />
                  </li>
                ))}
          </ul>
        </FramePanel>
      </Frame>

      <RemoveMemberDialog
        organizationId={organizationId}
        organizationName={organizationName}
        member={removing}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />
    </section>
  );
}

function MemberRow({
  organizationId,
  actorRole,
  member,
  isLastOwner,
  onRemove,
}: {
  organizationId: string;
  actorRole: OrganizationRole;
  member: Member;
  isLastOwner: boolean;
  onRemove: () => void;
}) {
  const changeRole = useChangeMemberRole(organizationId);
  const name = member.displayName || member.email;
  const manageable = !member.isCurrentUser && canManage(actorRole, member.role);
  const role = changeRole.isPending ? changeRole.variables.role : member.role;

  return (
    <li className="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-x-4 gap-y-2 px-4 py-3 text-sm sm:grid-cols-[minmax(0,1fr)_10rem_2rem]">
      <div className="flex min-w-0 items-center gap-3">
        <Avatar className="size-8">
          <AvatarFallback className="text-xs">{initialsOf(name)}</AvatarFallback>
        </Avatar>
        <div className="flex min-w-0 flex-col">
          <span className="flex items-center gap-2">
            <span className="truncate font-medium">{name}</span>
            {member.isCurrentUser ? (
              <Badge variant="secondary" size="sm">
                You
              </Badge>
            ) : null}
          </span>
          <span className="truncate text-muted-foreground text-xs">
            {member.displayName ? `${member.email} · ` : ""}
            Joined {formatShortDate(new Date(member.joinedAt))}
          </span>
        </div>
      </div>

      <div className="max-sm:order-last max-sm:col-span-2 max-sm:ps-11">
        {manageable ? (
          <RoleSelect
            aria-label={`Role of ${name}`}
            actorRole={actorRole}
            value={role}
            disabled={changeRole.isPending || isLastOwner}
            onValueChange={(next) => {
              if (next !== member.role) {
                changeRole.mutate({ userId: member.userId, role: next });
              }
            }}
          />
        ) : (
          <Badge variant="outline">{organizationRoleLabel(member.role)}</Badge>
        )}
      </div>

      <MemberActions
        member={member}
        manageable={manageable}
        isLastOwner={isLastOwner}
        onRemove={onRemove}
      />
    </li>
  );
}

function MemberActions({
  member,
  manageable,
  isLastOwner,
  onRemove,
}: {
  member: Member;
  manageable: boolean;
  isLastOwner: boolean;
  onRemove: () => void;
}) {
  if (!member.isCurrentUser && !manageable) {
    return <span className="size-8 sm:size-7" />;
  }

  return (
    <Menu>
      <MenuTrigger
        render={
          <Button size="icon-sm" variant="ghost" aria-label="Member actions" />
        }
      >
        <EllipsisIcon />
      </MenuTrigger>
      <MenuPopup align="end">
        {member.isCurrentUser ? (
          <MenuItem
            variant="destructive"
            disabled={isLastOwner}
            onClick={onRemove}
          >
            <LogOutIcon />
            {isLastOwner ? "Last owner can’t leave" : "Leave organization"}
          </MenuItem>
        ) : (
          <MenuItem variant="destructive" onClick={onRemove}>
            <UserMinusIcon />
            Remove from organization
          </MenuItem>
        )}
      </MenuPopup>
    </Menu>
  );
}
