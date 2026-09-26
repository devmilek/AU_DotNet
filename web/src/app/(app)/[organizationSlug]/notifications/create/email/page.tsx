import { ArrowLeftIcon } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { requireOrganization } from "@/lib/auth";
import { CreateEmailChannelForm } from "@/modules/channels/ui/create-email-channel-form";

export default async function CreateEmailChannelPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-4">
      <div className="flex flex-col items-start gap-3">
        <Button
          size="sm"
          variant="ghost"
          className="-ms-2"
          render={<Link href={`/${organizationSlug}/notifications/create`} />}
        >
          <ArrowLeftIcon />
          All channel types
        </Button>
        <div className="space-y-1">
          <h1 className="font-heading text-2xl">New email channel</h1>
          <p className="text-muted-foreground text-sm">
            Send incident alerts to one or more email addresses.
          </p>
        </div>
      </div>

      <CreateEmailChannelForm
        organizationId={organization.id}
        organizationSlug={organizationSlug}
      />
    </div>
  );
}
