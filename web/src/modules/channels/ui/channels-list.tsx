"use client";

import { BellIcon, CircleAlertIcon } from "lucide-react";
import Link from "next/link";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Frame, FrameHeader, FramePanel } from "@/components/ui/frame";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import { useChannels } from "@/modules/channels/hooks/use-channels";
import {
  channelTypeLabels,
  describeChannelTarget,
} from "@/modules/channels/lib/channel-type";
import { ChannelTypeIcon } from "@/modules/channels/ui/channel-type-icon";

const columnsClassName =
  "grid grid-cols-[minmax(0,1fr)_auto] items-center gap-x-4 gap-y-1 md:grid-cols-[minmax(0,1fr)_5rem_minmax(0,1fr)_7rem]";

const monitorCountFormatter = new Intl.PluralRules(undefined);

function formatMonitorCount(count: number) {
  return `${count} ${monitorCountFormatter.select(count) === "one" ? "monitor" : "monitors"}`;
}

export function ChannelsList({
  organizationId,
  channelsHref,
}: {
  organizationId: string;
  channelsHref: string;
}) {
  const channels = useChannels(organizationId);

  if (channels.isError) {
    return (
      <Alert variant="error">
        <CircleAlertIcon />
        <AlertDescription>{channels.error.message}</AlertDescription>
      </Alert>
    );
  }

  if (channels.data?.length === 0) {
    return (
      <Frame>
        <FramePanel>
          <Empty>
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <BellIcon />
              </EmptyMedia>
              <EmptyTitle>No notification channels yet</EmptyTitle>
              <EmptyDescription>
                Pick a channel type below to start getting alerts when your
                monitors go down.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        </FramePanel>
      </Frame>
    );
  }

  return (
    <Frame>
      <FrameHeader
        className={cn(
          columnsClassName,
          "py-2.5 font-medium text-muted-foreground text-xs max-md:hidden",
        )}
      >
        <span>Name</span>
        <span>Type</span>
        <span>Sends to</span>
        <span className="text-end">Monitors</span>
      </FrameHeader>
      <FramePanel className="p-0">
        <ul className="divide-y" aria-busy={channels.isPending || undefined}>
          {channels.data
            ? channels.data.map((channel) => (
                <li
                  key={channel.id}
                  className={cn(columnsClassName, "px-5 py-3 text-sm")}
                >
                  <Link
                    href={`${channelsHref}/${channel.id}`}
                    className="truncate font-medium hover:underline hover:underline-offset-4"
                  >
                    {channel.name}
                  </Link>
                  <span>
                    <Badge variant="outline">
                      <ChannelTypeIcon type={channel.type} />
                      {channelTypeLabels[channel.type]}
                    </Badge>
                  </span>
                  <span className="truncate text-muted-foreground max-md:col-span-2">
                    {describeChannelTarget(channel.config)}
                  </span>
                  <span className="text-muted-foreground tabular-nums md:text-end">
                    {formatMonitorCount(channel.monitorCount)}
                  </span>
                </li>
              ))
            : Array.from({ length: 3 }, (_, index) => (
                <li key={index} className={cn(columnsClassName, "px-5 py-3.5")}>
                  <Skeleton className="h-4 w-40" />
                  <Skeleton className="h-4 w-12" />
                  <Skeleton className="h-4 w-48 max-md:col-span-2" />
                  <Skeleton className="h-4 w-20 md:justify-self-end" />
                </li>
              ))}
        </ul>
      </FramePanel>
    </Frame>
  );
}
