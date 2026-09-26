"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { CircleAlertIcon } from "lucide-react";
import Link from "next/link";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import {
  type SignInValues,
  signInSchema,
} from "@/modules/auth/schemas/sign-in";
import { api } from "@/lib/api/client";
import { useRouter } from "next/navigation";

const defaultValues: SignInValues = {
  email: "",
  password: "",
};

export function SignInForm({ returnUrl }: { returnUrl?: string | null }) {
  const [formError, setFormError] = useState<string | null>(null);
  const router = useRouter();
  const form = useForm({
    defaultValues,
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: {
      onDynamic: signInSchema,
    },
    onSubmit: async ({ value }) => {
      setFormError(null);

      const { response, error } = await api.POST("/api/auth/login", {
        body: {
          email: value.email,
          password: value.password,
          rememberMe: false,
        },
      });

      if (response.ok) {
        router.push(returnUrl ?? "/");
        return;
      }

      setFormError(error?.title ?? "Something went wrong. Please try again.");
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
            <div className="flex w-full items-center justify-between gap-2">
              <FieldLabel>Password</FieldLabel>
              <Link
                href="/forgot-password"
                className="text-muted-foreground text-xs hover:text-foreground"
              >
                Forgot password?
              </Link>
            </div>
            <Input
              type="password"
              name={field.name}
              value={field.state.value}
              onValueChange={field.handleChange}
              onBlur={field.handleBlur}
              placeholder="••••••••"
              autoComplete="current-password"
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
            Login
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
