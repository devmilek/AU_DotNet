import type React from "react";
import {
  Popover,
  PopoverPopup,
  PopoverTitle,
  PopoverTrigger,
} from "@/components/ui/popover";
import { formatDate, formatTimeRange } from "../../lib/format";
import { type DateKey, toWallClock } from "../../lib/wall-clock";
import type { DayEvent } from "./group-events-by-day";

export function DayEventsPopover({
  dayKey,
  dayEvents,
  timeZone,
  trigger,
}: {
  dayKey: DateKey;
  dayEvents: DayEvent[];
  timeZone: string;
  trigger: React.ReactElement;
}) {
  return (
    <Popover>
      <PopoverTrigger render={trigger} />
      <PopoverPopup className="w-72" align="start">
        <PopoverTitle className="text-base">
          {formatDate(toWallClock(dayKey, "12:00"), "UTC", {
            weekday: "long",
            day: "numeric",
            month: "long",
          })}
        </PopoverTitle>
        <ul className="mt-3 flex flex-col gap-2">
          {dayEvents.map(({ event }) => (
            <li
              key={event.id}
              className="flex flex-col gap-0.5 rounded-md border-info/24 border-s-2 ps-2.5"
            >
              <span className="truncate font-medium text-sm">{event.title}</span>
              <span className="text-muted-foreground text-xs tabular-nums">
                {formatTimeRange(event.start, event.end, timeZone)}
              </span>
            </li>
          ))}
        </ul>
      </PopoverPopup>
    </Popover>
  );
}
