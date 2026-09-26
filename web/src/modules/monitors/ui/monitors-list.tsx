import { ActivityIcon, SearchXIcon } from "lucide-react";
import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import {
  Frame,
  FrameFooter,
  FrameHeader,
  FramePanel,
} from "@/components/ui/frame";
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import { monitorTypeLabel } from "@/modules/monitors/lib/search-params";
import type { MonitorStatus } from "@/modules/monitors/lib/status";
import { StatusDot } from "@/modules/monitors/ui/status-dot";
import {
  UptimeBars,
  UptimeBarsTooltip,
} from "@/modules/monitors/ui/uptime-bars";

type MonitorListItem = components["schemas"]["MonitorListItemResponse"];

const columnsClassName =
  "grid grid-cols-[minmax(0,1fr)_auto] items-center gap-x-4 gap-y-1 md:grid-cols-[minmax(0,1fr)_5rem_minmax(0,1fr)_minmax(0,14rem)]";

export function MonitorsListFrame({
  children,
  footer,
}: {
  children: React.ReactNode;
  footer?: React.ReactNode;
}) {
  return (
    <Frame>
      <FrameHeader
        className={cn(
          columnsClassName,
          "py-2.5 font-medium text-muted-foreground text-xs max-md:hidden",
        )}
      >
        <span>Name</span>
        <span>Type</span>
        <span>Target</span>
        <span>Last 24 hours</span>
      </FrameHeader>
      <FramePanel className="p-0">{children}</FramePanel>
      {footer ? (
        <FrameFooter className="flex flex-col items-center gap-3 sm:flex-row sm:justify-between">
          {footer}
        </FrameFooter>
      ) : null}
    </Frame>
  );
}

export function MonitorsListRows({
  items,
  statuses,
  hrefForMonitor,
}: {
  items: MonitorListItem[];
  statuses: ReadonlyMap<string, MonitorStatus>;
  hrefForMonitor: (monitorId: string) => string;
}) {
  return (
    <>
      <ul className="divide-y">
        {items.map(({ monitor, last24Hours }) => (
          <li
            key={monitor.id}
            className={cn(columnsClassName, "px-5 py-3 text-sm")}
          >
            <div className="flex min-w-0 items-center gap-2.5">
              <MonitorStatusIndicator status={statuses.get(monitor.id)} />
              <Link
                href={hrefForMonitor(monitor.id)}
                className="truncate font-medium hover:underline hover:underline-offset-4"
              >
                {monitor.name}
              </Link>
            </div>
            <span>
              <Badge variant="outline">{monitorTypeLabel(monitor.type)}</Badge>
            </span>
            <span
              className="col-span-2 truncate font-mono text-muted-foreground text-xs md:col-span-1"
              title={monitor.target}
            >
              {monitor.target}
            </span>
            <UptimeBars
              hours={last24Hours}
              className="col-span-2 max-md:mt-1.5 md:col-span-1"
            />
          </li>
        ))}
      </ul>
      <UptimeBarsTooltip />
    </>
  );
}

function MonitorStatusIndicator({ status }: { status?: MonitorStatus }) {
  return status ? (
    <StatusDot status={status} />
  ) : (
    <span className="size-2 shrink-0 rounded-full bg-muted" aria-hidden />
  );
}

export function MonitorsListSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <MonitorsListFrame>
      <ul className="divide-y" aria-busy="true" aria-label="Loading monitors">
        {Array.from({ length: rows }, (_, index) => (
          <li key={index} className={cn(columnsClassName, "px-5 py-3.5")}>
            <Skeleton className="h-4 w-40 max-w-full" />
            <Skeleton className="h-4 w-12" />
            <Skeleton className="col-span-2 h-3.5 w-56 max-w-full md:col-span-1" />
            <Skeleton className="col-span-2 h-7 w-full md:col-span-1" />
          </li>
        ))}
      </ul>
    </MonitorsListFrame>
  );
}

export function MonitorsListEmpty({
  filtered,
  action,
}: {
  filtered: boolean;
  action?: React.ReactNode;
}) {
  return (
    <Empty>
      <EmptyHeader>
        <EmptyMedia variant="icon">
          {filtered ? <SearchXIcon /> : <ActivityIcon />}
        </EmptyMedia>
        <EmptyTitle>
          {filtered ? "No monitors match your filters" : "No monitors yet"}
        </EmptyTitle>
        <EmptyDescription>
          {filtered
            ? "Try a different search term or clear the filters."
            : "Monitors you create in this organization will show up here."}
        </EmptyDescription>
      </EmptyHeader>
      {action}
    </Empty>
  );
}

type PageItem = number | "ellipsis-start" | "ellipsis-end";

// zawsze pierwsza i ostatnia strona + sąsiedzi aktualnej
function getPageItems(page: number, totalPages: number): PageItem[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  const start = Math.max(2, Math.min(page - 1, totalPages - 4));
  const end = Math.min(totalPages - 1, Math.max(page + 1, 5));
  const items: PageItem[] = [1];

  if (start > 2) items.push("ellipsis-start");
  for (let current = start; current <= end; current++) items.push(current);
  if (end < totalPages - 1) items.push("ellipsis-end");
  items.push(totalPages);

  return items;
}

export function MonitorsPagination({
  page,
  totalPages,
  hrefForPage,
}: {
  page: number;
  totalPages: number;
  hrefForPage: (page: number) => string;
}) {
  if (totalPages <= 1) return null;

  const hasPrevious = page > 1;
  const hasNext = page < totalPages;

  return (
    <Pagination className="mx-0 w-auto">
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious
            aria-disabled={!hasPrevious || undefined}
            className={cn(
              buttonVariants({ size: "default", variant: "ghost" }),
              "max-sm:aspect-square max-sm:p-0",
              !hasPrevious && "pointer-events-none opacity-64",
            )}
            render={<Link href={hrefForPage(Math.max(1, page - 1))} />}
          />
        </PaginationItem>
        {getPageItems(page, totalPages).map((item) =>
          typeof item === "number" ? (
            <PaginationItem key={item}>
              <PaginationLink
                isActive={item === page}
                className={buttonVariants({
                  size: "icon",
                  variant: item === page ? "outline" : "ghost",
                })}
                render={<Link href={hrefForPage(item)} />}
              >
                {item}
              </PaginationLink>
            </PaginationItem>
          ) : (
            <PaginationItem key={item}>
              <PaginationEllipsis />
            </PaginationItem>
          ),
        )}
        <PaginationItem>
          <PaginationNext
            aria-disabled={!hasNext || undefined}
            className={cn(
              buttonVariants({ size: "default", variant: "ghost" }),
              "max-sm:aspect-square max-sm:p-0",
              !hasNext && "pointer-events-none opacity-64",
            )}
            render={<Link href={hrefForPage(Math.min(totalPages, page + 1))} />}
          />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  );
}
