"use client";

import { useQueryClient } from "@tanstack/react-query";
import { TriangleAlertIcon } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState } from "react";
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
  DialogTrigger,
} from "@/components/ui/dialog";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { toastManager } from "@/components/ui/toast";
import { organizationKeys } from "../../hooks/keys";
import { useDeleteOrganization } from "../../hooks/use-organization-settings";

const deletedData = [
  "Monitors and their check history",
  "Incidents and uptime statistics",
  "Notification channels",
  "Maintenance windows",
  "Members and pending invitations",
];

export function DeleteOrganizationDialog({
  organizationId,
  organizationName,
}: {
  organizationId: string;
  organizationName: string;
}) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const deleteOrganization = useDeleteOrganization(organizationId);
  const [open, setOpen] = useState(false);
  const [confirmation, setConfirmation] = useState("");
  const matches = confirmation.trim() === organizationName;

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (!next) {
          setConfirmation("");
          deleteOrganization.reset();
        }
      }}
    >
      <DialogTrigger render={<Button variant="destructive" />}>
        Delete organization
      </DialogTrigger>
      <DialogPopup className="sm:max-w-md">
        <form
          className="contents"
          onSubmit={(event) => {
            event.preventDefault();
            if (!matches) return;
            deleteOrganization.mutate(confirmation.trim(), {
              onSuccess: () => {
                queryClient.removeQueries({ queryKey: organizationKeys.all });
                toastManager.add({
                  type: "success",
                  title: `${organizationName} was deleted`,
                });
                router.replace("/");
                router.refresh();
              },
            });
          }}
        >
          <DialogHeader>
            <DialogTitle>Delete {organizationName}?</DialogTitle>
            <DialogDescription>
              This permanently deletes the organization and everything in it.
              It can’t be undone.
            </DialogDescription>
          </DialogHeader>

          <DialogPanel className="grid gap-4">
            <Alert variant="error">
              <TriangleAlertIcon />
              <AlertDescription>
                <ul className="list-disc ps-4">
                  {deletedData.map((item) => (
                    <li key={item}>{item}</li>
                  ))}
                </ul>
              </AlertDescription>
            </Alert>

            <Field>
              <FieldLabel>
                <span>
                  To confirm, type{" "}
                  <span className="select-all font-semibold">
                    {organizationName}
                  </span>
                </span>
              </FieldLabel>
              <Input
                value={confirmation}
                onValueChange={setConfirmation}
                autoComplete="off"
                spellCheck={false}
                aria-invalid={deleteOrganization.isError || undefined}
              />
            </Field>

            {deleteOrganization.isError ? (
              <p className="text-destructive-foreground text-sm">
                {deleteOrganization.error.message}
              </p>
            ) : null}
          </DialogPanel>

          <DialogFooter>
            <DialogClose render={<Button variant="ghost" />}>Cancel</DialogClose>
            <Button
              type="submit"
              variant="destructive"
              disabled={!matches}
              loading={deleteOrganization.isPending}
            >
              Delete organization
            </Button>
          </DialogFooter>
        </form>
      </DialogPopup>
    </Dialog>
  );
}
