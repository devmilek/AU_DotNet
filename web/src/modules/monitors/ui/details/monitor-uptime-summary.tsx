import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { components } from "@/lib/api/schema";
import { formatUptime } from "@/modules/monitors/lib/format";

type UptimePeriod = components["schemas"]["UptimePeriodResponse"];

const periodLabels: Record<UptimePeriod["period"], string> = {
  Last24Hours: "Last 24 hours",
  Last7Days: "Last 7 days",
  Last30Days: "Last 30 days",
  Last365Days: "Last 365 days",
};

const periodOrder = Object.keys(periodLabels) as UptimePeriod["period"][];

export function MonitorUptimeSummary({
  periods,
}: {
  periods: UptimePeriod[] | undefined;
}) {
  return (
    <Card>
      <dl className="grid grid-cols-2 md:grid-cols-4">
        {periodOrder.map((period) => {
          const value = periods?.find((p) => p.period === period);
          return (
            <div
              key={period}
              // separatory: na mobile siatka 2×2, na desktopie jeden rząd
              className="flex flex-col gap-1.5 p-5 even:border-s max-md:nth-[n+3]:border-t md:not-first:border-s"
            >
              <dt className="text-muted-foreground text-sm">
                {periodLabels[period]}
              </dt>
              <dd className="font-heading text-2xl tabular-nums">
                {periods ? (
                  formatUptime(value?.uptimeRatio)
                ) : (
                  <Skeleton className="h-8 w-24" />
                )}
              </dd>
            </div>
          );
        })}
      </dl>
    </Card>
  );
}
