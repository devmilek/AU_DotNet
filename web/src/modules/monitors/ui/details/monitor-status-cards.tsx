"use client";

import { Badge } from "@/components/ui/badge";
import { Card, CardPanel } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useNow } from "@/hooks/use-now";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import {
  formatDuration,
  formatInterval,
  formatRelativeTime,
} from "@/modules/monitors/lib/format";
import { UptimeBars } from "@/modules/monitors/ui/uptime-bars";

type MonitorStatus = components["schemas"]["MonitorStatusResponse"];
type HourlySummary = components["schemas"]["HourlyCheckSummaryResponse"];

const statusAppearance: Record<
  MonitorStatus["status"],
  { label: string; className: string; describe: (since: string) => string }
> = {
  Up: {
    label: "Up",
    className: "text-success-foreground",
    describe: (since) => `Currently up for ${since}`,
  },
  Down: {
    label: "Down",
    className: "text-destructive-foreground",
    describe: (since) => `Currently down for ${since}`,
  },
  Paused: {
    label: "Paused",
    className: "text-warning-foreground",
    describe: () => "Checks are paused",
  },
  Pending: {
    label: "Pending",
    className: "text-muted-foreground",
    describe: () => "Waiting for the first check",
  },
};

function StatCard({
  label,
  children,
  className,
}: {
  label: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <Card className={className}>
      <CardPanel className="flex flex-col gap-2 p-5">
        <span className="font-medium text-sm">{label}</span>
        {children}
      </CardPanel>
    </Card>
  );
}

export function MonitorStatusCards({
  status,
  intervalSeconds,
  last24Hours,
}: {
  status: MonitorStatus | undefined;
  intervalSeconds: number;
  last24Hours: HourlySummary[] | undefined;
}) {
  // co sekundę, żeby "Last checked" i czas trwania stanu nie stały w miejscu
  const now = useNow(1000);
  const appearance = status ? statusAppearance[status.status] : null;

  const up = last24Hours?.reduce((sum, h) => sum + h.upChecks, 0) ?? 0;
  const down = last24Hours?.reduce((sum, h) => sum + h.downChecks, 0) ?? 0;

  return (
    <div className="grid gap-4 md:grid-cols-3">
      <StatCard label="Current status">
        {status && appearance ? (
          <>
            <span className={cn("font-heading text-2xl", appearance.className)}>
              {appearance.label}
            </span>
            <span className="text-muted-foreground text-sm">
              {appearance.describe(
                status.since ? formatDuration(new Date(status.since), now) : "",
              )}
            </span>
          </>
        ) : (
          <StatSkeleton />
        )}
      </StatCard>

      <StatCard label="Last checked">
        {status ? (
          <>
            <span className="font-heading text-2xl">
              {status.lastCheck
                ? formatRelativeTime(new Date(status.lastCheck.checkedAt), now)
                : "Never"}
            </span>
            <span className="truncate text-muted-foreground text-sm">
              Checked every {formatInterval(intervalSeconds)}
              {status.lastCheck?.errorMessage
                ? ` · ${status.lastCheck.errorMessage}`
                : null}
            </span>
          </>
        ) : (
          <StatSkeleton />
        )}
      </StatCard>

      <StatCard label="Last 24 hours">
        {last24Hours ? (
          <>
            <UptimeBars hours={last24Hours} />
            <div className="flex gap-2">
              <Badge variant="outline">{up} up</Badge>
              <Badge variant="outline">{down} down</Badge>
            </div>
          </>
        ) : (
          <StatSkeleton />
        )}
      </StatCard>
    </div>
  );
}

function StatSkeleton() {
  return (
    <>
      <Skeleton className="h-8 w-32" />
      <Skeleton className="h-4 w-44" />
    </>
  );
}
