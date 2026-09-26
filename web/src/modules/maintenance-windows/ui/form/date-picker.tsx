"use client";

import { CalendarIcon } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverPopup,
  PopoverTrigger,
} from "@/components/ui/popover";
import { formatDate } from "../../lib/format";
import {
  type DateKey,
  fromLocalDateKey,
  localDateKey,
  toWallClock,
} from "../../lib/wall-clock";

export function DatePicker({
  value,
  onValueChange,
  minDate,
  invalid,
  label,
}: {
  value: DateKey;
  onValueChange: (value: DateKey) => void;
  minDate?: DateKey;
  invalid?: boolean;
  label: string;
}) {
  const [open, setOpen] = useState(false);
  const selected = value ? fromLocalDateKey(value) : undefined;

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <Button
            variant="outline"
            className="w-full justify-start font-normal"
            aria-label={label}
            aria-invalid={invalid || undefined}
          />
        }
      >
        <CalendarIcon />
        {value ? (
          formatDate(toWallClock(value, "12:00"), "UTC")
        ) : (
          <span className="text-muted-foreground">Pick a date</span>
        )}
      </PopoverTrigger>
      <PopoverPopup align="start" className="w-auto">
        <Calendar
          mode="single"
          selected={selected}
          defaultMonth={selected}
          weekStartsOn={1}
          disabled={minDate ? { before: fromLocalDateKey(minDate) } : undefined}
          onSelect={(date: Date | undefined) => {
            if (!date) return;
            onValueChange(localDateKey(date));
            setOpen(false);
          }}
        />
      </PopoverPopup>
    </Popover>
  );
}
