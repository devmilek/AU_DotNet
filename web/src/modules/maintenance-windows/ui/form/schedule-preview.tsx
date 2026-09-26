"use client";

import {
  CircleAlertIcon,
  GlobeIcon,
  InfoIcon,
  TriangleAlertIcon,
} from "lucide-react";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardDescription,
  CardFooter,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import { useNow } from "@/hooks/use-now";
import { monthGrid, monthOf } from "../../lib/calendar";
import {
  formatDate,
  formatDuration,
  formatTimeRange,
} from "../../lib/format";
import {
  describeRule,
  firstOccurrences,
  hasOverlap,
  occurrencesBetween,
  type Occurrence,
  type Schedule,
} from "../../lib/recurrence";
import { scheduleFromValues } from "../../lib/schedule";
import { formatTimeZone } from "../../lib/time-zones";
import { addDays, toWallClock, wallClockNow } from "../../lib/wall-clock";
import type { ScheduleValues } from "../../schemas/create-maintenance-window";
import { type CalendarEvent, MonthCalendar } from "../calendar/month-calendar";

const UPCOMING_COUNT = 5;
const OVERLAP_SAMPLE = 200;

export function SchedulePreview({
  values,
  name,
}: {
  values: ScheduleValues;
  name: string;
}) {
  const now = useNow(60_000);
  const [month, setMonth] = useState(() => monthOf(values.startDate));
  const [followedStart, setFollowedStart] = useState(values.startDate);

  if (followedStart !== values.startDate) {
    setFollowedStart(values.startDate);
    if (/^\d{4}-\d{2}-\d{2}$/.test(values.startDate)) {
      setMonth(monthOf(values.startDate));
    }
  }

  const result = scheduleFromValues(values);
  const title = name.trim() || "Maintenance";
  const days = monthGrid(month);

  const events: CalendarEvent[] = result.ok
    ? occurrencesBetween(
        result.schedule,
        toWallClock(days[0], "00:00"),
        toWallClock(addDays(days[days.length - 1], 1), "00:00"),
      ).map((occurrence) => ({
        id: occurrence.start.toISOString(),
        title,
        start: occurrence.start,
        end: occurrence.end,
      }))
    : [];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Preview</CardTitle>
        <CardDescription>
          {result.ok ? describeRule(result.schedule.rule) : "Schedule"}
        </CardDescription>
      </CardHeader>
      <CardPanel className="flex flex-col gap-4">
        {result.ok ? (
          <ScheduleNotes schedule={result.schedule} />
        ) : (
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>{result.error}</AlertDescription>
          </Alert>
        )}

        <MonthCalendar
          month={month}
          onMonthChange={setMonth}
          events={events}
          timeZone="UTC"
          timeZoneLabel={formatTimeZone(values.timeZoneId)}
          now={wallClockNow(values.timeZoneId, now)}
          actions={
            <Badge variant="outline" className="max-sm:hidden">
              <GlobeIcon />
              {formatTimeZone(values.timeZoneId)}
            </Badge>
          }
        />
      </CardPanel>
      {result.ok ? (
        <CardFooter className="flex-col items-stretch gap-2">
          <UpcomingOccurrences
            occurrences={firstOccurrences(result.schedule, UPCOMING_COUNT)}
          />
        </CardFooter>
      ) : null}
    </Card>
  );
}

function ScheduleNotes({ schedule }: { schedule: Schedule }) {
  const sample = firstOccurrences(schedule, OVERLAP_SAMPLE);
  const startMatchesRule =
    !schedule.rule ||
    schedule.rule.between(schedule.start, schedule.start, true).length > 0;

  return (
    <>
      <p className="text-muted-foreground text-sm">
        {formatTimeRange(
          schedule.start,
          sample[0]?.end ?? schedule.start,
          "UTC",
        )}{" "}
        · {formatDuration(schedule.durationMinutes)} · occurrences are created
        up to 90 days ahead.
      </p>

      {hasOverlap(sample) ? (
        <Alert variant="warning">
          <TriangleAlertIcon />
          <AlertDescription>
            Occurrences overlap — the window lasts longer than the gap between
            them. Shorten the duration or repeat less often.
          </AlertDescription>
        </Alert>
      ) : null}

      {!startMatchesRule ? (
        <Alert variant="info">
          <InfoIcon />
          <AlertDescription>
            {formatDate(schedule.start, "UTC")} doesn’t match the rule, but the
            start date is always the first occurrence.
          </AlertDescription>
        </Alert>
      ) : null}
    </>
  );
}

function UpcomingOccurrences({ occurrences }: { occurrences: Occurrence[] }) {
  return (
    <>
      <h3 className="font-medium text-sm">
        {occurrences.length === 1 ? "Occurrence" : "First occurrences"}
      </h3>
      <ol className="flex flex-wrap gap-1.5">
        {occurrences.map((occurrence) => (
          <li key={occurrence.start.toISOString()}>
            <Badge variant="outline" size="lg" className="tabular-nums">
              {formatDate(occurrence.start, "UTC", {
                weekday: "short",
                day: "numeric",
                month: "short",
              })}
              <span className="text-muted-foreground">
                {formatTimeRange(occurrence.start, occurrence.end, "UTC")}
              </span>
            </Badge>
          </li>
        ))}
      </ol>
    </>
  );
}
