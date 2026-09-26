"use client";

import { CircleAlertIcon } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { useMonitorStatuses } from "@/modules/monitors/hooks/use-monitor-statuses";
import { useMonitors } from "@/modules/monitors/hooks/use-monitors";
import {
  buildMonitorsSearch,
  hasActiveFilters,
  MONITORS_PAGE_SIZE,
  type MonitorsFilters as MonitorsFiltersValues,
  parseMonitorsParams,
} from "@/modules/monitors/lib/search-params";
import { MonitorsFilters } from "@/modules/monitors/ui/monitors-filters";
import {
  MonitorsListEmpty,
  MonitorsListFrame,
  MonitorsListRows,
  MonitorsListSkeleton,
  MonitorsPagination,
} from "@/modules/monitors/ui/monitors-list";

export function MonitorsView({ organizationId }: { organizationId: string }) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // URL jest jedynym źródłem prawdy dla filtrów, sortowania i strony
  const params = parseMonitorsParams(searchParams);
  const { page, ...filters } = params;

  const monitors = useMonitors(organizationId, params);
  const statuses = useMonitorStatuses(organizationId);
  const statusById = new Map(
    statuses.data?.map((summary) => [summary.id, summary.status]),
  );

  const hrefForPage = (nextPage: number) =>
    `${pathname}${buildMonitorsSearch({ ...params, page: nextPage })}`;

  const handleFiltersChange = (next: MonitorsFiltersValues) => {
    // zmiana filtrów zawsze wraca na pierwszą stronę
    router.replace(`${pathname}${buildMonitorsSearch({ ...next, page: 1 })}`, {
      scroll: false,
    });
  };

  const filtered = hasActiveFilters(filters);
  const clearFiltersHref = `${pathname}${buildMonitorsSearch({
    ...params,
    search: "",
    types: [],
    page: 1,
  })}`;

  return (
    <div className="flex flex-col gap-4">
      <MonitorsFilters
        filters={filters}
        onFiltersChange={handleFiltersChange}
      />

      {monitors.isPending ? (
        <MonitorsListSkeleton />
      ) : monitors.isError ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertDescription>{monitors.error.message}</AlertDescription>
        </Alert>
      ) : monitors.data.items.length === 0 ? (
        <MonitorsListFrame>
          {page > 1 && monitors.data.totalCount > 0 ? (
            <MonitorsListEmpty
              filtered
              action={
                <Button
                  variant="outline"
                  render={<Link href={hrefForPage(1)} />}
                >
                  Go to first page
                </Button>
              }
            />
          ) : (
            <MonitorsListEmpty
              filtered={filtered}
              action={
                filtered ? (
                  <Button
                    variant="outline"
                    render={<Link href={clearFiltersHref} scroll={false} />}
                  >
                    Clear filters
                  </Button>
                ) : null
              }
            />
          )}
        </MonitorsListFrame>
      ) : (
        <MonitorsListFrame
          footer={
            <>
              <p className="text-muted-foreground text-sm">
                {formatRange(page, monitors.data.totalCount)}
              </p>
              <MonitorsPagination
                page={page}
                totalPages={monitors.data.totalPages ?? 0}
                hrefForPage={hrefForPage}
              />
            </>
          }
        >
          <div
            className="transition-opacity data-[stale=true]:opacity-64"
            data-stale={monitors.isPlaceholderData}
          >
            <MonitorsListRows
              items={monitors.data.items}
              statuses={statusById}
              hrefForMonitor={(monitorId) => `${pathname}/${monitorId}`}
            />
          </div>
        </MonitorsListFrame>
      )}
    </div>
  );
}

function formatRange(page: number, total: number) {
  const from = (page - 1) * MONITORS_PAGE_SIZE + 1;
  const to = Math.min(page * MONITORS_PAGE_SIZE, total);
  return `${from}–${to} of ${total} monitors`;
}
