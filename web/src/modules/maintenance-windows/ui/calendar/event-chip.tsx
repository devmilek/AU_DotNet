import { CornerDownRightIcon } from "lucide-react";
import type React from "react";
import { cn } from "@/lib/utils";
import { formatTime } from "../../lib/format";
import { EventDetailsPopover } from "./event-details-popover";
import type { DayEvent } from "./group-events-by-day";

export function EventChip({
  dayEvent: { event, continued },
  timeZone,
  timeZoneLabel,
  now,
  renderDetails,
}: {
  dayEvent: DayEvent;
  timeZone: string;
  timeZoneLabel: string;
  now: Date;
  renderDetails?: () => React.ReactNode;
}) {
  const time = formatTime(event.start, timeZone);

  return (
    <EventDetailsPopover
      event={event}
      timeZone={timeZone}
      timeZoneLabel={timeZoneLabel}
      now={now}
      renderDetails={renderDetails}
      trigger={
        <button
          type="button"
          aria-label={`${event.title}, ${time}`}
          className={cn(
            "flex w-full min-w-0 cursor-pointer items-center gap-1 rounded-sm bg-info/8 px-1.5 py-0.5 text-start text-info-foreground text-xs outline-none transition-colors hover:bg-info/16 focus-visible:ring-2 focus-visible:ring-ring data-popup-open:bg-info/16 dark:bg-info/16 dark:hover:bg-info/24 dark:data-popup-open:bg-info/24",
            event.end <= now && "opacity-56",
            event.cancelled &&
              "bg-muted text-muted-foreground line-through hover:bg-muted data-popup-open:bg-muted dark:bg-muted dark:hover:bg-muted dark:data-popup-open:bg-muted",
          )}
        >
          {continued ? (
            <CornerDownRightIcon className="size-3 shrink-0" aria-hidden />
          ) : (
            <span className="shrink-0 font-medium tabular-nums">{time}</span>
          )}
          <span className="truncate">{event.title}</span>
        </button>
      }
    />
  );
}
