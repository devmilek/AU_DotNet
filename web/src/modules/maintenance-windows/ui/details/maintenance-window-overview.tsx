import {
  BellIcon,
  BellOffIcon,
  GaugeIcon,
  type LucideIcon,
} from "lucide-react";
import type React from "react";
import { Card } from "@/components/ui/card";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import { formatDate, formatDuration, formatTimeRange } from "../../lib/format";
import { describeRule } from "../../lib/recurrence";
import { describeSeriesEnd, scheduleOfWindow } from "../../lib/schedule";
import { formatTimeZone } from "../../lib/time-zones";
import { addMinutes } from "../../lib/wall-clock";
import { MonitorBadges } from "../monitor-badges";

type MaintenanceWindow = components["schemas"]["MaintenanceWindowResponse"];

export function MaintenanceWindowOverview({
  maintenanceWindow,
  organizationId,
  monitorsHref,
}: {
  maintenanceWindow: MaintenanceWindow;
  organizationId: string;
  monitorsHref: string;
}) {
  const schedule = scheduleOfWindow(maintenanceWindow);
  const end = addMinutes(schedule.start, schedule.durationMinutes);

  return (
    <Card className="grid lg:grid-cols-2 lg:divide-x max-lg:divide-y">
      <dl className="divide-y">
        <Row label="Repeats">{describeRule(schedule.rule)}</Row>
        <Row label="Time">
          <span className="tabular-nums">
            {formatTimeRange(schedule.start, end, "UTC")}
          </span>{" "}
          <span className="text-muted-foreground">
            ({formatDuration(schedule.durationMinutes)})
          </span>
        </Row>
        <Row label="Time zone">{formatTimeZone(maintenanceWindow.timeZoneId)}</Row>
        <Row label="Starts">{formatDate(schedule.start, "UTC")}</Row>
        <Row label="Ends">{describeSeriesEnd(maintenanceWindow)}</Row>
      </dl>

      <dl className="divide-y">
        <Row
          label={
            <>
              Monitors
              <span className="ms-1 tabular-nums">
                ({maintenanceWindow.monitors.length})
              </span>
            </>
          }
        >
          <MonitorBadges
            organizationId={organizationId}
            monitors={maintenanceWindow.monitors}
            monitorsHref={monitorsHref}
            lineCount={3}
          />
        </Row>
        <Row label="Alerts">
          <Behavior
            enabled={maintenanceWindow.suppressNotifications}
            icon={maintenanceWindow.suppressNotifications ? BellOffIcon : BellIcon}
            title={
              maintenanceWindow.suppressNotifications ? "Muted" : "Sent as usual"
            }
            description={
              maintenanceWindow.suppressNotifications
                ? "Incidents don’t notify your channels."
                : "Incidents notify your channels."
            }
          />
        </Row>
        <Row label="Uptime">
          <Behavior
            enabled={maintenanceWindow.excludeFromSla}
            icon={GaugeIcon}
            title={
              maintenanceWindow.excludeFromSla
                ? "Excluded"
                : "Counts toward uptime"
            }
            description={
              maintenanceWindow.excludeFromSla
                ? "Downtime doesn’t lower your uptime and SLA."
                : "Downtime lowers your uptime and SLA."
            }
          />
        </Row>
      </dl>
    </Card>
  );
}

function Row({
  label,
  children,
}: {
  label: React.ReactNode;
  children: React.ReactNode;
}) {
  return (
    <div className="grid gap-1 px-5 py-3.5 text-sm sm:grid-cols-[7rem_minmax(0,1fr)] sm:gap-4">
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="min-w-0">{children}</dd>
    </div>
  );
}

function Behavior({
  enabled,
  icon: Icon,
  title,
  description,
}: {
  enabled: boolean;
  icon: LucideIcon;
  title: string;
  description: string;
}) {
  return (
    <div className="flex items-start gap-2">
      <Icon
        className={cn(
          "mt-0.5 size-4 shrink-0",
          enabled ? "text-info-foreground" : "text-muted-foreground",
        )}
      />
      <span className="flex flex-col">
        <span className="font-medium">{title}</span>
        <span className="text-muted-foreground text-xs">{description}</span>
      </span>
    </div>
  );
}
