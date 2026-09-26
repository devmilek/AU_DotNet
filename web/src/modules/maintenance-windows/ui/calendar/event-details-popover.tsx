import { ClockIcon, GlobeIcon } from "lucide-react";
import type React from "react";
import { Badge } from "@/components/ui/badge";
import {
  Popover,
  PopoverPopup,
  PopoverTitle,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Separator } from "@/components/ui/separator";
import { cn } from "@/lib/utils";
import {
  formatDate,
  formatDuration,
  formatRelative,
  formatTime,
  formatTimeRange,
} from "../../lib/format";
import { dateKeyOf } from "../../lib/wall-clock";
import type { CalendarEvent } from "./group-events-by-day";

const longDate: Intl.DateTimeFormatOptions = {
  weekday: "long",
  day: "numeric",
  month: "long",
  year: "numeric",
};

const shortDate: Intl.DateTimeFormatOptions = {
  weekday: "short",
  day: "numeric",
  month: "short",
};

export function EventDetailsPopover({
  event,
  timeZone,
  timeZoneLabel,
  now,
  trigger,
  renderDetails,
}: {
  event: CalendarEvent;
  timeZone: string;
  timeZoneLabel: string;
  now: Date;
  trigger: React.ReactElement;
  renderDetails?: () => React.ReactNode;
}) {
  const durationMinutes = Math.round(
    (event.end.getTime() - event.start.getTime()) / 60_000,
  );
  const sameDay =
    dateKeyOf(event.start, timeZone) ===
    dateKeyOf(new Date(event.end.getTime() - 1), timeZone);

  return (
    <Popover>
      <PopoverTrigger render={trigger} />
      <PopoverPopup className="w-80" align="start">
        <div className="flex flex-col gap-3">
          <div className="flex items-start gap-2.5">
            <span
              className={cn(
                "mt-1.5 size-2.5 shrink-0 rounded-full",
                event.cancelled ? "bg-muted-foreground/48" : "bg-info",
              )}
            />
            <div className="flex min-w-0 flex-col gap-1">
              <PopoverTitle className="text-base leading-snug">
                {event.title}
              </PopoverTitle>
              <OccurrenceStatus event={event} now={now} />
            </div>
          </div>

          <dl className="grid grid-cols-[1rem_minmax(0,1fr)] items-start gap-x-2.5 gap-y-2 text-sm">
            <dt className="pt-0.5 text-muted-foreground">
              <ClockIcon className="size-4" aria-label="When" />
            </dt>
            <dd className="flex flex-col gap-0.5">
              {sameDay ? (
                <>
                  <span>{formatDate(event.start, timeZone, longDate)}</span>
                  <span className="text-muted-foreground tabular-nums">
                    {formatTimeRange(event.start, event.end, timeZone)} ·{" "}
                    {formatDuration(durationMinutes)}
                  </span>
                </>
              ) : (
                <>
                  <span className="tabular-nums">
                    {formatDate(event.start, timeZone, shortDate)},{" "}
                    {formatTime(event.start, timeZone)} →{" "}
                    {formatDate(event.end, timeZone, shortDate)},{" "}
                    {formatTime(event.end, timeZone)}
                  </span>
                  <span className="text-muted-foreground">
                    {formatDuration(durationMinutes)}
                  </span>
                </>
              )}
            </dd>

            <dt className="pt-0.5 text-muted-foreground">
              <GlobeIcon className="size-4" aria-label="Time zone" />
            </dt>
            <dd className="text-muted-foreground">{timeZoneLabel}</dd>
          </dl>

          {renderDetails ? <DetailsSlot render={renderDetails} /> : null}
        </div>
      </PopoverPopup>
    </Popover>
  );
}

function DetailsSlot({ render }: { render: () => React.ReactNode }) {
  const content = render();
  if (!content) return null;

  return (
    <>
      <Separator />
      {content}
    </>
  );
}

function OccurrenceStatus({ event, now }: { event: CalendarEvent; now: Date }) {
  if (event.cancelled) {
    return (
      <span className="flex items-center gap-1.5 text-muted-foreground text-xs">
        <Badge variant="secondary" size="sm">
          Cancelled
        </Badge>
        monitors won’t be in maintenance
      </span>
    );
  }

  if (event.end <= now) {
    return (
      <span className="flex items-center gap-1.5 text-muted-foreground text-xs">
        <Badge variant="secondary" size="sm">
          Ended
        </Badge>
        {formatRelative(event.end, now)}
      </span>
    );
  }

  if (event.start <= now) {
    return (
      <span className="flex items-center gap-1.5 text-muted-foreground text-xs">
        <Badge variant="info" size="sm">
          In progress
        </Badge>
        ends {formatRelative(event.end, now)}
      </span>
    );
  }

  return (
    <span className="flex items-center gap-1.5 text-muted-foreground text-xs">
      <Badge variant="outline" size="sm">
        Upcoming
      </Badge>
      starts {formatRelative(event.start, now)}
    </span>
  );
}
