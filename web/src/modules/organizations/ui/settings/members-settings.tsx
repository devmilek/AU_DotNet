"use client";

import { CircleAlertIcon } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Skeleton } from "@/components/ui/skeleton";
import {
  useInvitations,
  useMembers,
  useOrganization,
} from "../../hooks/use-organization-settings";
import { canManageMembers } from "../../lib/roles";
import { InviteMemberCard } from "./invite-member-card";
import { MembersList } from "./members-list";
import { PendingInvitations } from "./pending-invitations";

export function MembersSettings({ organizationId }: { organizationId: string }) {
  const organization = useOrganization(organizationId);
  const actorRole = organization.data?.currentUserRole;
  const canManage = actorRole !== undefined && canManageMembers(actorRole);
  const members = useMembers(organizationId);
  const invitations = useInvitations(organizationId, canManage);

  const error = organization.error ?? members.error ?? invitations.error;

  if (error) {
    return (
      <Alert variant="error">
        <CircleAlertIcon />
        <AlertDescription>{error.message}</AlertDescription>
      </Alert>
    );
  }

  if (!organization.data || actorRole === undefined) {
    return (
      <div className="flex flex-col gap-6">
        <Skeleton className="h-36 w-full rounded-xl" />
        <Skeleton className="h-64 w-full rounded-xl" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-8">
      {canManage ? (
        <InviteMemberCard organizationId={organizationId} actorRole={actorRole} />
      ) : null}

      <MembersList
        organizationId={organizationId}
        organizationName={organization.data.name}
        actorRole={actorRole}
        members={members.data}
      />

      {canManage && invitations.data ? (
        <PendingInvitations
          organizationId={organizationId}
          actorRole={actorRole}
          invitations={invitations.data}
        />
      ) : null}
    </div>
  );
}
