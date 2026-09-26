"use client";

import {
  BanIcon,
  CalendarDaysIcon,
  CalendarOffIcon,
  CircleAlertIcon,
  EllipsisIcon,
  ListIcon,
  PencilIcon,
  Undo2Icon,
} from "lucide-react";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Frame, FramePanel } from "@/components/ui/frame";
import {
  Menu,
  MenuItem,
  MenuPopup,
  MenuSeparator,
  MenuTrigger,
} from "@/components/ui/menu";
import { PopoverClose } from "@/components/ui/popover";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTab } from "@/components/ui/tabs";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { useNow } from "@/hooks/use-now";
import { cn } from "@/lib/utils";
import type { MaintenanceWindowRef } from "../../hooks/keys";
import {
  useRestoreOccurrence,
  useWindowOccurrences,
} from "../../hooks/use-maintenance-window";
import { monthGrid, monthOf } from "../../lib/calendar";
import { formatDate, formatDuration, formatTimeRange } from "../../lib/format";
import {
  hasStarted,
  occurrencePhase,
  type WindowOccurrence,
} from "../../lib/occurrence";
import { formatTimeZone } from "../../lib/time-zones";
import { dateKeyOf } from "../../lib/wall-clock";
import { type CalendarEvent, MonthCalendar } from "../calendar/month-calendar";
import { CancelOccurrenceDialog } from "./cancel-occurrence-dialog";
import { EditOccurrenceDialog } from "./edit-occurrence-dialog";
import { OccurrenceBadges } from "./occurrence-badges";

const DAY_MS = 86_400_000;
const UPCOMING_DAYS = 120;
const PAST_DAYS = 90;

type View = "calendar" | "list";
type Period = "upcoming" | "past";

type OccurrenceActions = {
  edit: (occurrence: WindowOccurrence) => void;
  cancel: (occurrence: WindowOccurrence) => void;
  restore: (occurrence: WindowOccurrence) => void;
};

function availableActions(occurrence: WindowOccurrence, now: Date) {
  const phase = occurrencePhase(occurrence, now);
  return {
    canEdit: phase !== "cancelled",
    canCancel: phase === "upcoming",
    canRestore: phase === "cancelled" && !hasStarted(occurrence, now),
  };
}

export function WindowOccurrences({
  windowRef,
  timeZone,
}: {
  windowRef: MaintenanceWindowRef;
  timeZone: string;
}) {
  const [view, setView] = useState<View>("calendar");
  const [period, setPeriod] = useState<Period>("upcoming");
  const [editing, setEditing] = useState<WindowOccurrence | null>(null);
  const [editOpen, setEditOpen] = useState(false);
  const [cancelling, setCancelling] = useState<WindowOccurrence | null>(null);
  const [cancelOpen, setCancelOpen] = useState(false);
  const restoreOccurrence = useRestoreOccurrence(windowRef);

  const actions: OccurrenceActions = {
    edit: (occurrence) => {
      setEditing(occurrence);
      setEditOpen(true);
    },
    cancel: (occurrence) => {
      setCancelling(occurrence);
      setCancelOpen(true);
    },
    restore: (occurrence) => restoreOccurrence.mutate(occurrence.id),
  };

  return (
    <section className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="font-heading text-xl">Occurrences</h2>
        <div className="flex flex-wrap items-center gap-2">
          {view === "list" ? (
            <Tabs
              value={period}
              onValueChange={(value) => setPeriod(value as Period)}
            >
              <TabsList size="sm">
                <TabsTab value="upcoming">Upcoming</TabsTab>
                <TabsTab value="past">Past {PAST_DAYS} days</TabsTab>
              </TabsList>
            </Tabs>
          ) : null}
          <ToggleGroup
            variant="outline"
            size="sm"
            aria-label="View"
            value={[view]}
            onValueChange={(value) => {
              const [next] = value as View[];
              if (next) setView(next);
            }}
          >
            <ToggleGroupItem value="calendar">
              <CalendarDaysIcon />
              Calendar
            </ToggleGroupItem>
            <ToggleGroupItem value="list">
              <ListIcon />
              List
            </ToggleGroupItem>
          </ToggleGroup>
        </div>
      </div>

      {view === "calendar" ? (
        <OccurrenceCalendar
          windowRef={windowRef}
          timeZone={timeZone}
          actions={actions}
        />
      ) : (
        <OccurrenceList
          windowRef={windowRef}
          timeZone={timeZone}
          period={period}
          actions={actions}
        />
      )}

      <EditOccurrenceDialog
        windowRef={windowRef}
        occurrence={editing}
        timeZone={timeZone}
        open={editOpen}
        onOpenChange={setEditOpen}
      />
      <CancelOccurrenceDialog
        windowRef={windowRef}
        occurrence={cancelling}
        timeZone={timeZone}
        open={cancelOpen}
        onOpenChange={setCancelOpen}
      />
    </section>
  );
}

function OccurrenceList({
  windowRef,
  timeZone,
  period,
  actions,
}: {
  windowRef: MaintenanceWindowRef;
  timeZone: string;
  period: Period;
  actions: OccurrenceActions;
}) {
  const [loadedAt] = useState(() => Date.now());
  const now = useNow(60_000);

  const range =
    period === "upcoming"
      ? { from: new Date(loadedAt), to: new Date(loadedAt + UPCOMING_DAYS * DAY_MS) }
      : { from: new Date(loadedAt - PAST_DAYS * DAY_MS), to: new Date(loadedAt) };

  const occurrences = useWindowOccurrences(windowRef, range);

  const visible = (occurrences.data ?? [])
    .filter((occurrence) =>
      period === "past"
        ? new Date(occurrence.endsAtUtc) <= now
        : new Date(occurrence.endsAtUtc) > now,
    )
    .sort((a, b) =>
      period === "past"
        ? b.startsAtUtc.localeCompare(a.startsAtUtc)
        : a.startsAtUtc.localeCompare(b.startsAtUtc),
    );

  if (occurrences.isError) {
    return (
      <Alert variant="error">
        <CircleAlertIcon />
        <AlertDescription>{occurrences.error.message}</AlertDescription>
      </Alert>
    );
  }

  if (occurrences.isPending) {
    return (
      <Frame>
        <FramePanel className="flex flex-col gap-3">
          {Array.from({ length: 4 }, (_, index) => (
            <Skeleton key={index} className="h-12 w-full" />
          ))}
        </FramePanel>
      </Frame>
    );
  }

  if (visible.length === 0) {
    return (
      <Empty>
        <EmptyHeader>
          <EmptyMedia variant="icon">
            <CalendarOffIcon />
          </EmptyMedia>
          <EmptyTitle>
            {period === "upcoming"
              ? "No upcoming occurrences"
              : "No past occurrences"}
          </EmptyTitle>
          <EmptyDescription>
            {period === "upcoming"
              ? "Recurring windows are planned up to 90 days ahead."
              : `Nothing happened in the last ${PAST_DAYS} days.`}
          </EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <div
      className={cn(
        "flex flex-col gap-5 transition-opacity",
        occurrences.isPlaceholderData && "opacity-64",
      )}
    >
      {groupByMonth(visible, timeZone).map((group) => (
        <section key={group.key} className="flex flex-col gap-2">
          <h3 className="font-medium text-muted-foreground text-xs">
            {group.label}
          </h3>
          <Frame>
            <FramePanel className="p-0">
              <ul className="divide-y">
                {group.occurrences.map((occurrence) => (
                  <OccurrenceRow
                    key={occurrence.id}
                    occurrence={occurrence}
                    timeZone={timeZone}
                    now={now}
                    actions={actions}
                  />
                ))}
              </ul>
            </FramePanel>
          </Frame>
        </section>
      ))}
    </div>
  );
}

function groupByMonth(occurrences: WindowOccurrence[], timeZone: string) {
  const groups: { key: string; label: string; occurrences: WindowOccurrence[] }[] = [];

  for (const occurrence of occurrences) {
    const start = new Date(occurrence.startsAtUtc);
    const key = dateKeyOf(start, timeZone).slice(0, 7);
    let group = groups.at(-1);

    if (!group || group.key !== key) {
      group = {
        key,
        label: formatDate(start, timeZone, { month: "long", year: "numeric" }),
        occurrences: [],
      };
      groups.push(group);
    }

    group.occurrences.push(occurrence);
  }

  return groups;
}

function OccurrenceRow({
  occurrence,
  timeZone,
  now,
  actions,
}: {
  occurrence: WindowOccurrence;
  timeZone: string;
  now: Date;
  actions: OccurrenceActions;
}) {
  const start = new Date(occurrence.startsAtUtc);
  const end = new Date(occurrence.endsAtUtc);
  const cancelled = occurrence.status === "Cancelled";
  const minutes = Math.round((end.getTime() - start.getTime()) / 60_000);

  return (
    <li className="flex items-center gap-4 px-4 py-3 text-sm">
      <div
        className={cn(
          "flex w-12 shrink-0 flex-col items-center gap-0.5 rounded-lg border bg-muted/40 py-1.5 leading-none",
          cancelled && "opacity-56",
        )}
      >
        <span className="text-[0.625rem] text-muted-foreground uppercase">
          {formatDate(start, timeZone, { weekday: "short" })}
        </span>
        <span className="font-heading text-lg tabular-nums">
          {formatDate(start, timeZone, { day: "numeric" })}
        </span>
      </div>

      <div className="flex min-w-0 flex-1 flex-col gap-0.5">
        <span
          className={cn(
            "truncate font-medium",
            cancelled && "text-muted-foreground line-through",
          )}
        >
          {occurrence.name}
        </span>
        <span className="text-muted-foreground text-xs tabular-nums">
          {formatTimeRange(start, end, timeZone)} · {formatDuration(minutes)}
        </span>
        {occurrence.description ? (
          <span className="line-clamp-1 text-muted-foreground text-xs">
            {occurrence.description}
          </span>
        ) : null}
      </div>

      <div className="max-sm:hidden">
        <OccurrenceBadges occurrence={occurrence} now={now} />
      </div>

      <OccurrenceMenu occurrence={occurrence} now={now} actions={actions} />
    </li>
  );
}

function OccurrenceMenu({
  occurrence,
  now,
  actions,
}: {
  occurrence: WindowOccurrence;
  now: Date;
  actions: OccurrenceActions;
}) {
  const { canEdit, canCancel, canRestore } = availableActions(occurrence, now);

  if (!canEdit && !canRestore) {
    return <span className="size-8 shrink-0 sm:size-7" />;
  }

  return (
    <Menu>
      <MenuTrigger
        render={
          <Button size="icon-sm" variant="ghost" aria-label="Occurrence actions" />
        }
      >
        <EllipsisIcon />
      </MenuTrigger>
      <MenuPopup align="end">
        {canEdit ? (
          <MenuItem onClick={() => actions.edit(occurrence)}>
            <PencilIcon />
            Edit name and description
          </MenuItem>
        ) : null}
        {canRestore ? (
          <MenuItem onClick={() => actions.restore(occurrence)}>
            <Undo2Icon />
            Restore occurrence
          </MenuItem>
        ) : null}
        {canCancel ? (
          <>
            <MenuSeparator />
            <MenuItem
              variant="destructive"
              onClick={() => actions.cancel(occurrence)}
            >
              <BanIcon />
              Cancel occurrence
            </MenuItem>
          </>
        ) : null}
      </MenuPopup>
    </Menu>
  );
}

function OccurrenceCalendar({
  windowRef,
  timeZone,
  actions,
}: {
  windowRef: MaintenanceWindowRef;
  timeZone: string;
  actions: OccurrenceActions;
}) {
  const now = useNow(60_000);
  const [month, setMonth] = useState(() =>
    monthOf(dateKeyOf(new Date(), timeZone)),
  );

  const days = monthGrid(month);
  const occurrences = useWindowOccurrences(windowRef, {
    from: new Date(Date.parse(`${days[0]}T00:00:00Z`) - DAY_MS),
    to: new Date(Date.parse(`${days[days.length - 1]}T00:00:00Z`) + 2 * DAY_MS),
  });

  const byId = new Map(
    (occurrences.data ?? []).map((occurrence) => [occurrence.id, occurrence]),
  );

  const events: CalendarEvent[] = (occurrences.data ?? []).map((occurrence) => ({
    id: occurrence.id,
    title: occurrence.name,
    start: new Date(occurrence.startsAtUtc),
    end: new Date(occurrence.endsAtUtc),
    cancelled: occurrence.status === "Cancelled",
  }));

  return (
    <div className="flex flex-col gap-3">
      {occurrences.isError ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertDescription>{occurrences.error.message}</AlertDescription>
        </Alert>
      ) : null}

      <MonthCalendar
        month={month}
        onMonthChange={setMonth}
        events={events}
        timeZone={timeZone}
        timeZoneLabel={formatTimeZone(timeZone)}
        now={now}
        loading={occurrences.isFetching}
        renderEventDetails={(event) => {
          const occurrence = byId.get(event.id);
          if (!occurrence) return null;

          const { canEdit, canCancel, canRestore } = availableActions(
            occurrence,
            now,
          );

          return (
            <div className="flex flex-col gap-3">
              {occurrence.description ? (
                <p className="line-clamp-4 text-muted-foreground text-sm">
                  {occurrence.description}
                </p>
              ) : null}
              <OccurrenceBadges occurrence={occurrence} now={now} />
              <div className="flex flex-wrap gap-2">
                {canEdit ? (
                  <PopoverClose
                    render={
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => actions.edit(occurrence)}
                      />
                    }
                  >
                    <PencilIcon />
                    Edit
                  </PopoverClose>
                ) : null}
                {canRestore ? (
                  <PopoverClose
                    render={
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => actions.restore(occurrence)}
                      />
                    }
                  >
                    <Undo2Icon />
                    Restore
                  </PopoverClose>
                ) : null}
                {canCancel ? (
                  <PopoverClose
                    render={
                      <Button
                        size="sm"
                        variant="destructive-outline"
                        onClick={() => actions.cancel(occurrence)}
                      />
                    }
                  >
                    <BanIcon />
                    Cancel
                  </PopoverClose>
                ) : null}
              </div>
            </div>
          );
        }}
      />
    </div>
  );
}
