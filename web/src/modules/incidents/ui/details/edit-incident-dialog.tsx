"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { CircleAlertIcon } from "lucide-react";
import { z } from "zod";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogPanel,
  DialogPopup,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import type { components } from "@/lib/api/schema";
import { FieldShell } from "@/modules/maintenance-windows/ui/form/field-shell";
import type { IncidentRef } from "../../hooks/keys";
import { useUpdateIncident } from "../../hooks/use-incidents";

type Incident = components["schemas"]["IncidentResponse"];

const MAX_NAME_LENGTH = 200;
const MAX_CAUSE_LENGTH = 1000;

const editIncidentSchema = z.object({
  name: z
    .string()
    .max(MAX_NAME_LENGTH, `Name must be at most ${MAX_NAME_LENGTH} characters.`),
  cause: z
    .string()
    .max(MAX_CAUSE_LENGTH, `Cause must be at most ${MAX_CAUSE_LENGTH} characters.`),
});

export function EditIncidentDialog({
  incidentRef,
  incident,
  open,
  onOpenChange,
}: {
  incidentRef: IncidentRef;
  incident: Incident;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogPopup className="sm:max-w-lg">
        {open ? (
          <EditIncidentForm
            incidentRef={incidentRef}
            incident={incident}
            onDone={() => onOpenChange(false)}
          />
        ) : null}
      </DialogPopup>
    </Dialog>
  );
}

function EditIncidentForm({
  incidentRef,
  incident,
  onDone,
}: {
  incidentRef: IncidentRef;
  incident: Incident;
  onDone: () => void;
}) {
  const updateIncident = useUpdateIncident(incidentRef);
  const defaultName = `${incident.monitor.name} is down`;

  const form = useForm({
    defaultValues: {
      name: incident.hasCustomName ? incident.name : "",
      cause: incident.cause ?? "",
    },
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: { onDynamic: editIncidentSchema },
    onSubmit: async ({ value }) => {
      await updateIncident.mutateAsync({
        name: value.name.trim() || null,
        cause: value.cause.trim() || null,
      });
      onDone();
    },
  });

  return (
    <form
      noValidate
      className="contents"
      onSubmit={(event) => {
        event.preventDefault();
        event.stopPropagation();
        void form.handleSubmit();
      }}
    >
      <DialogHeader>
        <DialogTitle>Edit incident</DialogTitle>
        <DialogDescription>
          Give the incident a clear name and describe what caused it.
        </DialogDescription>
      </DialogHeader>

      <DialogPanel className="grid gap-4">
        {updateIncident.isError ? (
          <Alert variant="error">
            <CircleAlertIcon />
            <AlertDescription>{updateIncident.error.message}</AlertDescription>
          </Alert>
        ) : null}

        <form.Field name="name">
          {(field) => (
            <FieldShell
              field={field}
              label="Name"
              description="Leave empty to use the default name."
            >
              <Input
                name={field.name}
                value={field.state.value}
                onValueChange={field.handleChange}
                onBlur={field.handleBlur}
                placeholder={defaultName}
                autoComplete="off"
                aria-invalid={!field.state.meta.isValid || undefined}
              />
            </FieldShell>
          )}
        </form.Field>

        <form.Field name="cause">
          {(field) => (
            <FieldShell
              field={field}
              label="Cause"
              description="Filled in automatically from the failed check. Replace it with the root cause once you know it."
            >
              <Textarea
                name={field.name}
                rows={4}
                value={field.state.value}
                onChange={(event) => field.handleChange(event.target.value)}
                onBlur={field.handleBlur}
                placeholder="What went wrong?"
                aria-invalid={!field.state.meta.isValid || undefined}
              />
            </FieldShell>
          )}
        </form.Field>
      </DialogPanel>

      <DialogFooter>
        <DialogClose render={<Button variant="ghost" />}>Cancel</DialogClose>
        <form.Subscribe selector={(state) => state.isSubmitting}>
          {(isSubmitting) => (
            <Button type="submit" loading={isSubmitting}>
              Save
            </Button>
          )}
        </form.Subscribe>
      </DialogFooter>
    </form>
  );
}
