"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { toastManager } from "@/components/ui/toast";
import { useIsClient } from "@/hooks/use-is-client";
import { useCreateMaintenanceWindow } from "../../hooks/use-maintenance-windows";
import { useAppForm } from "../../lib/form";
import { type ServerError, toServerError } from "../../lib/server-error";
import { localDateKey } from "../../lib/wall-clock";
import {
  createMaintenanceWindowDefaults,
  maintenanceWindowFormOptions,
  toCreateMaintenanceWindowRequest,
} from "../../schemas/create-maintenance-window";
import {
  MaintenanceWindowFields,
  MaintenanceWindowFieldsSkeleton,
} from "./maintenance-window-fields";
import { ServerErrorAlert } from "./server-error-alert";

type CreateMaintenanceWindowFormProps = {
  organizationId: string;
  organizationSlug: string;
};

export function CreateMaintenanceWindowForm(
  props: CreateMaintenanceWindowFormProps,
) {
  const isClient = useIsClient();

  return isClient ? (
    <CreateMaintenanceWindowFormContent {...props} />
  ) : (
    <MaintenanceWindowFieldsSkeleton />
  );
}

function CreateMaintenanceWindowFormContent({
  organizationId,
  organizationSlug,
}: CreateMaintenanceWindowFormProps) {
  const router = useRouter();
  const createWindow = useCreateMaintenanceWindow(organizationId);
  const [defaultValues] = useState(() => createMaintenanceWindowDefaults());
  const [today] = useState(() => localDateKey(new Date()));
  const [serverError, setServerError] = useState<ServerError | null>(null);
  const listHref = `/${organizationSlug}/maintenance-windows`;

  const form = useAppForm({
    ...maintenanceWindowFormOptions,
    defaultValues,
    onSubmit: async ({ value }) => {
      setServerError(null);
      try {
        const windowId = await createWindow.mutateAsync(
          toCreateMaintenanceWindowRequest(value),
        );
        toastManager.add({
          type: "success",
          title: "Maintenance window created",
        });
        router.push(`${listHref}/${windowId}`);
      } catch (error) {
        setServerError(toServerError(error));
      }
    },
  });

  return (
    <form
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        event.stopPropagation();
        void form.handleSubmit();
      }}
    >
      <MaintenanceWindowFields
        form={form}
        organizationId={organizationId}
        minDate={today}
        notice={<ServerErrorAlert error={serverError} />}
        generalExtra={null}
        actions={
          <div className="flex justify-end gap-2">
            <Button variant="ghost" render={<Link href={listHref} />}>
              Cancel
            </Button>
            <form.Subscribe selector={(state) => state.isSubmitting}>
              {(isSubmitting) => (
                <Button type="submit" loading={isSubmitting}>
                  Create maintenance window
                </Button>
              )}
            </form.Subscribe>
          </div>
        }
      />
    </form>
  );
}
