import type { AnyFieldApi } from "@tanstack/react-form";
import type React from "react";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { cn } from "@/lib/utils";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";

export function FieldShell({
  field,
  label,
  description,
  hideLabel,
  className,
  children,
}: {
  field: AnyFieldApi;
  label?: React.ReactNode;
  description?: React.ReactNode;
  hideLabel?: boolean;
  className?: string;
  children: React.ReactNode;
}) {
  const invalid = !field.state.meta.isValid;

  return (
    <Field
      name={field.name}
      invalid={invalid}
      dirty={field.state.meta.isDirty}
      touched={field.state.meta.isTouched}
      className={className}
    >
      {label ? (
        <FieldLabel className={cn(hideLabel && "sr-only")}>{label}</FieldLabel>
      ) : null}
      {children}
      {description ? <FieldDescription>{description}</FieldDescription> : null}
      <FieldError match={invalid}>
        {formatFieldErrors(field.state.meta.errors)}
      </FieldError>
    </Field>
  );
}

export function InlineFieldError({ field }: { field: AnyFieldApi }) {
  if (field.state.meta.isValid) return null;

  return (
    <p className="w-full text-destructive-foreground text-xs">
      {formatFieldErrors(field.state.meta.errors)}
    </p>
  );
}
