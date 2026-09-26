"use client";

import { PencilIcon, Trash2Icon } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useNow } from "@/hooks/use-now";
import type { components } from "@/lib/api/schema";
import type { MaintenanceWindowRef } from "../../hooks/keys";
import { formatDate, formatRelative, formatTime } from "../../lib/format";
import { browserTimeZone } from "../../lib/time-zones";
import { DeleteMaintenanceWindowDialog } from "../delete-maintenance-window-dialog";
import { BackLink, MaintenanceWindowLoader } from "./maintenance-window-loader";
import { MaintenanceWindowOverview } from "./maintenance-window-overview";
import { WindowOccurrences } from "./window-occurrences";

type MaintenanceWindow = components["schemas"]["MaintenanceWindowResponse"];

export function MaintenanceWindowDetailsView({
  windowRef,
  listHref,
  monitorsHref,
}: {
  windowRef: MaintenanceWindowRef;
  listHref: string;
  monitorsHref: string;
}) {
  const router = useRouter();
  const [deleteOpen, setDeleteOpen] = useState(false);
  const backLink = <BackLink href={listHref} label="Maintenance windows" />;
  const editHref = `${listHref}/${windowRef.windowId}/edit`;

  return (
    <MaintenanceWindowLoader
      windowRef={windowRef}
      backLink={backLink}
      fallback={<DetailsSkeleton />}
    >
      {(maintenanceWindow) => (
        <div className="flex flex-col gap-6">
          {backLink}

          <div className="flex flex-wrap items-start justify-between gap-4">
            <div className="min-w-0 space-y-1.5">
              <h1 className="truncate font-heading text-2xl">
                {maintenanceWindow.name}
              </h1>
              {maintenanceWindow.description ? (
                <p className="max-w-3xl whitespace-pre-line text-muted-foreground text-sm">
                  {maintenanceWindow.description}
                </p>
              ) : null}
              <NextOccurrenceLine maintenanceWindow={maintenanceWindow} />
            </div>
            <div className="flex gap-2">
              <Button variant="outline" render={<Link href={editHref} />}>
                <PencilIcon />
                Edit
              </Button>
              <Button
                variant="destructive-outline"
                onClick={() => setDeleteOpen(true)}
              >
                <Trash2Icon />
                Delete
              </Button>
            </div>
          </div>

          <MaintenanceWindowOverview
            maintenanceWindow={maintenanceWindow}
            organizationId={windowRef.organizationId}
            monitorsHref={monitorsHref}
          />

          <WindowOccurrences
            windowRef={windowRef}
            timeZone={maintenanceWindow.timeZoneId}
          />

          <DeleteMaintenanceWindowDialog
            organizationId={windowRef.organizationId}
            maintenanceWindow={maintenanceWindow}
            open={deleteOpen}
            onOpenChange={setDeleteOpen}
            onDeleted={() => router.push(listHref)}
          />
        </div>
      )}
    </MaintenanceWindowLoader>
  );
}

function DetailsSkeleton() {
  return (
    <>
      <div className="space-y-2">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-80" />
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        {Array.from({ length: 3 }, (_, index) => (
          <Skeleton key={index} className="h-52 w-full rounded-xl" />
        ))}
      </div>
      <Skeleton className="h-96 w-full rounded-xl" />
    </>
  );
}

function NextOccurrenceLine({
  maintenanceWindow,
}: {
  maintenanceWindow: MaintenanceWindow;
}) {
  const now = useNow(60_000);
  const next = maintenanceWindow.nextOccurrence;

  if (!next) {
    return (
      <p className="text-muted-foreground text-sm">
        No upcoming occurrences — this window has finished.
      </p>
    );
  }

  const timeZone = browserTimeZone();
  const start = new Date(next.startsAtUtc);
  const end = new Date(next.endsAtUtc);

  if (start <= now) {
    return (
      <p className="flex items-center gap-2 text-muted-foreground text-sm">
        <Badge variant="info">In progress</Badge>
        Ends {formatRelative(end, now)} at {formatTime(end, timeZone)}
      </p>
    );
  }

  return (
    <p className="text-muted-foreground text-sm">
      Next: {formatDate(start, timeZone)}, {formatTime(start, timeZone)} ·{" "}
      {formatRelative(start, now)}
    </p>
  );
}
