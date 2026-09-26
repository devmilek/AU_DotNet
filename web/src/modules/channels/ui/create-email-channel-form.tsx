"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { CircleAlertIcon, PlusIcon, Trash2Icon } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
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
import { ApiError } from "@/lib/api/errors";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import { useCreateChannel } from "@/modules/channels/hooks/use-channels";
import {
  createEmailChannelDefaults,
  createEmailChannelSchema,
  MAX_EMAIL_RECIPIENTS,
  toCreateChannelRequest,
} from "@/modules/channels/schemas/create-email-channel";
import { MonitorPicker } from "@/modules/monitors/ui/monitor-picker";

type ServerError = { title: string; details: string[] };

function toServerError(error: unknown): ServerError {
  if (!(error instanceof ApiError)) {
    return { title: "Something went wrong. Please try again.", details: [] };
  }

  const errors = (error.problem as { errors?: Record<string, string[]> })
    ?.errors;
  return {
    title:
      error.status === 403
        ? "You need admin permissions in this organization."
        : error.message,
    details: Object.values(errors ?? {}).flat(),
  };
}

export function CreateEmailChannelForm({
  organizationId,
  organizationSlug,
}: {
  organizationId: string;
  organizationSlug: string;
}) {
  const [serverError, setServerError] = useState<ServerError | null>(null);
  const router = useRouter();
  const createChannel = useCreateChannel(organizationId);
  const channelsHref = `/${organizationSlug}/notifications`;

  const form = useForm({
    defaultValues: createEmailChannelDefaults,
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: { onDynamic: createEmailChannelSchema },
    onSubmit: async ({ value }) => {
      setServerError(null);
      try {
        const channelId = await createChannel.mutateAsync(
          toCreateChannelRequest(value),
        );
        router.push(`${channelsHref}/${channelId}`);
      } catch (error) {
        setServerError(toServerError(error));
      }
    },
  });

  return (
    <form
      className="flex flex-col gap-6"
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        event.stopPropagation();
        void form.handleSubmit();
      }}
    >
      {serverError ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertTitle>{serverError.title}</AlertTitle>
          {serverError.details.length > 0 ? (
            <AlertDescription>
              <ul className="list-disc ps-4">
                {serverError.details.map((detail) => (
                  <li key={detail}>{detail}</li>
                ))}
              </ul>
            </AlertDescription>
          ) : null}
        </Alert>
      ) : null}

      <Card>
        <CardHeader>
          <CardTitle>General</CardTitle>
          <CardDescription>
            A name to recognise this channel, e.g. “On-call team”.
          </CardDescription>
        </CardHeader>
        <CardPanel>
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
                  placeholder="On-call team"
                  autoComplete="off"
                  aria-invalid={!field.state.meta.isValid || undefined}
                />
                <FieldError match={!field.state.meta.isValid}>
                  {formatFieldErrors(field.state.meta.errors)}
                </FieldError>
              </Field>
            )}
          </form.Field>
        </CardPanel>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Recipients</CardTitle>
          <CardDescription>
            Every address gets an email when an incident opens.
          </CardDescription>
        </CardHeader>
        <CardPanel>
          <form.Field name="recipients" mode="array">
            {(recipientsField) => (
              <div className="flex flex-col gap-3">
                {recipientsField.state.value.map((_, index) => (
                  <form.Field
                    // pola tablicowe TanStack są adresowane indeksem
                    key={index}
                    name={`recipients[${index}].email`}
                  >
                    {(field) => (
                      <Field
                        name={field.name}
                        invalid={!field.state.meta.isValid}
                        dirty={field.state.meta.isDirty}
                        touched={field.state.meta.isTouched}
                      >
                        <FieldLabel className="sr-only">
                          Recipient {index + 1}
                        </FieldLabel>
                        <div className="flex w-full items-center gap-2">
                          <Input
                            name={field.name}
                            type="email"
                            inputMode="email"
                            value={field.state.value}
                            onValueChange={field.handleChange}
                            onBlur={field.handleBlur}
                            placeholder="ops@example.com"
                            autoComplete="off"
                            aria-invalid={
                              !field.state.meta.isValid || undefined
                            }
                          />
                          <Button
                            aria-label={`Remove recipient ${index + 1}`}
                            size="icon"
                            variant="ghost"
                            disabled={recipientsField.state.value.length === 1}
                            onClick={() => recipientsField.removeValue(index)}
                          >
                            <Trash2Icon />
                          </Button>
                        </div>
                        <FieldError match={!field.state.meta.isValid}>
                          {formatFieldErrors(field.state.meta.errors)}
                        </FieldError>
                      </Field>
                    )}
                  </form.Field>
                ))}

                {/* błędy całej listy (np. duplikaty) — nie należą do żadnego pojedynczego pola */}
                {!recipientsField.state.meta.isValid ? (
                  <p className="text-destructive-foreground text-xs">
                    {formatFieldErrors(recipientsField.state.meta.errors)}
                  </p>
                ) : null}

                <Button
                  className="self-start"
                  variant="outline"
                  disabled={
                    recipientsField.state.value.length >= MAX_EMAIL_RECIPIENTS
                  }
                  onClick={() => recipientsField.pushValue({ email: "" })}
                >
                  <PlusIcon />
                  Add recipient
                </Button>
              </div>
            )}
          </form.Field>
        </CardPanel>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Monitors</CardTitle>
          <CardDescription>
            Alerts from these monitors are sent to this channel. You can change
            this later.
          </CardDescription>
        </CardHeader>
        <CardPanel>
          <form.Field name="monitorIds">
            {(field) => (
              <MonitorPicker
                organizationId={organizationId}
                value={field.state.value}
                onValueChange={field.handleChange}
              />
            )}
          </form.Field>
        </CardPanel>
      </Card>

      <div className="flex justify-end gap-2">
        <Button variant="ghost" render={<Link href={channelsHref} />}>
          Cancel
        </Button>
        <form.Subscribe selector={(state) => state.isSubmitting}>
          {(isSubmitting) => (
            <Button type="submit" loading={isSubmitting}>
              Create channel
            </Button>
          )}
        </form.Subscribe>
      </div>
    </form>
  );
}
