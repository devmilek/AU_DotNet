import { notFound } from "next/navigation";
import { requireOrganization } from "@/lib/auth";
import { isUuid } from "@/lib/utils";
import { ChannelDetailsView } from "@/modules/channels/ui/channel-details-view";

export default async function ChannelDetailsPage({
  params,
}: {
  params: Promise<{ organizationSlug: string; channelId: string }>;
}) {
  const { organizationSlug, channelId } = await params;

  // API przyjmuje tylko GUID — inny segment to od razu 404
  if (!isUuid(channelId)) {
    notFound();
  }

  const organization = await requireOrganization(organizationSlug);

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-4">
      <ChannelDetailsView
        channelRef={{ organizationId: organization.id, channelId }}
        channelsHref={`/${organizationSlug}/notifications`}
      />
    </div>
  );
}
