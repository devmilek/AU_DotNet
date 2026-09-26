"use client";

import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { BadgeOverflow } from "@/components/ui/badge-overflow";
import {
  Popover,
  PopoverPopup,
  PopoverTitle,
  PopoverTrigger,
} from "@/components/ui/popover";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import { useMonitorStatuses } from "@/modules/monitors/hooks/use-monitor-statuses";
import type { MonitorStatus } from "@/modules/monitors/lib/status";
import { StatusDot } from "@/modules/monitors/ui/status-dot";

type MonitorName = components["schemas"]["MonitorNameResponse"];

export function MonitorBadges({
  organizationId,
  monitors,
  monitorsHref,
  lineCount = 1,
  className,
}: {
  organizationId: string;
  monitors: MonitorName[];
  monitorsHref: string;
  lineCount?: number;
  className?: string;
}) {
  const statuses = useMonitorStatuses(organizationId);
  const statusById = new Map(
    statuses.data?.map((summary) => [summary.id, summary.status]),
  );

  if (monitors.length === 0) {
    return <span className="text-muted-foreground text-xs">No monitors</span>;
  }

  return (
    <div className={cn("relative min-w-0", className)}>
      <BadgeOverflow
        className="gap-1"
        items={monitors}
        lineCount={lineCount}
        getBadgeLabel={(monitor) => monitor.name}
        renderBadge={(monitor, label) => (
          <MonitorBadge
            href={`${monitorsHref}/${monitor.id}`}
            label={label}
            status={statusById.get(monitor.id)}
          />
        )}
        renderOverflow={(count) => (
          <Popover>
            <PopoverTrigger
              render={
                <Badge
                  variant="outline"
                  render={<button type="button" />}
                  aria-label={`Show ${count} more monitors`}
                />
              }
            >
              +{count}
            </PopoverTrigger>
            <PopoverPopup className="w-72" align="start">
              <PopoverTitle className="text-sm">
                {monitors.length} monitors
              </PopoverTitle>
              <ul className="mt-3 flex max-h-64 flex-col gap-1 overflow-y-auto">
                {monitors.map((monitor) => (
                  <li key={monitor.id}>
                    <Link
                      href={`${monitorsHref}/${monitor.id}`}
                      className="flex items-center gap-2 rounded-md px-2 py-1.5 text-sm hover:bg-accent"
                    >
                      <MonitorStatusDot status={statusById.get(monitor.id)} />
                      <span className="truncate">{monitor.name}</span>
                    </Link>
                  </li>
                ))}
              </ul>
            </PopoverPopup>
          </Popover>
        )}
      />
    </div>
  );
}

function MonitorBadge({
  href,
  label,
  status,
}: {
  href: string;
  label: string;
  status?: MonitorStatus;
}) {
  return (
    <Badge
      variant="outline"
      className="max-w-44 gap-1.5"
      render={<Link href={href} />}
    >
      <MonitorStatusDot status={status} />
      <span className="truncate">{label}</span>
    </Badge>
  );
}

function MonitorStatusDot({ status }: { status?: MonitorStatus }) {
  return status ? (
    <StatusDot status={status} className="size-1.5 [&>span]:size-1.5" />
  ) : (
    <span className="size-1.5 shrink-0 rounded-full bg-muted" aria-hidden />
  );
}
