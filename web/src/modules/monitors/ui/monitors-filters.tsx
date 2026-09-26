"use client";

import { useForm } from "@tanstack/react-form";
import {
  ArrowDownWideNarrowIcon,
  ArrowUpNarrowWideIcon,
  ListFilterIcon,
  SearchIcon,
  XIcon,
} from "lucide-react";
import { useEffect, useRef } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import {
  Menu,
  MenuCheckboxItem,
  MenuGroup,
  MenuGroupLabel,
  MenuPopup,
  MenuTrigger,
} from "@/components/ui/menu";
import {
  Select,
  SelectItem,
  SelectPopup,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  areFiltersEqual,
  defaultMonitorsFilters,
  hasActiveFilters,
  type MonitorsFilters as MonitorsFiltersValues,
  monitorTypeLabels,
  monitorTypes,
  normalizeTypes,
  type SortByParam,
  sortByLabels,
  sortByOptions,
} from "@/modules/monitors/lib/search-params";

const SEARCH_DEBOUNCE_MS = 300;

export function MonitorsFilters({
  filters,
  onFiltersChange,
}: {
  /** Aktualne filtry z URL. */
  filters: MonitorsFiltersValues;
  onFiltersChange: (filters: MonitorsFiltersValues) => void;
}) {
  // ostatnie filtry wysłane do URL — pozwala odróżnić nasze zmiany od nawigacji wstecz/naprzód
  const lastSubmitted = useRef(filters);

  const form = useForm({
    defaultValues: filters,
    onSubmit: ({ value }) => {
      const next = { ...value, search: value.search.trim() };
      if (areFiltersEqual(next, lastSubmitted.current)) return;

      lastSubmitted.current = next;
      onFiltersChange(next);
    },
  });

  useEffect(() => {
    if (areFiltersEqual(filters, lastSubmitted.current)) return;

    lastSubmitted.current = filters;
    form.reset(filters);
  }, [filters, form]);

  const submit = () => void form.handleSubmit();

  return (
    <form
      className="flex flex-col gap-2 sm:flex-row sm:items-center"
      noValidate
      role="search"
      onSubmit={(event) => {
        event.preventDefault();
        event.stopPropagation();
        submit();
      }}
    >
      <form.Field
        name="search"
        listeners={{
          onChange: submit,
          onChangeDebounceMs: SEARCH_DEBOUNCE_MS,
        }}
      >
        {(field) => (
          <InputGroup className="sm:max-w-xs">
            <InputGroupAddon>
              <SearchIcon />
            </InputGroupAddon>
            <InputGroupInput
              name={field.name}
              type="search"
              value={field.state.value}
              onValueChange={field.handleChange}
              onBlur={field.handleBlur}
              placeholder="Search by name or target"
              aria-label="Search monitors"
              autoComplete="off"
            />
            {field.state.value ? (
              <InputGroupAddon align="inline-end">
                <Button
                  aria-label="Clear search"
                  size="icon-xs"
                  variant="ghost"
                  onClick={() => field.handleChange("")}
                >
                  <XIcon />
                </Button>
              </InputGroupAddon>
            ) : null}
          </InputGroup>
        )}
      </form.Field>

      <div className="flex items-center gap-2">
        <form.Field name="types" listeners={{ onChange: submit }}>
          {(field) => (
            <Menu>
              <MenuTrigger render={<Button variant="outline" />}>
                <ListFilterIcon />
                Type
                {field.state.value.length > 0 ? (
                  <Badge variant="secondary">{field.state.value.length}</Badge>
                ) : null}
              </MenuTrigger>
              <MenuPopup align="start">
                <MenuGroup>
                  <MenuGroupLabel>Monitor type</MenuGroupLabel>
                  {monitorTypes.map((type) => (
                    <MenuCheckboxItem
                      key={type}
                      checked={field.state.value.includes(type)}
                      onCheckedChange={(checked) =>
                        field.handleChange(
                          normalizeTypes(
                            checked
                              ? [...field.state.value, type]
                              : field.state.value.filter(
                                  (value) => value !== type,
                                ),
                          ),
                        )
                      }
                    >
                      {monitorTypeLabels[type]}
                    </MenuCheckboxItem>
                  ))}
                </MenuGroup>
              </MenuPopup>
            </Menu>
          )}
        </form.Field>

        <form.Field name="sortBy" listeners={{ onChange: submit }}>
          {(field) => (
            <Select
              items={sortByLabels}
              value={field.state.value}
              onValueChange={(value) => {
                if (value) field.handleChange(value as SortByParam);
              }}
            >
              <SelectTrigger aria-label="Sort by" className="sm:w-40">
                <span className="text-muted-foreground">Sort:</span>
                <SelectValue />
              </SelectTrigger>
              <SelectPopup>
                {sortByOptions.map((option) => (
                  <SelectItem key={option} value={option}>
                    {sortByLabels[option]}
                  </SelectItem>
                ))}
              </SelectPopup>
            </Select>
          )}
        </form.Field>

        <form.Field name="sortOrder" listeners={{ onChange: submit }}>
          {(field) => {
            const ascending = field.state.value === "asc";
            return (
              <Button
                aria-label={ascending ? "Sort ascending" : "Sort descending"}
                title={ascending ? "Ascending" : "Descending"}
                size="icon"
                variant="outline"
                onClick={() => field.handleChange(ascending ? "desc" : "asc")}
              >
                {ascending ? (
                  <ArrowUpNarrowWideIcon />
                ) : (
                  <ArrowDownWideNarrowIcon />
                )}
              </Button>
            );
          }}
        </form.Field>

        <form.Subscribe selector={(state) => hasActiveFilters(state.values)}>
          {(active) =>
            active ? (
              <Button
                variant="ghost"
                onClick={() => {
                  form.reset({
                    ...defaultMonitorsFilters,
                    sortBy: form.state.values.sortBy,
                    sortOrder: form.state.values.sortOrder,
                  });
                  submit();
                }}
              >
                Reset
              </Button>
            ) : null
          }
        </form.Subscribe>
      </div>
    </form>
  );
}
