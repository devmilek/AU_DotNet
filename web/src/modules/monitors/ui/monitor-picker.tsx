"use client";

import { SearchIcon } from "lucide-react";
import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { CheckboxGroup } from "@/components/ui/checkbox-group";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import { Label } from "@/components/ui/label";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import { useMonitorOptions } from "@/modules/monitors/hooks/use-monitor-options";
import { monitorTypeLabel } from "@/modules/monitors/lib/search-params";

/** Wielokrotny wybór monitorów organizacji (kontrolowany — wartość to lista id). */
export function MonitorPicker({
  organizationId,
  value,
  onValueChange,
  disabled,
}: {
  organizationId: string;
  value: string[];
  onValueChange: (value: string[]) => void;
  disabled?: boolean;
}) {
  const options = useMonitorOptions(organizationId);
  const [search, setSearch] = useState("");

  if (options.isPending) {
    return (
      <div className="flex flex-col gap-3">
        {Array.from({ length: 3 }, (_, index) => (
          <Skeleton key={index} className="h-10 w-full" />
        ))}
      </div>
    );
  }

  if (options.isError) {
    return (
      <p className="text-destructive-foreground text-sm">
        {options.error.message}
      </p>
    );
  }

  const { monitors, truncated } = options.data;

  if (monitors.length === 0) {
    return (
      <p className="text-muted-foreground text-sm">
        This organization has no monitors yet. You can attach them later.
      </p>
    );
  }

  const query = search.trim().toLowerCase();
  const visible = query
    ? monitors.filter(
        (m) =>
          m.name.toLowerCase().includes(query) ||
          m.target.toLowerCase().includes(query),
      )
    : monitors;
  const visibleIds = visible.map((m) => m.id);
  const allVisibleSelected = visibleIds.every((id) => value.includes(id));

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center gap-2">
        <InputGroup>
          <InputGroupAddon>
            <SearchIcon />
          </InputGroupAddon>
          <InputGroupInput
            type="search"
            value={search}
            onValueChange={setSearch}
            placeholder="Filter monitors"
            aria-label="Filter monitors"
            autoComplete="off"
          />
        </InputGroup>
        <Button
          variant="outline"
          disabled={disabled || visible.length === 0}
          onClick={() =>
            // dotyczy tylko widocznych (przefiltrowanych) monitorów
            onValueChange(
              allVisibleSelected
                ? value.filter((id) => !visibleIds.includes(id))
                : Array.from(new Set([...value, ...visibleIds])),
            )
          }
        >
          {allVisibleSelected ? "Clear" : "Select all"}
        </Button>
      </div>

      <p className="text-muted-foreground text-xs" aria-live="polite">
        {value.length} of {monitors.length} selected
        {truncated ? " · showing the first 100 monitors" : null}
      </p>

      <ScrollArea className="max-h-80 rounded-lg border">
        {visible.length === 0 ? (
          <p className="p-4 text-muted-foreground text-sm">
            No monitors match “{search.trim()}”.
          </p>
        ) : (
          <CheckboxGroup
            aria-label="Monitors"
            className="gap-0 divide-y"
            value={value}
            onValueChange={onValueChange}
            disabled={disabled}
          >
            {visible.map((monitor) => (
              <Label
                key={monitor.id}
                className="flex w-full cursor-pointer items-center gap-3 px-4 py-3 hover:bg-accent/50"
              >
                <Checkbox value={monitor.id} />
                <span className="flex min-w-0 flex-1 flex-col">
                  <span className="truncate font-medium">{monitor.name}</span>
                  <span className="truncate font-mono text-muted-foreground text-xs">
                    {monitor.target}
                  </span>
                </span>
                <Badge variant="outline">
                  {monitorTypeLabel(monitor.type)}
                </Badge>
              </Label>
            ))}
          </CheckboxGroup>
        )}
      </ScrollArea>
    </div>
  );
}
