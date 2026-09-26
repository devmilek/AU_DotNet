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
import { useDeleteMaintenanceWindow } from "../hooks/use-maintenance-windows";

export function DeleteMaintenanceWindowDialog({
  organizationId,
  maintenanceWindow,
  open,
  onOpenChange,
  onDeleted,
}: {
  organizationId: string;
  maintenanceWindow: { id: string; name: string } | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onDeleted?: () => void;
}) {
  const deleteWindow = useDeleteMaintenanceWindow(organizationId);

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogPopup>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete “{maintenanceWindow?.name}”?</AlertDialogTitle>
          <AlertDialogDescription>
            Upcoming occurrences are removed and an occurrence in progress ends
            now. Past occurrences stay in the history of your monitors.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogClose render={<Button variant="ghost" />}>
            Cancel
          </AlertDialogClose>
          <Button
            variant="destructive"
            loading={deleteWindow.isPending}
            onClick={() => {
              if (!maintenanceWindow) return;
              deleteWindow.mutate(maintenanceWindow.id, {
                onSuccess: () => {
                  onOpenChange(false);
                  onDeleted?.();
                },
              });
            }}
          >
            Delete window
          </Button>
        </AlertDialogFooter>
      </AlertDialogPopup>
    </AlertDialog>
  );
}
