import { ActivityIcon, ArrowRightIcon, RepeatIcon } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import type { components } from "@/lib/api/schema";
import { formatTimeRange } from "../lib/format";
import { describeRule } from "../lib/recurrence";
import { scheduleOfWindow } from "../lib/schedule";
import { addMinutes } from "../lib/wall-clock";
import { MonitorBadges } from "./monitor-badges";
import { PolicyBadges } from "./policy-badges";

type MaintenanceWindow =
  components["schemas"]["MaintenanceWindowListItemResponse"];

export function MaintenanceWindowSummary({
  maintenanceWindow,
  occurrenceName,
  href,
  organizationId,
  monitorsHref,
}: {
  maintenanceWindow: MaintenanceWindow;
  occurrenceName: string;
  href: string;
  organizationId: string;
  monitorsHref: string;
}) {
  const schedule = scheduleOfWindow(maintenanceWindow);
  const end = addMinutes(schedule.start, schedule.durationMinutes);

  return (
    <div className="flex flex-col gap-3 text-sm">
      {maintenanceWindow.description ? (
        <p className="line-clamp-4 text-muted-foreground">
          {maintenanceWindow.description}
        </p>
      ) : null}

      <dl className="grid grid-cols-[1rem_minmax(0,1fr)] items-start gap-x-2.5 gap-y-2">
        <dt className="pt-0.5 text-muted-foreground">
          <RepeatIcon className="size-4" aria-label="Schedule" />
        </dt>
        <dd className="flex flex-col gap-0.5">
          {maintenanceWindow.name !== occurrenceName ? (
            <span className="truncate font-medium">{maintenanceWindow.name}</span>
          ) : null}
          <span>
            {schedule.rule ? describeRule(schedule.rule) : "Does not repeat"}
          </span>
          <span className="text-muted-foreground text-xs tabular-nums">
            {formatTimeRange(schedule.start, end, "UTC")}{" "}
            {maintenanceWindow.timeZoneId.replaceAll("_", " ")}
          </span>
        </dd>

        <dt className="pt-0.5 text-muted-foreground">
          <ActivityIcon className="size-4" aria-label="Monitors" />
        </dt>
        <dd className="min-w-0">
          <MonitorBadges
            organizationId={organizationId}
            monitors={maintenanceWindow.monitors}
            monitorsHref={monitorsHref}
            lineCount={2}
          />
        </dd>
      </dl>

      <PolicyBadges
        suppressNotifications={maintenanceWindow.suppressNotifications}
        excludeFromSla={maintenanceWindow.excludeFromSla}
      />

      <Button
        size="sm"
        variant="outline"
        className="self-start"
        render={<Link href={href} />}
      >
        Open window
        <ArrowRightIcon />
      </Button>
    </div>
  );
}
