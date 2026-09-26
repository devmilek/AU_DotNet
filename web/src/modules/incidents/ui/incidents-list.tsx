"use client";

import {
  CircleAlertIcon,
  EllipsisIcon,
  EyeIcon,
  ShieldCheckIcon,
  Trash2Icon,
  XIcon,
} from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Frame, FrameHeader, FramePanel } from "@/components/ui/frame";
import {
  Menu,
  MenuItem,
  MenuLinkItem,
  MenuPopup,
  MenuTrigger,
} from "@/components/ui/menu";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTab } from "@/components/ui/tabs";
import { useNow } from "@/hooks/use-now";
import type { components } from "@/lib/api/schema";
import { cn, isUuid } from "@/lib/utils";
import { MonitorsPagination } from "@/modules/monitors/ui/monitors-list";
import { useIncidents } from "../hooks/use-incidents";
import {
  type IncidentStatusFilter,
  parsePage,
  parseStatusFilter,
  statusFilters,
} from "../lib/filters";
import { formatDateTime, formatElapsed } from "../lib/format";
import { DeleteIncidentDialog } from "./delete-incident-dialog";
import { AcknowledgementBadge, IncidentStatusBadge } from "./incident-badges";

type Incident = components["schemas"]["IncidentResponse"];

const columnsClassName =
  "grid grid-cols-[minmax(0,1fr)_auto] items-start gap-x-4 gap-y-2 md:grid-cols-[minmax(0,1.6fr)_minmax(0,1fr)_minmax(0,0.9fr)_minmax(0,1.1fr)_2rem]";

const emptyCopy: Record<IncidentStatusFilter, { title: string; description: string }> = {
  All: {
    title: "No incidents yet",
    description:
      "When a monitor goes down, the incident shows up here with its cause and timeline.",
  },
  Ongoing: {
    title: "All systems operational",
    description: "Nothing is down right now.",
  },
  Resolved: {
    title: "No resolved incidents",
    description: "Incidents appear here once their monitor recovers.",
  },
};

export function IncidentsList({
  organizationId,
  listHref,
  monitorsHref,
  canManage,
}: {
  organizationId: string;
  listHref: string;
  monitorsHref: string;
  canManage: boolean;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const status = parseStatusFilter(searchParams.get("status"));
  const page = parsePage(searchParams.get("page"));
  const monitorParam = searchParams.get("monitorId");
  const monitorId = monitorParam && isUuid(monitorParam) ? monitorParam : undefined;
  const incidents = useIncidents(organizationId, { status, page, monitorId });
  const filteredMonitorName = monitorId
    ? incidents.data?.items[0]?.monitor.name
    : undefined;
  const [toDelete, setToDelete] = useState<Incident | null>(null);
  const now = useNow(30_000);

  const hrefFor = (next: {
    status?: IncidentStatusFilter;
    page?: number;
    clearMonitor?: boolean;
  }) => {
    const params = new URLSearchParams(searchParams);
    if (next.clearMonitor) params.delete("monitorId");
    const nextStatus = next.status ?? status;
    const nextPage = next.page ?? page;
    if (nextStatus === "All") params.delete("status");
    else params.set("status", nextStatus.toLowerCase());
    if (nextPage <= 1) params.delete("page");
    else params.set("page", String(nextPage));
    const search = params.toString();
    return search ? `${pathname}?${search}` : pathname;
  };

  return (
    <div className="flex flex-col gap-4">
      <Tabs
        value={status}
        onValueChange={(value) =>
          router.replace(
            hrefFor({ status: value as IncidentStatusFilter, page: 1 }),
            { scroll: false },
          )
        }
      >
        <div className="flex flex-wrap items-center gap-3">
          <TabsList>
            {statusFilters.map((filter) => (
              <TabsTab key={filter.value} value={filter.value}>
                {filter.label}
              </TabsTab>
            ))}
          </TabsList>
          {monitorId ? (
            <Badge
              variant="outline"
              size="lg"
              render={
                <Link
                  href={hrefFor({ page: 1, clearMonitor: true })}
                  scroll={false}
                  aria-label="Clear monitor filter"
                />
              }
            >
              Monitor: {filteredMonitorName ?? "selected monitor"}
              <XIcon />
            </Badge>
          ) : null}
        </div>
      </Tabs>

      {incidents.isError ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertDescription>{incidents.error.message}</AlertDescription>
        </Alert>
      ) : incidents.data?.items.length === 0 ? (
        <Frame>
          <FramePanel>
            <Empty>
              <EmptyHeader>
                <EmptyMedia variant="icon">
                  <ShieldCheckIcon />
                </EmptyMedia>
                <EmptyTitle>{emptyCopy[status].title}</EmptyTitle>
                <EmptyDescription>{emptyCopy[status].description}</EmptyDescription>
              </EmptyHeader>
            </Empty>
          </FramePanel>
        </Frame>
      ) : (
        <Frame>
          <FrameHeader
            className={cn(
              columnsClassName,
              "py-2.5 font-medium text-muted-foreground text-xs max-md:hidden",
            )}
          >
            <span>Incident</span>
            <span>Monitor</span>
            <span>Duration</span>
            <span>Status</span>
            <span className="sr-only">Actions</span>
          </FrameHeader>
          <FramePanel className="p-0">
            <ul
              className={cn(
                "divide-y transition-opacity",
                incidents.isPlaceholderData && "opacity-64",
              )}
              aria-busy={incidents.isFetching || undefined}
            >
              {incidents.data
                ? incidents.data.items.map((incident) => (
                    <IncidentRow
                      key={incident.id}
                      incident={incident}
                      href={`${listHref}/${incident.id}`}
                      monitorsHref={monitorsHref}
                      canManage={canManage}
                      now={now}
                      onDelete={() => setToDelete(incident)}
                    />
                  ))
                : Array.from({ length: 4 }, (_, index) => (
                    <li key={index} className={cn(columnsClassName, "px-5 py-4")}>
                      <div className="flex flex-col gap-2">
                        <Skeleton className="h-4 w-48" />
                        <Skeleton className="h-3 w-64" />
                      </div>
                      <Skeleton className="h-4 w-28 max-md:hidden" />
                      <Skeleton className="h-4 w-20 max-md:hidden" />
                      <Skeleton className="h-5 w-36 max-md:hidden" />
                      <span />
                    </li>
                  ))}
            </ul>
          </FramePanel>
        </Frame>
      )}

      {incidents.data && incidents.data.totalPages && incidents.data.totalPages > 1 ? (
        <div className="flex justify-end">
          <MonitorsPagination
            page={page}
            totalPages={incidents.data.totalPages}
            hrefForPage={(nextPage) => hrefFor({ page: nextPage })}
          />
        </div>
      ) : null}

      <DeleteIncidentDialog
        organizationId={organizationId}
        incident={toDelete}
        open={toDelete !== null}
        onOpenChange={(open) => {
          if (!open) setToDelete(null);
        }}
      />
    </div>
  );
}

function IncidentRow({
  incident,
  href,
  monitorsHref,
  canManage,
  now,
  onDelete,
}: {
  incident: Incident;
  href: string;
  monitorsHref: string;
  canManage: boolean;
  now: Date;
  onDelete: () => void;
}) {
  const startedAt = new Date(incident.startedAt);
  const resolvedAt = incident.resolvedAt ? new Date(incident.resolvedAt) : null;
  const resolved = incident.status === "Resolved";

  return (
    <li className={cn(columnsClassName, "px-5 py-3.5 text-sm")}>
      <div className="flex min-w-0 flex-col gap-1">
        <Link
          href={href}
          className="truncate font-medium hover:underline hover:underline-offset-4"
        >
          {incident.name}
        </Link>
        {incident.cause ? (
          <span className="truncate text-muted-foreground text-xs">
            {incident.cause}
          </span>
        ) : null}
      </div>

      <div className="flex min-w-0 flex-col gap-0.5 max-md:order-last max-md:col-span-2">
        <Link
          href={`${monitorsHref}/${incident.monitor.id}`}
          className="truncate hover:underline hover:underline-offset-4"
        >
          {incident.monitor.name}
        </Link>
        <span className="truncate text-muted-foreground text-xs">
          {incident.monitor.target}
        </span>
      </div>

      <div className="flex min-w-0 flex-col gap-0.5 max-md:order-last max-md:col-span-2">
        <span className="tabular-nums">
          {formatElapsed(startedAt, resolvedAt ?? now)}
          {resolved ? null : (
            <span className="text-muted-foreground"> so far</span>
          )}
        </span>
        <span className="truncate text-muted-foreground text-xs tabular-nums">
          {formatDateTime(startedAt)}
        </span>
      </div>

      <div className="flex flex-wrap items-start gap-1.5 max-md:order-last max-md:col-span-2">
        <IncidentStatusBadge incident={incident} />
        <AcknowledgementBadge incident={incident} />
      </div>

      <Menu>
        <MenuTrigger
          render={
            <Button
              size="icon-sm"
              variant="ghost"
              aria-label={`Actions for ${incident.name}`}
            />
          }
        >
          <EllipsisIcon />
        </MenuTrigger>
        <MenuPopup align="end">
          <MenuLinkItem render={<Link href={href} />}>
            <EyeIcon />
            View details
          </MenuLinkItem>
          {canManage && resolved ? (
            <MenuItem variant="destructive" onClick={onDelete}>
              <Trash2Icon />
              Delete
            </MenuItem>
          ) : null}
        </MenuPopup>
      </Menu>
    </li>
  );
}
