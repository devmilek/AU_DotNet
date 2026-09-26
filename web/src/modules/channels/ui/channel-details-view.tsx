"use client";

import { useForm } from "@tanstack/react-form";
import { ArrowLeftIcon, CircleAlertIcon, SearchXIcon } from "lucide-react";
import Link from "next/link";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardDescription,
  CardFooter,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { ApiError } from "@/lib/api/errors";
import type { components } from "@/lib/api/schema";
import type { ChannelRef } from "@/modules/channels/hooks/keys";
import {
  useChannel,
  useSetChannelMonitors,
} from "@/modules/channels/hooks/use-channels";
import { channelTypeLabels } from "@/modules/channels/lib/channel-type";
import { MonitorPicker } from "@/modules/monitors/ui/monitor-picker";

type Channel = components["schemas"]["NotificationChannelResponse"];

export function ChannelDetailsView({
  channelRef,
  channelsHref,
}: {
  channelRef: ChannelRef;
  channelsHref: string;
}) {
  const channel = useChannel(channelRef);

  const backLink = (
    <Button
      size="sm"
      variant="ghost"
      className="-ms-2 self-start"
      render={<Link href={channelsHref} />}
    >
      <ArrowLeftIcon />
      Notifications
    </Button>
  );

  if (channel.isError) {
    return (
      <div className="flex flex-col gap-6">
        {backLink}
        {channel.error instanceof ApiError && channel.error.status === 404 ? (
          <Empty>
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <SearchXIcon />
              </EmptyMedia>
              <EmptyTitle>Channel not found</EmptyTitle>
              <EmptyDescription>
                It may have been deleted or belongs to another organization.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>{channel.error.message}</AlertDescription>
          </Alert>
        )}
      </div>
    );
  }

  if (!channel.data) {
    return (
      <div className="flex flex-col gap-6">
        {backLink}
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-40 w-full" />
        <Skeleton className="h-80 w-full" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      {backLink}

      <div className="space-y-1">
        <div className="flex items-center gap-2">
          <h1 className="truncate font-heading text-2xl">
            {channel.data.name}
          </h1>
          <Badge variant="outline">
            {channelTypeLabels[channel.data.type]}
          </Badge>
        </div>
        <p className="text-muted-foreground text-sm">
          Alerts from the monitors below are delivered to this channel.
        </p>
      </div>

      <ChannelConfigCard channel={channel.data} />

      {/* key: po zapisie i odświeżeniu danych formularz startuje od nowego stanu serwera */}
      <ChannelMonitorsCard
        key={channel.data.monitors.map((m) => m.id).join(",")}
        channelRef={channelRef}
        initialMonitorIds={channel.data.monitors.map((m) => m.id)}
      />
    </div>
  );
}

function ChannelConfigCard({ channel }: { channel: Channel }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Recipients</CardTitle>
        <CardDescription>Email addresses that receive alerts.</CardDescription>
      </CardHeader>
      <CardPanel>
        <ul className="flex flex-wrap gap-2">
          {(channel.config.email?.to ?? []).map((address) => (
            <li key={address}>
              <Badge variant="secondary" size="lg">
                {address}
              </Badge>
            </li>
          ))}
        </ul>
      </CardPanel>
    </Card>
  );
}

function ChannelMonitorsCard({
  channelRef,
  initialMonitorIds,
}: {
  channelRef: ChannelRef;
  initialMonitorIds: string[];
}) {
  const setMonitors = useSetChannelMonitors(channelRef);

  const form = useForm({
    defaultValues: { monitorIds: initialMonitorIds },
    onSubmit: async ({ value }) => {
      await setMonitors.mutateAsync(value.monitorIds);
    },
  });

  return (
    <form
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        event.stopPropagation();
        void form.handleSubmit();
      }}
    >
      <Card>
        <CardHeader>
          <CardTitle>Monitors</CardTitle>
          <CardDescription>
            Choose which monitors send their alerts to this channel.
          </CardDescription>
        </CardHeader>
        <CardPanel>
          <form.Field name="monitorIds">
            {(field) => (
              <MonitorPicker
                organizationId={channelRef.organizationId}
                value={field.state.value}
                onValueChange={field.handleChange}
                disabled={setMonitors.isPending}
              />
            )}
          </form.Field>
        </CardPanel>
        <CardFooter className="justify-end gap-2">
          <form.Subscribe
            selector={(state) => ({
              changed: !sameIds(state.values.monitorIds, initialMonitorIds),
              isSubmitting: state.isSubmitting,
            })}
          >
            {({ changed, isSubmitting }) => (
              <>
                <Button
                  variant="ghost"
                  disabled={!changed || isSubmitting}
                  onClick={() => form.reset()}
                >
                  Discard
                </Button>
                <Button
                  type="submit"
                  disabled={!changed}
                  loading={isSubmitting}
                >
                  Save changes
                </Button>
              </>
            )}
          </form.Subscribe>
        </CardFooter>
      </Card>
    </form>
  );
}

// kolejność zaznaczania nie ma znaczenia — porównujemy zbiory
function sameIds(a: string[], b: string[]) {
  return a.length === b.length && a.every((id) => b.includes(id));
}
