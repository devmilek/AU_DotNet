"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { SendIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import { useInviteMember } from "../../hooks/use-organization-settings";
import { type OrganizationRole, ROLE } from "../../lib/roles";
import { inviteMemberSchema } from "../../schemas/organization-settings";
import { RoleSelect } from "./role-select";

export function InviteMemberCard({
  organizationId,
  actorRole,
}: {
  organizationId: string;
  actorRole: OrganizationRole;
}) {
  const inviteMember = useInviteMember(organizationId);

  const form = useForm({
    defaultValues: { email: "", role: ROLE.Member as OrganizationRole },
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: { onDynamic: inviteMemberSchema },
    onSubmit: async ({ value, formApi }) => {
      const invited = await inviteMember
        .mutateAsync({ email: value.email.trim(), role: value.role })
        .then(
          () => true,
          () => false,
        );

      if (invited) formApi.reset();
    },
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Invite a member</CardTitle>
        <CardDescription>
          They’ll get an email with a link to join. Invitations expire after a
          few days.
        </CardDescription>
      </CardHeader>
      <CardPanel>
        <form
          noValidate
          className="grid gap-3 sm:grid-cols-[minmax(0,1fr)_11rem_auto] sm:items-start"
          onSubmit={(event) => {
            event.preventDefault();
            event.stopPropagation();
            void form.handleSubmit();
          }}
        >
          <form.Field name="email">
            {(field) => (
              <Field
                name={field.name}
                invalid={!field.state.meta.isValid}
                dirty={field.state.meta.isDirty}
                touched={field.state.meta.isTouched}
              >
                <FieldLabel className="sr-only">Email</FieldLabel>
                <Input
                  name={field.name}
                  type="email"
                  inputMode="email"
                  placeholder="teammate@company.com"
                  autoComplete="off"
                  value={field.state.value}
                  onValueChange={field.handleChange}
                  onBlur={field.handleBlur}
                  aria-invalid={!field.state.meta.isValid || undefined}
                />
                <FieldError match={!field.state.meta.isValid}>
                  {formatFieldErrors(field.state.meta.errors)}
                </FieldError>
              </Field>
            )}
          </form.Field>

          <form.Field name="role">
            {(field) => (
              <RoleSelect
                aria-label="Role"
                actorRole={actorRole}
                value={field.state.value}
                onValueChange={field.handleChange}
              />
            )}
          </form.Field>

          <form.Subscribe selector={(state) => state.isSubmitting}>
            {(isSubmitting) => (
              <Button type="submit" loading={isSubmitting}>
                <SendIcon />
                Send invite
              </Button>
            )}
          </form.Subscribe>

          {inviteMember.isError ? (
            <p className="text-destructive-foreground text-sm sm:col-span-3">
              {inviteMember.error.message}
            </p>
          ) : null}
        </form>
      </CardPanel>
    </Card>
  );
}
