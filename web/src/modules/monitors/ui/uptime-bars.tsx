"use client";

import {
  Tooltip,
  TooltipCreateHandle,
  TooltipPopup,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";

type HourlySummary = components["schemas"]["HourlyCheckSummaryResponse"];

type HourBreakdown = {
  hour: Date;
  up: number;
  down: number;
  expected: number;
  /** Udziały w pojemności godziny, sumują się do 1. */
  upShare: number;
  downShare: number;
  noDataShare: number;
};

// jeden tooltip na całą listę zamiast osobnego na każdy z 24 × N słupków
const uptimeTooltip = TooltipCreateHandle<HourBreakdown>();

const hourFormatter = new Intl.DateTimeFormat(undefined, {
  hour: "2-digit",
  minute: "2-digit",
});
const dayFormatter = new Intl.DateTimeFormat(undefined, {
  day: "numeric",
  month: "short",
});
const percentFormatter = new Intl.NumberFormat(undefined, {
  style: "percent",
  maximumFractionDigits: 1,
});

function toBreakdown(summary: HourlySummary): HourBreakdown {
  const up = summary.upChecks;
  const down = summary.downChecks;
  // backend pilnuje expected >= up + down; Math.max chroni przed dzieleniem przez 0
  const expected = Math.max(summary.expectedChecks, up + down, 1);

  return {
    hour: new Date(summary.hour),
    up,
    down,
    expected,
    upShare: up / expected,
    downShare: down / expected,
    noDataShare: (expected - up - down) / expected,
  };
}

export function UptimeBars({
  hours,
  className,
}: {
  hours: HourlySummary[];
  className?: string;
}) {
  const breakdowns = hours.map(toBreakdown);
  const up = breakdowns.reduce((sum, hour) => sum + hour.up, 0);
  const total = breakdowns.reduce((sum, hour) => sum + hour.up + hour.down, 0);

  return (
    <div
      className={cn("flex h-7 items-stretch gap-0.5", className)}
      role="img"
      aria-label={
        total === 0
          ? "No checks in the last 24 hours"
          : `Uptime in the last 24 hours: ${percentFormatter.format(up / total)}`
      }
    >
      {breakdowns.map((hour) => (
        <TooltipTrigger
          key={hour.hour.toISOString()}
          handle={uptimeTooltip}
          payload={hour}
          delay={0}
          render={
            <div className="flex min-w-0 flex-1 flex-col-reverse overflow-hidden rounded-[2px] bg-muted-foreground/16 transition-opacity hover:opacity-80" />
          }
        >
          {/* od dołu: up, down; reszta (tło) to brak danych */}
          <span
            className="block w-full bg-success"
            style={{ height: `${hour.upShare * 100}%` }}
          />
          <span
            className="block w-full bg-destructive"
            style={{ height: `${hour.downShare * 100}%` }}
          />
        </TooltipTrigger>
      ))}
    </div>
  );
}

/** Wspólny popup dla wszystkich {@link UptimeBars} — renderuj raz, np. obok listy. */
export function UptimeBarsTooltip() {
  return (
    <Tooltip handle={uptimeTooltip}>
      {({ payload: hour }) =>
        hour ? (
          <TooltipPopup>
            <div className="flex flex-col gap-1 py-0.5">
              <span className="font-medium">
                {dayFormatter.format(hour.hour)},{" "}
                {hourFormatter.format(hour.hour)}–
                {hourFormatter.format(
                  new Date(hour.hour.getTime() + 60 * 60 * 1000),
                )}
              </span>
              {hour.up + hour.down === 0 ? (
                <span className="text-muted-foreground">No data</span>
              ) : (
                <>
                  <LegendRow
                    className="bg-success"
                    label="Up"
                    value={`${hour.up} · ${percentFormatter.format(hour.up / (hour.up + hour.down))}`}
                  />
                  <LegendRow
                    className="bg-destructive"
                    label="Down"
                    value={`${hour.down} · ${percentFormatter.format(hour.down / (hour.up + hour.down))}`}
                  />
                  {hour.noDataShare > 0 ? (
                    <LegendRow
                      className="bg-muted-foreground/16"
                      label="No data"
                      value={percentFormatter.format(hour.noDataShare)}
                    />
                  ) : null}
                </>
              )}
            </div>
          </TooltipPopup>
        ) : null
      }
    </Tooltip>
  );
}

function LegendRow({
  className,
  label,
  value,
}: {
  className: string;
  label: string;
  value: string;
}) {
  return (
    <span className="flex items-center gap-2">
      <span className={cn("size-2 rounded-[2px]", className)} />
      <span className="text-muted-foreground">{label}</span>
      <span className="ms-auto ps-3 tabular-nums">{value}</span>
    </span>
  );
}
