"use client";

import { BellIcon, BellOffIcon, CircleAlertIcon, PlusIcon } from "lucide-react";
import Link from "next/link";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogPanel,
  DialogPopup,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { Switch } from "@/components/ui/switch";
import type { MonitorRef } from "@/modules/monitors/hooks/keys";
import {
  useChannels,
  useMonitorChannels,
  useToggleMonitorChannel,
} from "../hooks/use-channels";
import { describeChannelTarget } from "../lib/channel-type";
import { ChannelTypeIcon } from "./channel-type-icon";

export function MonitorChannelsDialog({
  monitorRef,
  monitorName,
  notificationsHref,
}: {
  monitorRef: MonitorRef;
  monitorName: string;
  notificationsHref: string;
}) {
  const monitorChannels = useMonitorChannels(monitorRef);
  const connectedCount = monitorChannels.data?.length;

  return (
    <Dialog>
      <DialogTrigger render={<Button variant="outline" />}>
        {connectedCount === 0 ? <BellOffIcon /> : <BellIcon />}
        Notifications
        {connectedCount !== undefined ? (
          <Badge
            variant={connectedCount === 0 ? "warning" : "secondary"}
            size="sm"
            className="tabular-nums"
          >
            {connectedCount}
          </Badge>
        ) : null}
      </DialogTrigger>
      <DialogPopup className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Notification channels</DialogTitle>
          <DialogDescription>
            Choose where alerts for {monitorName} are sent. Changes are saved
            right away.
          </DialogDescription>
        </DialogHeader>
        <DialogPanel>
          <ChannelToggles
            monitorRef={monitorRef}
            notificationsHref={notificationsHref}
          />
        </DialogPanel>
        <DialogFooter className="sm:justify-between">
          <Button variant="ghost" render={<Link href={notificationsHref} />}>
            Manage channels
          </Button>
          <DialogClose render={<Button variant="outline" />}>Done</DialogClose>
        </DialogFooter>
      </DialogPopup>
    </Dialog>
  );
}

function ChannelToggles({
  monitorRef,
  notificationsHref,
}: {
  monitorRef: MonitorRef;
  notificationsHref: string;
}) {
  const channels = useChannels(monitorRef.organizationId);
  const monitorChannels = useMonitorChannels(monitorRef);
  const toggle = useToggleMonitorChannel(monitorRef);

  const error = channels.error ?? monitorChannels.error;
  if (error) {
    return (
      <Alert variant="error">
        <CircleAlertIcon />
        <AlertDescription>{error.message}</AlertDescription>
      </Alert>
    );
  }

  if (!channels.data || !monitorChannels.data) {
    return (
      <div className="flex flex-col gap-3">
        {Array.from({ length: 3 }, (_, index) => (
          <Skeleton key={index} className="h-12 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (channels.data.length === 0) {
    return (
      <Empty className="py-6">
        <EmptyHeader>
          <EmptyMedia variant="icon">
            <BellOffIcon />
          </EmptyMedia>
          <EmptyTitle>No notification channels yet</EmptyTitle>
          <EmptyDescription>
            Create a channel first, then connect it to this monitor.
          </EmptyDescription>
        </EmptyHeader>
        <EmptyContent>
          <Button render={<Link href={`${notificationsHref}/create`} />}>
            <PlusIcon />
            New channel
          </Button>
        </EmptyContent>
      </Empty>
    );
  }

  const connected = new Set(monitorChannels.data.map((channel) => channel.id));
  const pendingChannelId = toggle.isPending ? toggle.variables?.channelId : null;

  return (
    <div className="flex flex-col gap-3">
      {connected.size === 0 ? (
        <Alert variant="warning">
          <BellOffIcon />
          <AlertDescription>
            No channel is connected, so nobody gets alerted when this monitor
            goes down.
          </AlertDescription>
        </Alert>
      ) : null}

      <ul className="divide-y rounded-lg border">
        {channels.data.map((channel) => {
          const isConnected =
            pendingChannelId === channel.id
              ? Boolean(toggle.variables?.assign)
              : connected.has(channel.id);

          return (
            <li key={channel.id}>
              <label className="flex cursor-pointer items-center gap-3 px-3.5 py-3">
                <span className="flex size-8 shrink-0 items-center justify-center rounded-md bg-muted">
                  <ChannelTypeIcon type={channel.type} className="size-4" />
                </span>
                <span className="flex min-w-0 flex-1 flex-col">
                  <span className="flex items-center gap-2 truncate font-medium text-sm">
                    {channel.name}
                    {channel.isActive ? null : (
                      <Badge variant="secondary" size="sm">
                        Paused
                      </Badge>
                    )}
                  </span>
                  <span className="truncate text-muted-foreground text-xs">
                    {describeChannelTarget(channel.config)}
                  </span>
                </span>
                <Switch
                  checked={isConnected}
                  disabled={toggle.isPending}
                  onCheckedChange={(checked) =>
                    toggle.mutate({ channelId: channel.id, assign: checked })
                  }
                  aria-label={`Send alerts to ${channel.name}`}
                />
              </label>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
