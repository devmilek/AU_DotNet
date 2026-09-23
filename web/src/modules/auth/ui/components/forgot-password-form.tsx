"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { CircleAlertIcon, CircleCheckIcon } from "lucide-react";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import {
  type ForgotPasswordValues,
  forgotPasswordSchema,
} from "@/modules/auth/schemas/forgot-password";

const defaultValues: ForgotPasswordValues = {
  email: "",
};

export function ForgotPasswordForm() {
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [submittedEmail, setSubmittedEmail] = useState("");

  const form = useForm({
    defaultValues,
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: {
      onDynamic: forgotPasswordSchema,
    },
    onSubmit: async ({ value }) => {
      setFormError(null);

      try {
        // TODO: replace with real forgot-password action
        await new Promise((resolve) => setTimeout(resolve, 800));
        console.log("forgot-password", value);
        setSubmittedEmail(value.email);
        setIsSubmitted(true);
      } catch {
        setFormError("Something went wrong. Please try again.");
      }
    },
  });

  if (isSubmitted) {
    return (
      <Alert variant="success">
        <CircleCheckIcon />
        <AlertDescription>
          If an account exists for{" "}
          <span className="font-medium text-foreground">{submittedEmail}</span>,
          we&apos;ve sent a password reset link.
        </AlertDescription>
      </Alert>
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

      <form.Subscribe selector={(state) => state.isSubmitting}>
        {(isSubmitting) => (
          <Button className="w-full" type="submit" loading={isSubmitting}>
            Send reset link
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
