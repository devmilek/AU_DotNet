"use client";

import {
  AlertDialog,
  AlertDialogClose,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogPopup,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import type { MaintenanceWindowRef } from "../../hooks/keys";
import { useCancelOccurrence } from "../../hooks/use-maintenance-window";
import { formatDate, formatTimeRange } from "../../lib/format";
import type { WindowOccurrence } from "../../lib/occurrence";

export function CancelOccurrenceDialog({
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
  const cancelOccurrence = useCancelOccurrence(windowRef);
  const start = occurrence ? new Date(occurrence.startsAtUtc) : null;
  const end = occurrence ? new Date(occurrence.endsAtUtc) : null;

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogPopup>
        <AlertDialogHeader>
          <AlertDialogTitle>Cancel this occurrence?</AlertDialogTitle>
          <AlertDialogDescription>
            {start && end ? (
              <>
                On{" "}
                <span className="font-medium text-foreground">
                  {formatDate(start, timeZone, {
                    weekday: "long",
                    day: "numeric",
                    month: "long",
                  })}
                  , {formatTimeRange(start, end, timeZone)}
                </span>{" "}
                monitors won’t be in maintenance — alerts and uptime work as
                usual. You can restore it until it starts.
              </>
            ) : null}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogClose render={<Button variant="ghost" />}>
            Keep it
          </AlertDialogClose>
          <Button
            variant="destructive"
            loading={cancelOccurrence.isPending}
            onClick={() => {
              if (!occurrence) return;
              cancelOccurrence.mutate(occurrence.id, {
                onSuccess: () => onOpenChange(false),
              });
            }}
          >
            Cancel occurrence
          </Button>
        </AlertDialogFooter>
      </AlertDialogPopup>
    </AlertDialog>
  );
}
