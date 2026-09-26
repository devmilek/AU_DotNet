"use client";

import { useRouter } from "next/navigation";
import type { MaintenanceWindowRef } from "../../hooks/keys";
import { MaintenanceWindowFieldsSkeleton } from "../form/maintenance-window-fields";
import { EditMaintenanceWindowForm } from "./edit-maintenance-window-form";
import { BackLink, MaintenanceWindowLoader } from "./maintenance-window-loader";

export function EditMaintenanceWindowView({
  windowRef,
  detailsHref,
}: {
  windowRef: MaintenanceWindowRef;
  detailsHref: string;
}) {
  const router = useRouter();

  return (
    <MaintenanceWindowLoader
      windowRef={windowRef}
      backLink={<BackLink href={detailsHref} label="Back" />}
      fallback={<MaintenanceWindowFieldsSkeleton />}
    >
      {(maintenanceWindow) => (
        <div className="flex flex-col gap-6">
          <div className="flex flex-col items-start gap-3">
            <BackLink href={detailsHref} label={maintenanceWindow.name} />
            <div className="space-y-1">
              <h1 className="font-heading text-2xl">Edit maintenance window</h1>
              <p className="text-muted-foreground text-sm">
                Changes apply to upcoming occurrences. Past ones stay as they
                were unless you choose otherwise.
              </p>
            </div>
          </div>

          <EditMaintenanceWindowForm
            key={maintenanceWindow.updatedAt}
            windowRef={windowRef}
            maintenanceWindow={maintenanceWindow}
            cancelHref={detailsHref}
            onSaved={() => router.push(detailsHref)}
          />
        </div>
      )}
    </MaintenanceWindowLoader>
  );
}
