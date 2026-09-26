"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { HistoryIcon, RotateCcwIcon } from "lucide-react";
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
import { useNow } from "@/hooks/use-now";
import type { MaintenanceWindowRef } from "../../hooks/keys";
import {
  useResetOccurrenceContent,
  useUpdateOccurrenceContent,
} from "../../hooks/use-maintenance-window";
import { formatDate, formatTimeRange } from "../../lib/format";
import { hasStarted, type WindowOccurrence } from "../../lib/occurrence";
import { occurrenceContentSchema } from "../../schemas/occurrence-content";
import { FieldShell } from "../form/field-shell";

export function EditOccurrenceDialog({
  windowRef,
  occurrence,
  timeZone,
  open,
  onOpenChange,
}: {
  windowRef: MaintenanceWindowRef;
  occurrence: WindowOccurrence | null;
  timeZone: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogPopup className="sm:max-w-lg">
        {occurrence ? (
          <EditOccurrenceForm
            key={occurrence.id}
            windowRef={windowRef}
            occurrence={occurrence}
            timeZone={timeZone}
            onDone={() => onOpenChange(false)}
          />
        ) : null}
      </DialogPopup>
    </Dialog>
  );
}

function EditOccurrenceForm({
  windowRef,
  occurrence,
  timeZone,
  onDone,
}: {
  windowRef: MaintenanceWindowRef;
  occurrence: WindowOccurrence;
  timeZone: string;
  onDone: () => void;
}) {
  const now = useNow(60_000);
  const updateContent = useUpdateOccurrenceContent(windowRef);
  const resetContent = useResetOccurrenceContent(windowRef);
  const started = hasStarted(occurrence, now);
  const start = new Date(occurrence.startsAtUtc);
  const end = new Date(occurrence.endsAtUtc);

  const form = useForm({
    defaultValues: {
      name: occurrence.name,
      description: occurrence.description ?? "",
    },
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: { onDynamic: occurrenceContentSchema },
    onSubmit: async ({ value }) => {
      await updateContent.mutateAsync({
        occurrenceId: occurrence.id,
        name: value.name.trim(),
        description: value.description.trim() || null,
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
        <DialogTitle>Edit occurrence</DialogTitle>
        <DialogDescription className="tabular-nums">
          {formatDate(start, timeZone, {
            weekday: "long",
            day: "numeric",
            month: "long",
            year: "numeric",
          })}
          {" · "}
          {formatTimeRange(start, end, timeZone)}
        </DialogDescription>
      </DialogHeader>

      <DialogPanel className="grid gap-4">
        {started ? (
          <Alert variant="warning">
            <HistoryIcon />
            <AlertDescription>
              This occurrence has already started. Your change rewrites what
              customers see in its history on the status page.
            </AlertDescription>
          </Alert>
        ) : (
          <p className="text-muted-foreground text-sm">
            Only this occurrence changes. Renaming the whole window later won’t
            overwrite it.
          </p>
        )}

        <form.Field name="name">
          {(field) => (
            <FieldShell field={field} label="Name">
              <Input
                name={field.name}
                value={field.state.value}
                onValueChange={field.handleChange}
                onBlur={field.handleBlur}
                autoComplete="off"
                aria-invalid={!field.state.meta.isValid || undefined}
              />
            </FieldShell>
          )}
        </form.Field>

        <form.Field name="description">
          {(field) => (
            <FieldShell field={field} label="Description">
              <Textarea
                name={field.name}
                rows={4}
                value={field.state.value}
                onChange={(event) => field.handleChange(event.target.value)}
                onBlur={field.handleBlur}
                aria-invalid={!field.state.meta.isValid || undefined}
              />
            </FieldShell>
          )}
        </form.Field>
      </DialogPanel>

      <DialogFooter className="sm:justify-between">
        {!occurrence.followsWindow && !started ? (
          <Button
            variant="ghost"
            loading={resetContent.isPending}
            onClick={() =>
              resetContent.mutate(occurrence.id, { onSuccess: onDone })
            }
          >
            <RotateCcwIcon />
            Use window’s content
          </Button>
        ) : (
          <span />
        )}
        <div className="flex gap-2">
          <DialogClose render={<Button variant="ghost" />}>Cancel</DialogClose>
          <form.Subscribe selector={(state) => state.isSubmitting}>
            {(isSubmitting) => (
              <Button type="submit" loading={isSubmitting}>
                Save
              </Button>
            )}
          </form.Subscribe>
        </div>
      </DialogFooter>
    </form>
  );
}
