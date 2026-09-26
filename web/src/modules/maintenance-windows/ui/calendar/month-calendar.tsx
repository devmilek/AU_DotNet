"use client";

import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import type React from "react";
import { Button } from "@/components/ui/button";
import { Group, GroupSeparator } from "@/components/ui/group";
import { cn } from "@/lib/utils";
import {
  type CalendarMonth,
  formatMonthTitle,
  isInMonth,
  isSameMonth,
  monthGrid,
  monthOf,
  shiftMonth,
  weekdayHeaders,
} from "../../lib/calendar";
import { type DateKey, dateKeyOf } from "../../lib/wall-clock";
import { DayEventsPopover } from "./day-events-popover";
import { EventChip } from "./event-chip";
import {
  type CalendarEvent,
  type DayEvent,
  groupEventsByDay,
} from "./group-events-by-day";

export type { CalendarEvent };

const MAX_VISIBLE_EVENTS = 3;

export function MonthCalendar({
  month,
  onMonthChange,
  events,
  timeZone,
  timeZoneLabel,
  now,
  loading,
  actions,
  renderEventDetails,
  className,
}: {
  month: CalendarMonth;
  onMonthChange: (month: CalendarMonth) => void;
  events: CalendarEvent[];
  timeZone: string;
  timeZoneLabel: string;
  now: Date;
  loading?: boolean;
  actions?: React.ReactNode;
  renderEventDetails?: (event: CalendarEvent) => React.ReactNode;
  className?: string;
}) {
  const days = monthGrid(month);
  const todayKey = dateKeyOf(now, timeZone);
  const eventsByDay = groupEventsByDay(events, timeZone, days[0], days.at(-1)!);
  const currentMonth = monthOf(todayKey);

  return (
    <section
      className={cn("flex flex-col gap-3", className)}
      aria-label="Calendar"
    >
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="font-heading text-lg" aria-live="polite">
          {formatMonthTitle(month)}
        </h2>
        <div className="flex items-center gap-2">
          {actions}
          <Button
            size="sm"
            variant="outline"
            disabled={isSameMonth(month, currentMonth)}
            onClick={() => onMonthChange(currentMonth)}
          >
            Today
          </Button>
          <Group aria-label="Change month">
            <Button
              size="icon-sm"
              variant="outline"
              aria-label="Previous month"
              onClick={() => onMonthChange(shiftMonth(month, -1))}
            >
              <ChevronLeftIcon />
            </Button>
            <GroupSeparator />
            <Button
              size="icon-sm"
              variant="outline"
              aria-label="Next month"
              onClick={() => onMonthChange(shiftMonth(month, 1))}
            >
              <ChevronRightIcon />
            </Button>
          </Group>
        </div>
      </div>

      <div
        className={cn(
          "overflow-hidden rounded-xl border bg-card transition-opacity",
          loading && "opacity-64",
        )}
        aria-busy={loading || undefined}
      >
        <div className="grid grid-cols-7 border-b bg-muted/72">
          {weekdayHeaders().map((weekday) => (
            <div
              key={weekday.long}
              className="px-2 py-1.5 text-center font-medium text-muted-foreground text-xs sm:text-start"
            >
              <abbr title={weekday.long} className="no-underline">
                {weekday.short}
              </abbr>
            </div>
          ))}
        </div>
        <div className="grid grid-cols-7">
          {days.map((dayKey) => (
            <CalendarDay
              key={dayKey}
              dayKey={dayKey}
              dayEvents={eventsByDay.get(dayKey) ?? []}
              inMonth={isInMonth(dayKey, month)}
              isToday={dayKey === todayKey}
              timeZone={timeZone}
              timeZoneLabel={timeZoneLabel}
              now={now}
              renderEventDetails={renderEventDetails}
            />
          ))}
        </div>
      </div>
    </section>
  );
}

function CalendarDay({
  dayKey,
  dayEvents,
  inMonth,
  isToday,
  timeZone,
  timeZoneLabel,
  now,
  renderEventDetails,
}: {
  dayKey: DateKey;
  dayEvents: DayEvent[];
  inMonth: boolean;
  isToday: boolean;
  timeZone: string;
  timeZoneLabel: string;
  now: Date;
  renderEventDetails?: (event: CalendarEvent) => React.ReactNode;
}) {
  const visible = dayEvents.slice(0, MAX_VISIBLE_EVENTS);
  const hiddenCount = dayEvents.length - visible.length;
  const dayNumber = Number(dayKey.slice(8));

  return (
    <div
      className={cn(
        "relative flex min-h-16 min-w-0 flex-col gap-1 border-e border-b p-1 sm:min-h-28 sm:p-1.5",
        "[&:nth-child(7n)]:border-e-0 [&:nth-last-child(-n+7)]:border-b-0",
        !inMonth && "bg-muted/40",
      )}
    >
      <span
        className={cn(
          "flex size-6 items-center justify-center self-center rounded-full text-xs tabular-nums sm:self-start",
          !inMonth && "text-muted-foreground/72",
          isToday && "bg-primary font-medium text-primary-foreground",
        )}
        aria-current={isToday ? "date" : undefined}
      >
        {dayNumber}
      </span>

      {dayEvents.length > 0 ? (
        <>
          <ul className="hidden min-w-0 flex-col gap-0.5 sm:flex">
            {visible.map((dayEvent) => (
              <li key={dayEvent.event.id} className="min-w-0">
                <EventChip
                  dayEvent={dayEvent}
                  timeZone={timeZone}
                  timeZoneLabel={timeZoneLabel}
                  now={now}
                  renderDetails={
                    renderEventDetails
                      ? () => renderEventDetails(dayEvent.event)
                      : undefined
                  }
                />
              </li>
            ))}
          </ul>

          {hiddenCount > 0 ? (
            <DayEventsPopover
              dayKey={dayKey}
              dayEvents={dayEvents}
              timeZone={timeZone}
              trigger={
                <Button
                  size="xs"
                  variant="ghost"
                  className="hidden self-start text-muted-foreground sm:inline-flex"
                >
                  +{hiddenCount} more
                </Button>
              }
            />
          ) : null}

          <DayEventsPopover
            dayKey={dayKey}
            dayEvents={dayEvents}
            timeZone={timeZone}
            trigger={
              <button
                type="button"
                className="absolute inset-0 flex items-end justify-center gap-0.5 pb-2 sm:hidden"
                aria-label={`${dayEvents.length} maintenance on ${dayKey}`}
              >
                {dayEvents.slice(0, 3).map((dayEvent) => (
                  <span
                    key={dayEvent.event.id}
                    className="size-1.5 rounded-full bg-info"
                  />
                ))}
              </button>
            }
          />
        </>
      ) : null}
    </div>
  );
}
