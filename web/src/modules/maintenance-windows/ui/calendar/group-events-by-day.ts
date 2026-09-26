import { addDays, type DateKey, dateKeyOf } from "../../lib/wall-clock";

export type CalendarEvent = {
  id: string;
  title: string;
  start: Date;
  end: Date;
  cancelled?: boolean;
};

export type DayEvent = { event: CalendarEvent; continued: boolean };

const MAX_SPAN_DAYS = 62;

export function groupEventsByDay(
  events: CalendarEvent[],
  timeZone: string,
  firstDay: DateKey,
  lastDay: DateKey,
): Map<DateKey, DayEvent[]> {
  const byDay = new Map<DateKey, DayEvent[]>();

  for (const event of events) {
    const startKey = dateKeyOf(event.start, timeZone);
    const endKey = dateKeyOf(new Date(event.end.getTime() - 1), timeZone);

    let key = startKey < firstDay ? firstDay : startKey;
    for (let step = 0; key <= endKey && key <= lastDay && step < MAX_SPAN_DAYS; step++) {
      const dayEvents = byDay.get(key) ?? [];
      dayEvents.push({ event, continued: key !== startKey });
      byDay.set(key, dayEvents);
      key = addDays(key, 1);
    }
  }

  for (const dayEvents of byDay.values()) {
    dayEvents.sort(
      (a, b) =>
        Number(b.continued) - Number(a.continued) ||
        a.event.start.getTime() - b.event.start.getTime(),
    );
  }

  return byDay;
}
