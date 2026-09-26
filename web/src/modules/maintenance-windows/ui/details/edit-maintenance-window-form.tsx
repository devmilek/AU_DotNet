"use client";

import Link from "next/link";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import type { components } from "@/lib/api/schema";
import type { MaintenanceWindowRef } from "../../hooks/keys";
import { useUpdateMaintenanceWindow } from "../../hooks/use-maintenance-window";
import { useAppForm } from "../../lib/form";
import { type ServerError, toServerError } from "../../lib/server-error";
import {
  diffMaintenanceWindow,
  hasChanges,
  maintenanceWindowFormOptions,
  maintenanceWindowToFormValues,
} from "../../schemas/create-maintenance-window";
import { MaintenanceWindowFields } from "../form/maintenance-window-fields";
import { ServerErrorAlert } from "../form/server-error-alert";

type MaintenanceWindow = components["schemas"]["MaintenanceWindowResponse"];

export function EditMaintenanceWindowForm({
  windowRef,
  maintenanceWindow,
  cancelHref,
  onSaved,
}: {
  windowRef: MaintenanceWindowRef;
  maintenanceWindow: MaintenanceWindow;
  cancelHref: string;
  onSaved: () => void;
}) {
  const [initialValues] = useState(() =>
    maintenanceWindowToFormValues(maintenanceWindow),
  );
  const [applyToPastOccurrences, setApplyToPastOccurrences] = useState(false);
  const [serverError, setServerError] = useState<ServerError | null>(null);
  const updateWindow = useUpdateMaintenanceWindow(windowRef);

  const form = useAppForm({
    ...maintenanceWindowFormOptions,
    defaultValues: initialValues,
    onSubmit: async ({ value }) => {
      const changes = diffMaintenanceWindow(initialValues, value);
      if (!hasChanges(changes)) return;

      setServerError(null);
      try {
        await updateWindow.mutateAsync({ changes, applyToPastOccurrences });
        onSaved();
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
        organizationId={windowRef.organizationId}
        minDate=""
        notice={<ServerErrorAlert error={serverError} />}
        generalExtra={
          <form.Subscribe
            selector={(state) =>
              diffMaintenanceWindow(initialValues, state.values).content !==
              null
            }
          >
            {(contentChanged) =>
              contentChanged ? (
                <Label className="flex items-start gap-3 rounded-lg border bg-muted/40 p-3">
                  <Checkbox
                    className="mt-0.5"
                    checked={applyToPastOccurrences}
                    onCheckedChange={setApplyToPastOccurrences}
                  />
                  <span className="flex flex-col gap-0.5">
                    <span className="font-medium">
                      Also update past occurrences
                    </span>
                    <span className="font-normal text-muted-foreground text-xs">
                      Changes what customers see in the history on your status
                      page. Upcoming occurrences are always updated, except the
                      ones you edited separately.
                    </span>
                  </span>
                </Label>
              ) : null
            }
          </form.Subscribe>
        }
        actions={
          <form.Subscribe
            selector={(state) => ({
              changed: hasChanges(
                diffMaintenanceWindow(initialValues, state.values),
              ),
              isSubmitting: state.isSubmitting,
            })}
          >
            {({ changed, isSubmitting }) => (
              <div className="flex items-center justify-end gap-2">
                {changed ? (
                  <span className="me-auto text-muted-foreground text-sm">
                    Unsaved changes
                  </span>
                ) : null}
                <Button variant="ghost" render={<Link href={cancelHref} />}>
                  Cancel
                </Button>
                <Button type="submit" disabled={!changed} loading={isSubmitting}>
                  Save changes
                </Button>
              </div>
            )}
          </form.Subscribe>
        }
      />
    </form>
  );
}
