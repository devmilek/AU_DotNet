"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { CircleAlertIcon } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { api } from "@/lib/api/client";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import { organizationKeys } from "@/modules/organizations/hooks/keys";
import {
  type CreateOrganizationValues,
  createOrganizationSchema,
} from "@/modules/organizations/schemas/create-organization";

const defaultValues: CreateOrganizationValues = {
  name: "",
};

export function CreateOrganizationForm() {
  const [formError, setFormError] = useState<string | null>(null);
  const router = useRouter();
  const queryClient = useQueryClient();

  const createOrganization = useMutation({
    mutationFn: async (name: string) => {
      const { data, response, error } = await api.POST("/api/organizations", {
        body: { name },
      });

      if (!response.ok || !data) {
        throw new Error(error?.title ?? "Nie udało się utworzyć organizacji.");
      }

      return data;
    },
    onSuccess: async (organization) => {
      await queryClient.invalidateQueries({ queryKey: organizationKeys.all });
      router.push(`/${organization.slug}/monitors`);
      router.refresh();
    },
  });

  const form = useForm({
    defaultValues,
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: {
      onDynamic: createOrganizationSchema,
    },
    onSubmit: async ({ value }) => {
      setFormError(null);
      try {
        await createOrganization.mutateAsync(value.name);
      } catch (error) {
        setFormError(
          error instanceof Error
            ? error.message
            : "Something went wrong. Please try again.",
        );
      }
    },
  });

  return (
    <form
      className="flex w-full flex-col gap-4"
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        event.stopPropagation();
        void form.handleSubmit();
      }}
    >
      {formError ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertDescription>{formError}</AlertDescription>
        </Alert>
      ) : null}

      <form.Field name="name">
        {(field) => (
          <Field
            name={field.name}
            invalid={!field.state.meta.isValid}
            dirty={field.state.meta.isDirty}
            touched={field.state.meta.isTouched}
          >
            <FieldLabel>Organization name</FieldLabel>
            <Input
              name={field.name}
              value={field.state.value}
              onValueChange={field.handleChange}
              onBlur={field.handleBlur}
              placeholder="Acme Inc"
              autoComplete="organization"
              aria-invalid={!field.state.meta.isValid || undefined}
            />
            <FieldError match={!field.state.meta.isValid}>
              {formatFieldErrors(field.state.meta.errors)}
            </FieldError>
          </Field>
        )}
      </form.Field>

      <form.Subscribe selector={(state) => state.isSubmitting}>
        {(isSubmitting) => (
          <Button className="w-full" type="submit" loading={isSubmitting}>
            Create organization
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
