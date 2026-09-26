"use client";

import {
  ArrowLeftIcon,
  CircleAlertIcon,
  CircleCheckIcon,
  CircleDotIcon,
  EyeIcon,
  type LucideIcon,
  PencilIcon,
  SearchXIcon,
  Trash2Icon,
} from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import type React from "react";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Card,
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
import { Tooltip, TooltipPopup, TooltipTrigger } from "@/components/ui/tooltip";
import { useIsClient } from "@/hooks/use-is-client";
import { useNow } from "@/hooks/use-now";
import { ApiError } from "@/lib/api/errors";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import type { IncidentRef } from "../../hooks/keys";
import { useAcknowledgeIncident, useIncident } from "../../hooks/use-incidents";
import { formatDateTime, formatElapsed } from "../../lib/format";
import { DeleteIncidentDialog } from "../delete-incident-dialog";
import { AcknowledgementBadge, IncidentStatusBadge } from "../incident-badges";
import { EditIncidentDialog } from "./edit-incident-dialog";

type Incident = components["schemas"]["IncidentResponse"];

export function IncidentDetailsView({
  incidentRef,
  listHref,
  monitorsHref,
  canManage,
}: {
  incidentRef: IncidentRef;
  listHref: string;
  monitorsHref: string;
  canManage: boolean;
}) {
  const incident = useIncident(incidentRef);
  const isClient = useIsClient();
  const backLink = (
    <Button
      size="sm"
      variant="ghost"
      className="-ms-2 self-start"
      render={<Link href={listHref} />}
    >
      <ArrowLeftIcon />
      Incidents
    </Button>
  );

  if (incident.isError) {
    const notFound =
      incident.error instanceof ApiError && incident.error.status === 404;

    return (
      <div className="flex flex-col gap-6">
        {backLink}
        {notFound ? (
          <Empty>
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <SearchXIcon />
              </EmptyMedia>
              <EmptyTitle>Incident not found</EmptyTitle>
              <EmptyDescription>
                It may have been deleted or belongs to another organization.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>{incident.error.message}</AlertDescription>
          </Alert>
        )}
      </div>
    );
  }

  if (!incident.data || !isClient) {
    return (
      <div className="flex flex-col gap-6">
        {backLink}
        <DetailsSkeleton />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      {backLink}
      <IncidentDetails
        incident={incident.data}
        incidentRef={incidentRef}
        listHref={listHref}
        monitorsHref={monitorsHref}
        canManage={canManage}
      />
    </div>
  );
}

function IncidentDetails({
  incident,
  incidentRef,
  listHref,
  monitorsHref,
  canManage,
}: {
  incident: Incident;
  incidentRef: IncidentRef;
  listHref: string;
  monitorsHref: string;
  canManage: boolean;
}) {
  const router = useRouter();
  const now = useNow(30_000);
  const acknowledge = useAcknowledgeIncident(incidentRef);
  const [editOpen, setEditOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);

  const resolved = incident.status === "Resolved";
  const startedAt = new Date(incident.startedAt);
  const resolvedAt = incident.resolvedAt ? new Date(incident.resolvedAt) : null;
  const canAcknowledge = !resolved && !incident.acknowledgement;

  return (
    <>
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0 space-y-2">
          <h1 className="font-heading text-2xl">{incident.name}</h1>
          <div className="flex flex-wrap items-center gap-1.5">
            <IncidentStatusBadge incident={incident} />
            <AcknowledgementBadge incident={incident} />
          </div>
        </div>

        <div className="flex flex-wrap gap-2">
          {canAcknowledge ? (
            <Button
              loading={acknowledge.isPending}
              onClick={() => acknowledge.mutate()}
            >
              <EyeIcon />
              Acknowledge
            </Button>
          ) : null}
          {canManage ? (
            <>
              <Button variant="outline" onClick={() => setEditOpen(true)}>
                <PencilIcon />
                Edit
              </Button>
              {resolved ? (
                <Button
                  variant="destructive-outline"
                  onClick={() => setDeleteOpen(true)}
                >
                  <Trash2Icon />
                  Delete
                </Button>
              ) : (
                <Tooltip>
                  <TooltipTrigger
                    render={<span className="inline-flex" tabIndex={0} />}
                  >
                    <Button variant="destructive-outline" disabled>
                      <Trash2Icon />
                      Delete
                    </Button>
                  </TooltipTrigger>
                  <TooltipPopup className="max-w-60">
                    Ongoing incidents can’t be deleted. You can delete it once
                    the monitor recovers.
                  </TooltipPopup>
                </Tooltip>
              )}
            </>
          ) : null}
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Cause</CardTitle>
          <CardDescription>
            {incident.cause
              ? "Recorded from the check that opened the incident, unless edited."
              : "No cause recorded for this incident."}
          </CardDescription>
        </CardHeader>
        {incident.cause ? (
          <CardPanel>
            <p className="whitespace-pre-line rounded-lg bg-muted px-3.5 py-2.5 font-mono text-sm">
              {incident.cause}
            </p>
          </CardPanel>
        ) : null}
      </Card>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]">
        <Card>
          <dl className="divide-y">
            <Row label="Monitor">
              <Link
                href={`${monitorsHref}/${incident.monitor.id}`}
                className="font-medium hover:underline hover:underline-offset-4"
              >
                {incident.monitor.name}
              </Link>
              <span className="block truncate text-muted-foreground text-xs">
                {incident.monitor.target}
              </span>
            </Row>
            <Row label="Started">
              <span className="tabular-nums">{formatDateTime(startedAt)}</span>
            </Row>
            <Row label="Resolved">
              {resolvedAt ? (
                <span className="tabular-nums">{formatDateTime(resolvedAt)}</span>
              ) : (
                <span className="text-muted-foreground">Not yet</span>
              )}
            </Row>
            <Row label="Duration">
              <span className="tabular-nums">
                {formatElapsed(startedAt, resolvedAt ?? now)}
              </span>
              {resolved ? null : (
                <span className="text-muted-foreground"> and counting</span>
              )}
            </Row>
            <Row label="Failed checks">
              <span className="tabular-nums">{incident.failedChecksCount}</span>
            </Row>
            {incident.startedInMaintenance ? (
              <Row label="Maintenance">
                Started during a maintenance window
              </Row>
            ) : null}
          </dl>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Timeline</CardTitle>
          </CardHeader>
          <CardPanel>
            <ol className="relative flex flex-col gap-5 before:absolute before:inset-y-2 before:start-[0.6875rem] before:w-px before:bg-border">
              <TimelineItem
                icon={CircleDotIcon}
                iconClassName="text-destructive-foreground"
                title="Incident started"
                at={startedAt}
                description={incident.cause}
              />
              {incident.acknowledgement ? (
                <TimelineItem
                  icon={EyeIcon}
                  iconClassName="text-info-foreground"
                  title={
                    incident.acknowledgement.userName
                      ? `Acknowledged by ${incident.acknowledgement.userName}`
                      : "Acknowledged"
                  }
                  at={new Date(incident.acknowledgement.at)}
                />
              ) : null}
              {resolvedAt ? (
                <TimelineItem
                  icon={CircleCheckIcon}
                  iconClassName="text-success-foreground"
                  title="Resolved"
                  at={resolvedAt}
                  description="The monitor recovered."
                />
              ) : null}
            </ol>
          </CardPanel>
        </Card>
      </div>

      <EditIncidentDialog
        incidentRef={incidentRef}
        incident={incident}
        open={editOpen}
        onOpenChange={setEditOpen}
      />

      <DeleteIncidentDialog
        organizationId={incidentRef.organizationId}
        incident={incident}
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        onDeleted={() => router.push(listHref)}
      />
    </>
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
    <div className="grid gap-1 px-5 py-3.5 text-sm sm:grid-cols-[8rem_minmax(0,1fr)] sm:gap-4">
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="min-w-0">{children}</dd>
    </div>
  );
}

function TimelineItem({
  icon: Icon,
  iconClassName,
  title,
  at,
  description,
}: {
  icon: LucideIcon;
  iconClassName: string;
  title: string;
  at: Date;
  description?: string | null;
}) {
  return (
    <li className="relative flex gap-3">
      <span className="z-10 flex size-6 shrink-0 items-center justify-center rounded-full bg-card">
        <Icon className={cn("size-4", iconClassName)} />
      </span>
      <div className="min-w-0 space-y-0.5 text-sm">
        <p className="font-medium">{title}</p>
        <p className="text-muted-foreground text-xs tabular-nums">
          {formatDateTime(at)}
        </p>
        {description ? (
          <p className="truncate text-muted-foreground text-xs">{description}</p>
        ) : null}
      </div>
    </li>
  );
}

function DetailsSkeleton() {
  return (
    <>
      <div className="space-y-2">
        <Skeleton className="h-8 w-72" />
        <Skeleton className="h-5 w-56" />
      </div>
      <Skeleton className="h-32 w-full rounded-xl" />
      <div className="grid gap-6 lg:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]">
        <Skeleton className="h-72 w-full rounded-xl" />
        <Skeleton className="h-72 w-full rounded-xl" />
      </div>
    </>
  );
}
