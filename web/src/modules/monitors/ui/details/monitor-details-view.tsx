"use client";

import { ArrowLeftIcon, CircleAlertIcon, SearchXIcon } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { ApiError } from "@/lib/api/errors";
import type { MonitorRef } from "@/modules/monitors/hooks/keys";
import {
  useMonitor,
  useMonitorStatus,
  useMonitorUptime,
} from "@/modules/monitors/hooks/use-monitor-details";
import {
  defaultResponseTimeRange,
  parseResponseTimeRange,
  type ResponseTimeRange,
  responseTimeRangeParam,
} from "@/modules/monitors/lib/response-time-range";
import { UptimeBarsTooltip } from "@/modules/monitors/ui/uptime-bars";
import { MonitorHeader } from "./monitor-header";
import { MonitorStatusCards } from "./monitor-status-cards";
import { MonitorUptimeSummary } from "./monitor-uptime-summary";
import { ResponseTimeCard } from "./response-time-card";

export function MonitorDetailsView({
  monitorRef,
  monitorsHref,
}: {
  monitorRef: MonitorRef;
  monitorsHref: string;
}) {
  const monitor = useMonitor(monitorRef);
  const status = useMonitorStatus(monitorRef, monitor.data?.intervalSeconds);
  const uptime = useMonitorUptime(monitorRef);
  const [range, setRange] = useResponseTimeRangeParam();

  const backLink = (
    <Button
      size="sm"
      variant="ghost"
      className="-ms-2 self-start"
      render={<Link href={monitorsHref} />}
    >
      <ArrowLeftIcon />
      Monitors
    </Button>
  );

  if (monitor.isError) {
    return (
      <div className="flex flex-col gap-6">
        {backLink}
        {monitor.error instanceof ApiError && monitor.error.status === 404 ? (
          <Empty>
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <SearchXIcon />
              </EmptyMedia>
              <EmptyTitle>Monitor not found</EmptyTitle>
              <EmptyDescription>
                It may have been deleted or belongs to another organization.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>{monitor.error.message}</AlertDescription>
          </Alert>
        )}
      </div>
    );
  }

  const sectionError = status.error ?? uptime.error;

  return (
    <div className="flex flex-col gap-6">
      {backLink}

      {monitor.data ? (
        <MonitorHeader monitor={monitor.data} monitorRef={monitorRef} />
      ) : (
        <div className="space-y-2">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-4 w-80" />
        </div>
      )}

      {sectionError ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertDescription>{sectionError.message}</AlertDescription>
        </Alert>
      ) : null}

      <MonitorStatusCards
        status={status.data}
        intervalSeconds={monitor.data?.intervalSeconds ?? 60}
        last24Hours={uptime.data?.last24Hours}
      />

      <MonitorUptimeSummary periods={uptime.data?.periods} />

      <ResponseTimeCard
        monitorRef={monitorRef}
        range={range}
        onRangeChange={setRange}
      />

      <UptimeBarsTooltip />
    </div>
  );
}

/** Zakres wykresu trzymany w URL (`?range=7d`), żeby link do widoku był współdzielny. */
function useResponseTimeRangeParam(): [
  ResponseTimeRange,
  (range: ResponseTimeRange) => void,
] {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const range = parseResponseTimeRange(searchParams.get("range"));

  const setRange = (next: ResponseTimeRange) => {
    const params = new URLSearchParams(searchParams);
    if (next === defaultResponseTimeRange) params.delete("range");
    else params.set("range", responseTimeRangeParam(next));

    const query = params.toString();
    router.replace(query ? `${pathname}?${query}` : pathname, {
      scroll: false,
    });
  };

  return [range, setRange];
}
