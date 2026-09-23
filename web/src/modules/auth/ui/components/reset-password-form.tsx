"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { CircleAlertIcon, CircleCheckIcon } from "lucide-react";
import Link from "next/link";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import {
  type ResetPasswordValues,
  resetPasswordSchema,
} from "@/modules/auth/schemas/reset-password";

const defaultValues: ResetPasswordValues = {
  password: "",
  confirmPassword: "",
};

type ResetPasswordFormProps = {
  token: string;
};

export function ResetPasswordForm({ token }: ResetPasswordFormProps) {
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitted, setIsSubmitted] = useState(false);

  const form = useForm({
    defaultValues,
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: {
      onDynamic: resetPasswordSchema,
    },
    onSubmit: async ({ value }) => {
      setFormError(null);

      try {
        // TODO: replace with real reset-password action
        await new Promise((resolve) => setTimeout(resolve, 800));
        console.log("reset-password", { token, ...value });
        setIsSubmitted(true);
      } catch {
        setFormError("Something went wrong. Please try again.");
      }
    },
  });

  if (isSubmitted) {
    return (
      <div className="flex flex-col gap-4">
        <Alert variant="success">
          <CircleCheckIcon />
          <AlertDescription>
            Your password has been updated. You can now sign in with your new
            password.
          </AlertDescription>
        </Alert>
        <Button className="w-full" render={<Link href="/sign-in" />}>
          Back to login
        </Button>
      </div>
    );
  }

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

      <form.Field name="password">
        {(field) => (
          <Field
            name={field.name}
            invalid={!field.state.meta.isValid}
            dirty={field.state.meta.isDirty}
            touched={field.state.meta.isTouched}
          >
            <FieldLabel>New password</FieldLabel>
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

      <form.Field name="confirmPassword">
        {(field) => (
          <Field
            name={field.name}
            invalid={!field.state.meta.isValid}
            dirty={field.state.meta.isDirty}
            touched={field.state.meta.isTouched}
          >
            <FieldLabel>Confirm new password</FieldLabel>
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
            Reset password
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
