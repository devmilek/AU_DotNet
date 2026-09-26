"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { CircleAlertIcon } from "lucide-react";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import {
  type SignUpValues,
  signUpSchema,
} from "@/modules/auth/schemas/sign-up";
import { api } from "@/lib/api/client";
import { useRouter } from "next/navigation";

export function SignUpForm({ defaultEmail = "" }: { defaultEmail?: string }) {
  const defaultValues: SignUpValues = {
    name: "",
    email: defaultEmail,
    password: "",
  };

  const [formError, setFormError] = useState<string | null>(null);
  const router = useRouter();

  const form = useForm({
    defaultValues,
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: {
      onDynamic: signUpSchema,
    },
    onSubmit: async ({ value }) => {
      setFormError(null);
      const { response } = await api.POST("/api/auth/register", {
        body: {
          displayName: value.name,
          email: value.email,
          password: value.password,
        },
      });

      if (response.ok) {
        router.push(`/confirm-email?email=${value.email}`);
      }

      setFormError(
        response.statusText ?? "Something went wrong. Please try again.",
      );
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
            <FieldLabel>Name</FieldLabel>
            <Input
              type="text"
              name={field.name}
              value={field.state.value}
              onValueChange={field.handleChange}
              onBlur={field.handleBlur}
              placeholder="Jane Doe"
              autoComplete="name"
              aria-invalid={!field.state.meta.isValid || undefined}
            />
            <FieldError match={!field.state.meta.isValid}>
              {formatFieldErrors(field.state.meta.errors)}
            </FieldError>
          </Field>
        )}
      </form.Field>

      <form.Field name="email">
        {(field) => (
          <Field
            name={field.name}
            invalid={!field.state.meta.isValid}
            dirty={field.state.meta.isDirty}
            touched={field.state.meta.isTouched}
          >
            <FieldLabel>Email</FieldLabel>
            <Input
              type="email"
              name={field.name}
              value={field.state.value}
              onValueChange={field.handleChange}
              onBlur={field.handleBlur}
              placeholder="m@example.com"
              autoComplete="email"
              aria-invalid={!field.state.meta.isValid || undefined}
            />
            <FieldError match={!field.state.meta.isValid}>
              {formatFieldErrors(field.state.meta.errors)}
            </FieldError>
          </Field>
        )}
      </form.Field>

      <form.Field name="password">
        {(field) => (
          <Field
            name={field.name}
            invalid={!field.state.meta.isValid}
            dirty={field.state.meta.isDirty}
            touched={field.state.meta.isTouched}
          >
            <FieldLabel>Password</FieldLabel>
            <Input
              type="password"
              name={field.name}
              value={field.state.value}
              onValueChange={field.handleChange}
              onBlur={field.handleBlur}
              placeholder="••••••••"
              autoComplete="new-password"
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
            Create account
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
