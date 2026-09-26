import { PlusIcon } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { requireOrganization } from "@/lib/auth";
import { ChannelTypePicker } from "@/modules/channels/ui/channel-type-picker";
import { ChannelsList } from "@/modules/channels/ui/channels-list";

export default async function NotificationsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  const organization = await requireOrganization(organizationSlug);
  const channelsHref = `/${organizationSlug}/notifications`;

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-4">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div className="space-y-1">
          <h1 className="font-heading text-2xl">Notifications</h1>
          <p className="text-muted-foreground text-sm">
            Where {organization.name} gets alerted when monitors go down.
          </p>
        </div>
        <Button render={<Link href={`${channelsHref}/create`} />}>
          <PlusIcon />
          New channel
        </Button>
      </div>

      <ChannelsList
        organizationId={organization.id}
        channelsHref={channelsHref}
      />

      <ChannelTypePicker createHref={`${channelsHref}/create`} />
    </div>
  );
}
