import { ArrowLeftIcon } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { requireOrganization } from "@/lib/auth";
import { ChannelTypePicker } from "@/modules/channels/ui/channel-type-picker";

export default async function CreateNotificationChannelPage({
  params,
}: {
  params: Promise<{ organizationSlug: string }>;
}) {
  const { organizationSlug } = await params;
  await requireOrganization(organizationSlug);
  const notificationsHref = `/${organizationSlug}/notifications`;

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-4">
      <Button
        size="sm"
        variant="ghost"
        className="-ms-2 self-start"
        render={<Link href={notificationsHref} />}
      >
        <ArrowLeftIcon />
        Notifications
      </Button>

      <ChannelTypePicker
        createHref={`${notificationsHref}/create`}
        title="New notification channel"
        headingLevel="h1"
      />
    </div>
  );
}
