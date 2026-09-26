"use client";

import {
  CalendarClockIcon,
  CircleAlertIcon,
  EllipsisIcon,
  EyeIcon,
  PencilIcon,
  PlusIcon,
  Trash2Icon,
} from "lucide-react";
import Link from "next/link";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Frame, FrameHeader, FramePanel } from "@/components/ui/frame";
import {
  Menu,
  MenuItem,
  MenuLinkItem,
  MenuPopup,
  MenuTrigger,
} from "@/components/ui/menu";
import { Skeleton } from "@/components/ui/skeleton";
import { useNow } from "@/hooks/use-now";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import { useMaintenanceWindows } from "../hooks/use-maintenance-windows";
import {
  formatDate,
  formatDuration,
  formatRelative,
  formatTime,
  formatTimeRange,
} from "../lib/format";
import { describeRule } from "../lib/recurrence";
import { scheduleOfWindow } from "../lib/schedule";
import { browserTimeZone } from "../lib/time-zones";
import { addMinutes } from "../lib/wall-clock";
import { DeleteMaintenanceWindowDialog } from "./delete-maintenance-window-dialog";
import { MonitorBadges } from "./monitor-badges";
import { PolicyBadges } from "./policy-badges";

type MaintenanceWindow =
  components["schemas"]["MaintenanceWindowListItemResponse"];

const columnsClassName =
  "grid grid-cols-[minmax(0,1fr)_auto] items-start gap-x-4 gap-y-2 md:grid-cols-[minmax(0,1.1fr)_minmax(0,1.1fr)_minmax(0,0.9fr)_minmax(0,1fr)_2rem]";

export function MaintenanceWindowsList({
  organizationId,
  listHref,
  monitorsHref,
}: {
  organizationId: string;
  listHref: string;
  monitorsHref: string;
}) {
  const maintenanceWindows = useMaintenanceWindows(organizationId);
  const [toDelete, setToDelete] = useState<MaintenanceWindow | null>(null);
  const now = useNow(60_000);

  if (maintenanceWindows.isError) {
    return (
      <Alert variant="error">
        <CircleAlertIcon />
        <AlertDescription>{maintenanceWindows.error.message}</AlertDescription>
      </Alert>
    );
  }

  if (maintenanceWindows.data?.length === 0) {
    return (
      <Empty>
        <EmptyHeader>
          <EmptyMedia variant="icon">
            <CalendarClockIcon />
          </EmptyMedia>
          <EmptyTitle>No maintenance windows yet</EmptyTitle>
          <EmptyDescription>
            Schedule planned work so it doesn’t trigger alerts or count against
            your uptime.
          </EmptyDescription>
        </EmptyHeader>
        <EmptyContent>
          <Button render={<Link href={`${listHref}/create`} />}>
            <PlusIcon />
            New maintenance window
          </Button>
        </EmptyContent>
      </Empty>
    );
  }

  return (
    <>
      <Frame>
        <FrameHeader
          className={cn(
            columnsClassName,
            "py-2.5 font-medium text-muted-foreground text-xs max-md:hidden",
          )}
        >
          <span>Name</span>
          <span>Schedule</span>
          <span>Next occurrence</span>
          <span>Monitors</span>
          <span className="sr-only">Actions</span>
        </FrameHeader>
        <FramePanel className="p-0">
          <ul
            className="divide-y"
            aria-busy={maintenanceWindows.isPending || undefined}
          >
            {maintenanceWindows.data
              ? maintenanceWindows.data.map((maintenanceWindow) => (
                  <MaintenanceWindowRow
                    key={maintenanceWindow.id}
                    maintenanceWindow={maintenanceWindow}
                    organizationId={organizationId}
                    href={`${listHref}/${maintenanceWindow.id}`}
                    monitorsHref={monitorsHref}
                    now={now}
                    onDelete={() => setToDelete(maintenanceWindow)}
                  />
                ))
              : Array.from({ length: 3 }, (_, index) => (
                  <li key={index} className={cn(columnsClassName, "px-5 py-4")}>
                    <Skeleton className="h-4 w-40" />
                    <Skeleton className="h-4 w-48 max-md:hidden" />
                    <Skeleton className="h-4 w-32 max-md:hidden" />
                    <Skeleton className="h-4 w-28 max-md:hidden" />
                    <span />
                  </li>
                ))}
          </ul>
        </FramePanel>
      </Frame>

      <DeleteMaintenanceWindowDialog
        organizationId={organizationId}
        maintenanceWindow={toDelete}
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null);
        }}
      />
    </>
  );
}

function MaintenanceWindowRow({
  maintenanceWindow,
  organizationId,
  href,
  monitorsHref,
  now,
  onDelete,
}: {
  maintenanceWindow: MaintenanceWindow;
  organizationId: string;
  href: string;
  monitorsHref: string;
  now: Date;
  onDelete: () => void;
}) {
  const schedule = scheduleOfWindow(maintenanceWindow);
  const end = addMinutes(schedule.start, schedule.durationMinutes);

  return (
    <li className={cn(columnsClassName, "px-5 py-3.5 text-sm")}>
      <div className="flex min-w-0 flex-col gap-1">
        <Link
          href={href}
          className="truncate font-medium hover:underline hover:underline-offset-4"
        >
          {maintenanceWindow.name}
        </Link>
        {maintenanceWindow.description ? (
          <span className="truncate text-muted-foreground text-xs">
            {maintenanceWindow.description}
          </span>
        ) : null}
        <PolicyBadges
          suppressNotifications={maintenanceWindow.suppressNotifications}
          excludeFromSla={maintenanceWindow.excludeFromSla}
          className="mt-2"
        />
      </div>

      <div className="flex min-w-0 flex-col gap-0.5 max-md:order-last max-md:col-span-2">
        <span className="truncate">
          {schedule.rule
            ? describeRule(schedule.rule)
            : formatDate(schedule.start, "UTC")}
        </span>
        <span className="truncate text-muted-foreground text-xs tabular-nums">
          {formatTimeRange(schedule.start, end, "UTC")} ·{" "}
          {formatDuration(schedule.durationMinutes)} ·{" "}
          {maintenanceWindow.timeZoneId.replaceAll("_", " ")}
        </span>
      </div>

      <div className="min-w-0 max-md:order-last max-md:col-span-2">
        <NextOccurrence maintenanceWindow={maintenanceWindow} now={now} />
      </div>

      <MonitorBadges
        organizationId={organizationId}
        monitors={maintenanceWindow.monitors}
        monitorsHref={monitorsHref}
        className="max-md:order-last max-md:col-span-2"
      />

      <Menu>
        <MenuTrigger
          render={
            <Button
              size="icon-sm"
              variant="ghost"
              aria-label={`Actions for ${maintenanceWindow.name}`}
            />
          }
        >
          <EllipsisIcon />
        </MenuTrigger>
        <MenuPopup align="end">
          <MenuLinkItem render={<Link href={href} />}>
            <EyeIcon />
            View details
          </MenuLinkItem>
          <MenuLinkItem render={<Link href={`${href}/edit`} />}>
            <PencilIcon />
            Edit
          </MenuLinkItem>
          <MenuItem variant="destructive" onClick={onDelete}>
            <Trash2Icon />
            Delete
          </MenuItem>
        </MenuPopup>
      </Menu>
    </li>
  );
}

function NextOccurrence({
  maintenanceWindow,
  now,
}: {
  maintenanceWindow: MaintenanceWindow;
  now: Date;
}) {
  const { nextOccurrence } = maintenanceWindow;

  if (!nextOccurrence) {
    return <Badge variant="secondary">Finished</Badge>;
  }

  const timeZone = browserTimeZone();
  const start = new Date(nextOccurrence.startsAtUtc);
  const end = new Date(nextOccurrence.endsAtUtc);

  if (start <= now) {
    return (
      <div className="flex flex-col items-start gap-1">
        <Badge variant="info">In progress</Badge>
        <span className="text-muted-foreground text-xs">
          Ends {formatRelative(end, now)} · {formatTime(end, timeZone)}
        </span>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-0.5">
      <span>{formatDate(start, timeZone)}</span>
      <span className="text-muted-foreground text-xs tabular-nums">
        {formatTime(start, timeZone)} · {formatRelative(start, now)}
      </span>
    </div>
  );
}
