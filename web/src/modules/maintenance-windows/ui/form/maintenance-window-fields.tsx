import type React from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { withForm } from "../../lib/form";
import type { DateKey } from "../../lib/wall-clock";
import { maintenanceWindowFormOptions } from "../../schemas/create-maintenance-window";
import { BehaviorSection } from "./behavior-section";
import { GeneralSection } from "./general-section";
import { MonitorsSection } from "./monitors-section";
import { RecurrenceSection } from "./recurrence-section";
import { ScheduleSection } from "./schedule-section";
import { SchedulePreview } from "./schedule-preview";

export const MaintenanceWindowFields = withForm({
  ...maintenanceWindowFormOptions,
  props: {
    organizationId: "",
    minDate: "" as DateKey,
    notice: null as React.ReactNode,
    generalExtra: null as React.ReactNode,
    actions: null as React.ReactNode,
  },
  render: function Render({
    form,
    organizationId,
    minDate,
    notice,
    generalExtra,
    actions,
  }) {
    return (
      <div className="grid gap-6 lg:grid-cols-[minmax(0,26rem)_minmax(0,1fr)] lg:items-start">
        <div className="flex min-w-0 flex-col gap-6">
          {notice}
          <GeneralSection form={form} extra={generalExtra} />
          <ScheduleSection form={form} minDate={minDate} />
          <RecurrenceSection form={form} />
        </div>

        <div className="min-w-0 lg:sticky lg:top-4 lg:col-start-2 lg:row-span-2 lg:row-start-1">
          <form.Subscribe
            selector={(state) => ({
              schedule: state.values.schedule,
              name: state.values.name,
            })}
          >
            {({ schedule, name }) => (
              <SchedulePreview values={schedule} name={name} />
            )}
          </form.Subscribe>
        </div>

        <div className="flex min-w-0 flex-col gap-6 lg:col-start-1">
          <MonitorsSection form={form} organizationId={organizationId} />
          <BehaviorSection form={form} />
          {actions}
        </div>
      </div>
    );
  },
});

export function MaintenanceWindowFieldsSkeleton() {
  return (
    <div className="grid gap-6 lg:grid-cols-[minmax(0,26rem)_minmax(0,1fr)]">
      <div className="flex flex-col gap-6">
        <Skeleton className="h-56 w-full rounded-xl" />
        <Skeleton className="h-80 w-full rounded-xl" />
        <Skeleton className="h-64 w-full rounded-xl" />
      </div>
      <Skeleton className="h-[44rem] w-full rounded-xl" />
    </div>
  );
}
