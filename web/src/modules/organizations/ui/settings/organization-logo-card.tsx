"use client";

import { useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import {
  useRemoveOrganizationLogo,
  useUploadOrganizationLogo,
} from "../../hooks/use-organization-settings";
import { LOGO_ACCEPT, LOGO_MAX_BYTES, validateLogoFile } from "../../lib/logo";
import { canManageMembers } from "../../lib/roles";
import { OrganizationAvatar } from "../organization-avatar";

type OrganizationDetails = components["schemas"]["OrganizationDetails"];

export function OrganizationLogoCard({
  organization,
}: {
  organization: OrganizationDetails;
}) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [error, setError] = useState<string | null>(null);
  const uploadLogo = useUploadOrganizationLogo(organization.id);
  const removeLogo = useRemoveOrganizationLogo(organization.id);
  const editable = canManageMembers(organization.currentUserRole);
  const busy = uploadLogo.isPending || removeLogo.isPending;

  function openFilePicker() {
    inputRef.current?.click();
  }

  function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;

    const validationError = validateLogoFile(file);
    setError(validationError);
    if (!validationError) {
      uploadLogo.mutate(file);
    }
  }

  const avatar = (
    <span className="relative block">
      <OrganizationAvatar
        name={organization.name}
        logoUrl={organization.logoUrl}
        className="size-20 rounded-xl border bg-muted"
        fallbackClassName="bg-muted font-heading text-2xl text-muted-foreground"
      />
      {busy ? (
        <span className="absolute inset-0 z-20 flex items-center justify-center rounded-xl bg-black/48 text-white">
          <Spinner className="size-6" />
        </span>
      ) : null}
    </span>
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Logo</CardTitle>
        <CardDescription>
          {editable
            ? "Shown in the sidebar and on your status pages."
            : "Only admins and owners can change the logo."}
        </CardDescription>
      </CardHeader>
      <CardPanel className="flex flex-wrap items-center gap-5">
        {editable ? (
          <button
            type="button"
            onClick={openFilePicker}
            disabled={busy}
            aria-label={organization.logoUrl ? "Change logo" : "Upload logo"}
            className="rounded-xl outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
          >
            {avatar}
          </button>
        ) : (
          avatar
        )}

        {editable ? (
          <div className="flex flex-col gap-2">
            <div className="flex items-center gap-2">
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={openFilePicker}
                disabled={busy}
              >
                {organization.logoUrl ? "Change logo" : "Upload logo"}
              </Button>
              {organization.logoUrl ? (
                <Button
                  type="button"
                  size="sm"
                  variant="ghost"
                  onClick={() => {
                    setError(null);
                    removeLogo.mutate();
                  }}
                  disabled={busy}
                >
                  Remove
                </Button>
              ) : null}
            </div>
            <p
              className={cn(
                "text-xs",
                error ? "text-destructive-foreground" : "text-muted-foreground",
              )}
            >
              {error ??
                `PNG, JPG or WebP, up to ${LOGO_MAX_BYTES / 1024 / 1024} MB.`}
            </p>
            <input
              ref={inputRef}
              type="file"
              accept={LOGO_ACCEPT}
              className="sr-only"
              tabIndex={-1}
              aria-hidden
              onChange={handleFileChange}
            />
          </div>
        ) : null}
      </CardPanel>
    </Card>
  );
}
