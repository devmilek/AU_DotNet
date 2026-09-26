"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { CircleAlertIcon } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardDescription,
  CardFooter,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import type { components } from "@/lib/api/schema";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import {
  useOrganization,
  useUpdateOrganization,
} from "../../hooks/use-organization-settings";
import { canManageMembers, ROLE } from "../../lib/roles";
import { organizationNameSchema } from "../../schemas/organization-settings";
import { DeleteOrganizationDialog } from "./delete-organization-dialog";
import { OrganizationLogoCard } from "./organization-logo-card";

type OrganizationDetails = components["schemas"]["OrganizationDetails"];

export function GeneralSettings({ organizationId }: { organizationId: string }) {
  const organization = useOrganization(organizationId);

  if (organization.isError) {
    return (
      <Alert variant="error">
        <CircleAlertIcon />
        <AlertDescription>{organization.error.message}</AlertDescription>
      </Alert>
    );
  }

  if (!organization.data) {
    return (
      <div className="flex flex-col gap-6">
        <Skeleton className="h-40 w-full rounded-xl" />
        <Skeleton className="h-56 w-full rounded-xl" />
        <Skeleton className="h-36 w-full rounded-xl" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <OrganizationLogoCard organization={organization.data} />
      <OrganizationNameCard
        key={organization.data.name}
        organization={organization.data}
      />
      {organization.data.currentUserRole === ROLE.Owner ? (
        <DangerZone organization={organization.data} />
      ) : null}
    </div>
  );
}

function OrganizationNameCard({
  organization,
}: {
  organization: OrganizationDetails;
}) {
  const updateOrganization = useUpdateOrganization(organization.id);
  const editable = canManageMembers(organization.currentUserRole);

  const form = useForm({
    defaultValues: { name: organization.name },
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: { onDynamic: organizationNameSchema },
    onSubmit: async ({ value }) => {
      await updateOrganization.mutateAsync(value.name.trim());
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
      <Card>
        <CardHeader>
          <CardTitle>Organization</CardTitle>
          <CardDescription>
            {editable
              ? "How your organization appears to members and on emails."
              : "Only admins and owners can change these settings."}
          </CardDescription>
        </CardHeader>
        <CardPanel className="grid gap-4">
          <form.Field name="name">
            {(field) => (
              <Field
                name={field.name}
                invalid={!field.state.meta.isValid}
                dirty={field.state.meta.isDirty}
                touched={field.state.meta.isTouched}
              >
                <FieldLabel>Name</FieldLabel>
                <Input
                  name={field.name}
                  value={field.state.value}
                  onValueChange={field.handleChange}
                  onBlur={field.handleBlur}
                  disabled={!editable}
                  autoComplete="organization"
                  aria-invalid={!field.state.meta.isValid || undefined}
                />
                <FieldError match={!field.state.meta.isValid}>
                  {formatFieldErrors(field.state.meta.errors)}
                </FieldError>
              </Field>
            )}
          </form.Field>

          <Field>
            <FieldLabel>URL</FieldLabel>
            <Input value={`/${organization.slug}`} disabled readOnly />
            <FieldDescription>
              The address of your organization. It can’t be changed yet.
            </FieldDescription>
          </Field>

          {updateOrganization.isError ? (
            <p className="text-destructive-foreground text-sm">
              {updateOrganization.error.message}
            </p>
          ) : null}
        </CardPanel>
        {editable ? (
          <CardFooter className="justify-end gap-2">
            <form.Subscribe
              selector={(state) => ({
                changed: state.values.name.trim() !== organization.name,
                isSubmitting: state.isSubmitting,
              })}
            >
              {({ changed, isSubmitting }) => (
                <>
                  <Button
                    variant="ghost"
                    disabled={!changed || isSubmitting}
                    onClick={() => form.reset()}
                  >
                    Discard
                  </Button>
                  <Button type="submit" disabled={!changed} loading={isSubmitting}>
                    Save changes
                  </Button>
                </>
              )}
            </form.Subscribe>
          </CardFooter>
        ) : null}
      </Card>
    </form>
  );
}

function DangerZone({ organization }: { organization: OrganizationDetails }) {
  return (
    <Card className="border-destructive/32">
      <CardHeader>
        <CardTitle>Danger zone</CardTitle>
        <CardDescription>Irreversible actions for this organization.</CardDescription>
      </CardHeader>
      <CardPanel className="flex flex-wrap items-center justify-between gap-4">
        <div className="space-y-1 text-sm">
          <p className="font-medium">Delete organization</p>
          <p className="text-muted-foreground">
            Removes all monitors, history and members. You’ll need to type the
            organization name to confirm.
          </p>
        </div>
        <DeleteOrganizationDialog
          organizationId={organization.id}
          organizationName={organization.name}
        />
      </CardPanel>
    </Card>
  );
}
