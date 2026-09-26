"use client";

import { CircleAlertIcon, GlobeIcon } from "lucide-react";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { useNow } from "@/hooks/use-now";
import {
  useMaintenanceOccurrences,
  useMaintenanceWindows,
} from "../hooks/use-maintenance-windows";
import { monthGrid, monthOf } from "../lib/calendar";
import { browserTimeZone, formatTimeZone } from "../lib/time-zones";
import { addDays, dateKeyOf, fromLocalDateKey } from "../lib/wall-clock";
import { type CalendarEvent, MonthCalendar } from "./calendar/month-calendar";
import { MaintenanceWindowSummary } from "./maintenance-window-summary";

export function MaintenanceOccurrencesCalendar({
  organizationId,
  listHref,
  monitorsHref,
}: {
  organizationId: string;
  listHref: string;
  monitorsHref: string;
}) {
  const timeZone = browserTimeZone();
  const now = useNow(60_000);
  const [month, setMonth] = useState(() =>
    monthOf(dateKeyOf(new Date(), timeZone)),
  );

  const days = monthGrid(month);
  const occurrences = useMaintenanceOccurrences(organizationId, {
    from: fromLocalDateKey(days[0]),
    to: fromLocalDateKey(addDays(days[days.length - 1], 1)),
  });

  const maintenanceWindows = useMaintenanceWindows(organizationId);
  const windowsById = new Map(
    maintenanceWindows.data?.map((maintenanceWindow) => [
      maintenanceWindow.id,
      maintenanceWindow,
    ]),
  );
  const windowIdByOccurrence = new Map(
    occurrences.data?.map((occurrence) => [
      occurrence.id,
      occurrence.maintenanceWindowId,
    ]),
  );

  const renderEventDetails = (event: CalendarEvent) => {
    const windowId = windowIdByOccurrence.get(event.id);
    const maintenanceWindow = windowId ? windowsById.get(windowId) : undefined;

    return maintenanceWindow ? (
      <MaintenanceWindowSummary
        maintenanceWindow={maintenanceWindow}
        occurrenceName={event.title}
        href={`${listHref}/${maintenanceWindow.id}`}
        organizationId={organizationId}
        monitorsHref={monitorsHref}
      />
    ) : null;
  };

  const events: CalendarEvent[] = (occurrences.data ?? []).map(
    (occurrence) => ({
      id: occurrence.id,
      title: occurrence.name,
      start: new Date(occurrence.startsAtUtc),
      end: new Date(occurrence.endsAtUtc),
    }),
  );

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
        renderEventDetails={renderEventDetails}
        loading={occurrences.isFetching}
        actions={
          <Badge variant="outline" className="max-sm:hidden">
            <GlobeIcon />
            {formatTimeZone(timeZone)}
          </Badge>
        }
      />
    </div>
  );
}
