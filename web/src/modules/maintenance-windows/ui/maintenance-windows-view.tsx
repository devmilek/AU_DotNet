"use client";

import { CalendarDaysIcon, ListIcon } from "lucide-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsPanel, TabsTab } from "@/components/ui/tabs";
import { useIsClient } from "@/hooks/use-is-client";
import { MaintenanceOccurrencesCalendar } from "./maintenance-occurrences-calendar";
import { MaintenanceWindowsList } from "./maintenance-windows-list";

const views = ["list", "calendar"] as const;
type View = (typeof views)[number];

function parseView(value: string | null): View {
  return views.includes(value as View) ? (value as View) : "list";
}

export function MaintenanceWindowsView({
  organizationId,
  listHref,
  monitorsHref,
}: {
  organizationId: string;
  listHref: string;
  monitorsHref: string;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const isClient = useIsClient();
  const view = parseView(searchParams.get("view"));

  const handleViewChange = (next: View) => {
    const params = new URLSearchParams(searchParams);
    if (next === "list") params.delete("view");
    else params.set("view", next);
    const search = params.toString();
    router.replace(search ? `${pathname}?${search}` : pathname, {
      scroll: false,
    });
  };

  return (
    <Tabs value={view} onValueChange={(value) => handleViewChange(value as View)}>
      <TabsList className="mb-2">
        <TabsTab value="list">
          <ListIcon />
          List
        </TabsTab>
        <TabsTab value="calendar">
          <CalendarDaysIcon />
          Calendar
        </TabsTab>
      </TabsList>

      <TabsPanel value="list">
        <MaintenanceWindowsList
          organizationId={organizationId}
          listHref={listHref}
          monitorsHref={monitorsHref}
        />
      </TabsPanel>

      <TabsPanel value="calendar">
        {isClient ? (
          <MaintenanceOccurrencesCalendar
            organizationId={organizationId}
            listHref={listHref}
            monitorsHref={monitorsHref}
          />
        ) : (
          <Skeleton className="h-[42rem] w-full rounded-xl" />
        )}
      </TabsPanel>
    </Tabs>
  );
}
