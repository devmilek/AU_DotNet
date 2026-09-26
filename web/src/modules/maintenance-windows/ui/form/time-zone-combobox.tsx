"use client";

import { GlobeIcon } from "lucide-react";
import {
  Combobox,
  ComboboxEmpty,
  ComboboxInput,
  ComboboxItem,
  ComboboxList,
  ComboboxPopup,
} from "@/components/ui/combobox";
import { formatTimeZone, listTimeZones } from "../../lib/time-zones";

export function TimeZoneCombobox({
  value,
  onValueChange,
  onBlur,
  invalid,
}: {
  value: string;
  onValueChange: (value: string) => void;
  onBlur?: () => void;
  invalid?: boolean;
}) {
  return (
    <Combobox
      items={listTimeZones()}
      value={value}
      onValueChange={(next) => {
        if (next) onValueChange(next);
      }}
      itemToStringLabel={formatTimeZone}
    >
      <ComboboxInput
        placeholder="Search time zones"
        startAddon={<GlobeIcon />}
        onBlur={onBlur}
        aria-invalid={invalid || undefined}
      />
      <ComboboxPopup>
        <ComboboxEmpty>No time zone found.</ComboboxEmpty>
        <ComboboxList>
          {(timeZone: string) => (
            <ComboboxItem key={timeZone} value={timeZone}>
              {formatTimeZone(timeZone)}
            </ComboboxItem>
          )}
        </ComboboxList>
      </ComboboxPopup>
    </Combobox>
  );
}
