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
import { useDeleteIncident } from "../hooks/use-incidents";

export function DeleteIncidentDialog({
  organizationId,
  incident,
  open,
  onOpenChange,
  onDeleted,
}: {
  organizationId: string;
  incident: { id: string; name: string } | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onDeleted?: () => void;
}) {
  const deleteIncident = useDeleteIncident(organizationId);

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogPopup>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete “{incident?.name}”?</AlertDialogTitle>
          <AlertDialogDescription>
            The incident and its notification history are removed permanently.
            Checks of the monitor stay untouched, so uptime doesn’t change.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogClose render={<Button variant="ghost" />}>
            Cancel
          </AlertDialogClose>
          <Button
            variant="destructive"
            loading={deleteIncident.isPending}
            onClick={() => {
              if (!incident) return;
              deleteIncident.mutate(incident.id, {
                onSuccess: () => {
                  onOpenChange(false);
                  onDeleted?.();
                },
              });
            }}
          >
            Delete incident
          </Button>
        </AlertDialogFooter>
      </AlertDialogPopup>
    </AlertDialog>
  );
}
