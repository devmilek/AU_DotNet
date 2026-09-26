"use client";

import { EllipsisIcon, MailIcon, RotateCwIcon, XIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Frame, FramePanel } from "@/components/ui/frame";
import { Menu, MenuItem, MenuPopup, MenuTrigger } from "@/components/ui/menu";
import { toastManager } from "@/components/ui/toast";
import { useNow } from "@/hooks/use-now";
import type { components } from "@/lib/api/schema";
import {
  useInviteMember,
  useRevokeInvitation,
} from "../../hooks/use-organization-settings";
import { formatRelativeDays } from "../../lib/format";
import { canManage, type OrganizationRole } from "../../lib/roles";
import { organizationRoleLabel } from "../../lib/role-label";

type Invitation = components["schemas"]["PendingInvitationRow"];

export function PendingInvitations({
  organizationId,
  actorRole,
  invitations,
}: {
  organizationId: string;
  actorRole: OrganizationRole;
  invitations: Invitation[];
}) {
  const now = useNow(60_000);

  if (invitations.length === 0) return null;

  return (
    <section className="flex flex-col gap-3">
      <h2 className="font-heading text-lg">
        Pending invitations
        <span className="ms-2 font-normal text-muted-foreground text-sm tabular-nums">
          {invitations.length}
        </span>
      </h2>
      <Frame>
        <FramePanel className="p-0">
          <ul className="divide-y">
            {invitations.map((invitation) => (
              <InvitationRow
                key={invitation.id}
                organizationId={organizationId}
                actorRole={actorRole}
                invitation={invitation}
                now={now}
              />
            ))}
          </ul>
        </FramePanel>
      </Frame>
    </section>
  );
}

function InvitationRow({
  organizationId,
  actorRole,
  invitation,
  now,
}: {
  organizationId: string;
  actorRole: OrganizationRole;
  invitation: Invitation;
  now: Date;
}) {
  const resend = useInviteMember(organizationId);
  const revoke = useRevokeInvitation(organizationId);
  const manageable = canManage(actorRole, invitation.role);

  return (
    <li className="flex items-center gap-3 px-4 py-3 text-sm">
      <span className="flex size-8 shrink-0 items-center justify-center rounded-full border border-dashed text-muted-foreground">
        <MailIcon className="size-4" />
      </span>
      <div className="flex min-w-0 flex-1 flex-col">
        <span className="truncate font-medium">{invitation.email}</span>
        <span className="truncate text-muted-foreground text-xs">
          {invitation.invitedByName ? `Invited by ${invitation.invitedByName} · ` : ""}
          Expires {formatRelativeDays(new Date(invitation.expiresAt), now)}
        </span>
      </div>
      <Badge variant="outline">{organizationRoleLabel(invitation.role)}</Badge>
      {manageable ? (
        <Menu>
          <MenuTrigger
            render={
              <Button
                size="icon-sm"
                variant="ghost"
                aria-label={`Actions for invitation to ${invitation.email}`}
              />
            }
          >
            <EllipsisIcon />
          </MenuTrigger>
          <MenuPopup align="end">
            <MenuItem
              disabled={resend.isPending}
              onClick={() =>
                resend.mutate(
                  { email: invitation.email, role: invitation.role },
                  {
                    onError: (error) =>
                      toastManager.add({
                        type: "error",
                        title: "Could not resend the invitation",
                        description: error.message,
                      }),
                  },
                )
              }
            >
              <RotateCwIcon />
              Resend invitation
            </MenuItem>
            <MenuItem
              variant="destructive"
              disabled={revoke.isPending}
              onClick={() => revoke.mutate(invitation.id)}
            >
              <XIcon />
              Revoke invitation
            </MenuItem>
          </MenuPopup>
        </Menu>
      ) : (
        <span className="size-8 sm:size-7" />
      )}
    </li>
  );
}
