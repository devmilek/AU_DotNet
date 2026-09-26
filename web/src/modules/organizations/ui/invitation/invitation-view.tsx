"use client";

import {
  CircleAlertIcon,
  MailXIcon,
  TriangleAlertIcon,
  UserCheckIcon,
} from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import type React from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { components } from "@/lib/api/schema";
import { useLogout } from "@/modules/auth/hooks/use-logout";
import { useMe } from "@/modules/auth/hooks/use-me";
import {
  useAcceptInvitation,
  useInvitationPreview,
} from "../../hooks/use-organization-settings";
import { organizationRoleLabel } from "../../lib/role-label";

type InvitationPreview = components["schemas"]["InvitationPreview"];

const closedStateCopy: Record<
  Exclude<InvitationPreview["state"], "Pending">,
  { title: string; description: string }
> = {
  Accepted: {
    title: "Invitation already accepted",
    description: "This invitation has been used. Sign in to open the organization.",
  },
  Revoked: {
    title: "Invitation revoked",
    description: "An admin withdrew this invitation. Ask them to send a new one.",
  },
  Expired: {
    title: "Invitation expired",
    description: "Invitations are valid for a limited time. Ask for a new one.",
  },
};

export function InvitationView({ token }: { token: string }) {
  const preview = useInvitationPreview(token);
  const me = useMe();

  if (!token || preview.isError) {
    return (
      <Message
        icon={<MailXIcon />}
        title="Invitation not found"
        description="The link may be incomplete or no longer valid. Open it again from your email."
        action={<Button render={<Link href="/" />}>Go to Asterio Uptime</Button>}
      />
    );
  }

  if (preview.isPending || me.isPending) {
    return (
      <div className="flex flex-col items-center gap-4">
        <Skeleton className="size-14 rounded-2xl" />
        <Skeleton className="h-7 w-56" />
        <Skeleton className="h-4 w-72" />
        <Skeleton className="h-9 w-full" />
      </div>
    );
  }

  const invitation = preview.data;

  if (invitation.state !== "Pending") {
    const copy = closedStateCopy[invitation.state];
    return (
      <Message
        icon={<MailXIcon />}
        title={copy.title}
        description={copy.description}
        action={<Button render={<Link href="/" />}>Go to Asterio Uptime</Button>}
      />
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <InvitationHeader invitation={invitation} />
      {me.data ? (
        <SignedInActions
          token={token}
          invitation={invitation}
          signedInEmail={me.data.email}
        />
      ) : (
        <SignedOutActions token={token} invitation={invitation} />
      )}
    </div>
  );
}

function InvitationHeader({ invitation }: { invitation: InvitationPreview }) {
  return (
    <div className="flex flex-col items-center gap-3 text-center">
      <span className="flex size-14 items-center justify-center rounded-2xl bg-primary font-heading text-2xl text-primary-foreground">
        {invitation.organizationName.slice(0, 1).toUpperCase()}
      </span>
      <div className="space-y-1">
        <h1 className="font-heading text-2xl">
          Join {invitation.organizationName}
        </h1>
        <p className="text-balance text-muted-foreground text-sm">
          {invitation.invitedByName ?? "Someone"} invited you to join as{" "}
          <Badge variant="outline">{organizationRoleLabel(invitation.role)}</Badge>
        </p>
        <p className="text-muted-foreground text-xs">Sent to {invitation.email}</p>
      </div>
    </div>
  );
}

function SignedOutActions({
  token,
  invitation,
}: {
  token: string;
  invitation: InvitationPreview;
}) {
  const returnUrl = encodeURIComponent(`/invite?token=${token}`);

  return (
    <div className="flex flex-col gap-3">
      <Button render={<Link href={`/sign-in?returnUrl=${returnUrl}`} />}>
        Sign in to accept
      </Button>
      <Button
        variant="outline"
        render={
          <Link
            href={`/sign-up?email=${encodeURIComponent(invitation.email)}`}
          />
        }
      >
        Create an account
      </Button>
      <p className="text-center text-muted-foreground text-xs">
        New here? After confirming your email, open this invitation link again.
      </p>
    </div>
  );
}

function SignedInActions({
  token,
  invitation,
  signedInEmail,
}: {
  token: string;
  invitation: InvitationPreview;
  signedInEmail: string;
}) {
  const router = useRouter();
  const accept = useAcceptInvitation(token);
  const logout = useLogout(
    `/sign-in?returnUrl=${encodeURIComponent(`/invite?token=${token}`)}`,
  );
  const matches =
    signedInEmail.trim().toLowerCase() === invitation.email.toLowerCase();

  if (!matches) {
    return (
      <div className="flex flex-col gap-3">
        <Alert variant="warning">
          <TriangleAlertIcon />
          <AlertDescription>
            You’re signed in as{" "}
            <span className="font-medium text-foreground">{signedInEmail}</span>
            , but this invitation is for{" "}
            <span className="font-medium text-foreground">{invitation.email}</span>
            .
          </AlertDescription>
        </Alert>
        <Button loading={logout.isPending} onClick={() => logout.mutate()}>
          Sign in with another account
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-3">
      {accept.isError ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertDescription>{accept.error.message}</AlertDescription>
        </Alert>
      ) : null}
      <Button
        loading={accept.isPending}
        onClick={() =>
          accept.mutate(undefined, {
            onSuccess: (organization) => {
              router.push(`/${organization.slug}/monitors`);
              router.refresh();
            },
          })
        }
      >
        <UserCheckIcon />
        Accept invitation
      </Button>
      <Button variant="ghost" render={<Link href="/" />}>
        Not now
      </Button>
    </div>
  );
}

function Message({
  icon,
  title,
  description,
  action,
}: {
  icon: React.ReactNode;
  title: string;
  description: string;
  action: React.ReactNode;
}) {
  return (
    <div className="flex flex-col items-center gap-4 text-center">
      <span className="flex size-12 items-center justify-center rounded-xl border text-muted-foreground [&_svg]:size-5">
        {icon}
      </span>
      <div className="space-y-1">
        <h1 className="font-heading text-2xl">{title}</h1>
        <p className="text-balance text-muted-foreground text-sm">{description}</p>
      </div>
      {action}
    </div>
  );
}
