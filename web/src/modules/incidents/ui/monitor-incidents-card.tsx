"use client";

import { ArrowRightIcon, CircleAlertIcon, ShieldCheckIcon } from "lucide-react";
import Link from "next/link";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { useNow } from "@/hooks/use-now";
import { useIncidents } from "../hooks/use-incidents";
import { formatDateTime, formatElapsed } from "../lib/format";
import { AcknowledgementBadge, IncidentStatusBadge } from "./incident-badges";

const PAGE_SIZE = 5;

export function MonitorIncidentsCard({
  organizationId,
  monitorId,
  incidentsHref,
}: {
  organizationId: string;
  monitorId: string;
  incidentsHref: string;
}) {
  const incidents = useIncidents(organizationId, {
    status: "All",
    page: 1,
    pageSize: PAGE_SIZE,
    monitorId,
  });
  const now = useNow(30_000);
  const totalCount = incidents.data?.totalCount ?? 0;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Incidents</CardTitle>
        <CardDescription>
          {incidents.data
            ? totalCount === 0
              ? "No incidents recorded for this monitor."
              : `${totalCount} ${totalCount === 1 ? "incident" : "incidents"} in total${totalCount > PAGE_SIZE ? `, showing the latest ${PAGE_SIZE}` : ""}.`
            : "Outages detected for this monitor."}
        </CardDescription>
        {totalCount > 0 ? (
          <CardAction>
            <Button
              size="sm"
              variant="outline"
              render={<Link href={`${incidentsHref}?monitorId=${monitorId}`} />}
            >
              View all
              <ArrowRightIcon />
            </Button>
          </CardAction>
        ) : null}
      </CardHeader>

      {incidents.isError ? (
        <CardPanel>
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>{incidents.error.message}</AlertDescription>
          </Alert>
        </CardPanel>
      ) : incidents.data?.items.length === 0 ? (
        <CardPanel>
          <Empty className="py-6">
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <ShieldCheckIcon />
              </EmptyMedia>
              <EmptyTitle>No downtime so far</EmptyTitle>
              <EmptyDescription>
                When this monitor goes down, the incident shows up here.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        </CardPanel>
      ) : (
        <CardPanel className="p-0">
          <ul className="divide-y border-t">
            {incidents.data
              ? incidents.data.items.map((incident) => {
                  const startedAt = new Date(incident.startedAt);
                  const resolvedAt = incident.resolvedAt
                    ? new Date(incident.resolvedAt)
                    : null;

                  return (
                    <li
                      key={incident.id}
                      className="grid gap-x-4 gap-y-2 px-5 py-3.5 text-sm sm:grid-cols-[minmax(0,1fr)_auto] sm:items-center"
                    >
                      <div className="flex min-w-0 flex-col gap-1">
                        <Link
                          href={`${incidentsHref}/${incident.id}`}
                          className="truncate font-medium hover:underline hover:underline-offset-4"
                        >
                          {incident.name}
                        </Link>
                        <span className="truncate text-muted-foreground text-xs tabular-nums">
                          {formatDateTime(startedAt)} ·{" "}
                          {formatElapsed(startedAt, resolvedAt ?? now)}
                          {resolvedAt ? "" : " so far"}
                          {incident.cause ? ` · ${incident.cause}` : ""}
                        </span>
                      </div>
                      <div className="flex flex-wrap gap-1.5 sm:justify-end">
                        <IncidentStatusBadge incident={incident} />
                        <AcknowledgementBadge incident={incident} />
                      </div>
                    </li>
                  );
                })
              : Array.from({ length: 3 }, (_, index) => (
                  <li key={index} className="flex flex-col gap-2 px-5 py-4">
                    <Skeleton className="h-4 w-56" />
                    <Skeleton className="h-3 w-72" />
                  </li>
                ))}
          </ul>
        </CardPanel>
      )}
    </Card>
  );
}
