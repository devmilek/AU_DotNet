"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
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
import { toastManager } from "@/components/ui/toast";
import type { components } from "@/lib/api/schema";
import { organizationKeys } from "../../hooks/keys";
import { useRemoveMember } from "../../hooks/use-organization-settings";

type Member = components["schemas"]["MemberRow"];

export function RemoveMemberDialog({
  organizationId,
  organizationName,
  member,
  open,
  onOpenChange,
}: {
  organizationId: string;
  organizationName: string;
  member: Member | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const removeMember = useRemoveMember(organizationId);
  const leaving = member?.isCurrentUser ?? false;
  const name = member ? member.displayName || member.email : "";

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogPopup>
        <AlertDialogHeader>
          <AlertDialogTitle>
            {leaving ? `Leave ${organizationName}?` : `Remove ${name}?`}
          </AlertDialogTitle>
          <AlertDialogDescription>
            {leaving
              ? "You’ll lose access to its monitors and settings. You’ll need a new invitation to come back."
              : `${name} will lose access to ${organizationName} right away. You can invite them again later.`}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogClose render={<Button variant="ghost" />}>
            Cancel
          </AlertDialogClose>
          <Button
            variant="destructive"
            loading={removeMember.isPending}
            onClick={() => {
              if (!member) return;
              removeMember.mutate(member.userId, {
                onSuccess: () => {
                  onOpenChange(false);
                  if (leaving) {
                    queryClient.removeQueries({ queryKey: organizationKeys.all });
                    toastManager.add({
                      type: "success",
                      title: `You left ${organizationName}`,
                    });
                    router.replace("/");
                    router.refresh();
                  } else {
                    toastManager.add({
                      type: "success",
                      title: `${name} was removed`,
                    });
                  }
                },
              });
            }}
          >
            {leaving ? "Leave organization" : "Remove member"}
          </Button>
        </AlertDialogFooter>
      </AlertDialogPopup>
    </AlertDialog>
  );
}
