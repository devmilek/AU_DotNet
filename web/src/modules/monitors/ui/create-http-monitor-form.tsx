"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { CircleAlertIcon } from "lucide-react";
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
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  NumberField,
  NumberFieldDecrement,
  NumberFieldGroup,
  NumberFieldIncrement,
  NumberFieldInput,
} from "@/components/ui/number-field";
import { Radio, RadioGroup } from "@/components/ui/radio-group";
import {
  Select,
  SelectItem,
  SelectPopup,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { api } from "@/lib/api/client";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import { monitorKeys } from "@/modules/monitors/hooks/keys";
import {
  type CreateHttpMonitorValues,
  createHttpMonitorDefaults,
  createHttpMonitorSchema,
  httpAuthTypes,
  httpMethods,
  intervalOptions,
  toCreateMonitorRequest,
} from "@/modules/monitors/schemas/create-http-monitor";

type ServerErrors = { title: string; details: string[] };

// klucze błędów z FluentValidation -> etykiety pól w formularzu
const serverFieldLabels: Record<string, string> = {
  Name: "Name",
  Target: "URL",
  IntervalSeconds: "Check interval",
  TimeoutMs: "Timeout",
  AlertThreshold: "Alert threshold",
  RecoveryThreshold: "Recovery threshold",
  "Http.Method": "Method",
  "Http.AcceptedStatusCodes": "Accepted status codes",
  "Http.Auth.Username": "Username",
  "Http.Auth.Password": "Password",
  "Http.Auth.Token": "Token",
};

const authTypeLabels: Record<(typeof httpAuthTypes)[number], string> = {
  None: "None",
  Basic: "Basic auth",
  Bearer: "Bearer token",
};

function toServerErrors(error: unknown): ServerErrors {
  const problem = (error ?? {}) as {
    title?: string;
    errors?: Record<string, string[]>;
  };

  const details = Object.entries(problem.errors ?? {}).flatMap(
    ([key, messages]) => {
      const label = serverFieldLabels[key.replace(/\[\d+\]/g, "")] ?? key;
      return messages.map((message) => `${label}: ${message}`);
    },
  );

  return {
    title: problem.title ?? "Nie udało się utworzyć monitora.",
    details,
  };
}

export function CreateHttpMonitorForm({
  organizationId,
  organizationSlug,
}: {
  organizationId: string;
  organizationSlug: string;
}) {
  const [serverErrors, setServerErrors] = useState<ServerErrors | null>(null);
  const router = useRouter();
  const queryClient = useQueryClient();
  const monitorsHref = `/${organizationSlug}/monitors`;

  const createMonitor = useMutation({
    mutationFn: async (values: CreateHttpMonitorValues) => {
      const { data, response, error } = await api.POST(
        "/api/{orgId}/monitors",
        {
          params: { path: { orgId: organizationId } },
          body: toCreateMonitorRequest(values),
        },
      );

      if (!response.ok || !data) {
        throw toServerErrors(error);
      }

      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: monitorKeys.lists(organizationId),
      });
      router.push(monitorsHref);
    },
  });

  const form = useForm({
    defaultValues: createHttpMonitorDefaults,
    validationLogic: revalidateLogic({
      mode: "submit",
      modeAfterSubmission: "change",
    }),
    validators: {
      onDynamic: createHttpMonitorSchema,
    },
    onSubmit: async ({ value }) => {
      setServerErrors(null);
      try {
        await createMonitor.mutateAsync(value);
      } catch (error) {
        setServerErrors(
          error && typeof error === "object" && "details" in error
            ? (error as ServerErrors)
            : {
                title: "Something went wrong. Please try again.",
                details: [],
              },
        );
      }
    },
  });

  const renderNumberField = ({
    name,
    label,
    description,
    min,
    max,
    step = 1,
  }: {
    name: "timeoutMs" | "alertThreshold" | "recoveryThreshold";
    label: string;
    description?: string;
    min?: number;
    max?: number;
    step?: number;
  }) => (
    <form.Field name={name}>
      {(field) => (
        <Field
          name={field.name}
          invalid={!field.state.meta.isValid}
          dirty={field.state.meta.isDirty}
          touched={field.state.meta.isTouched}
        >
          <FieldLabel>{label}</FieldLabel>
          <NumberField
            value={Number.isNaN(field.state.value) ? null : field.state.value}
            onValueChange={(value) => field.handleChange(value ?? Number.NaN)}
            min={min}
            max={max}
            step={step}
          >
            <NumberFieldGroup>
              <NumberFieldDecrement />
              <NumberFieldInput
                onBlur={field.handleBlur}
                aria-invalid={!field.state.meta.isValid || undefined}
              />
              <NumberFieldIncrement />
            </NumberFieldGroup>
          </NumberField>
          {description ? (
            <FieldDescription>{description}</FieldDescription>
          ) : null}
          <FieldError match={!field.state.meta.isValid}>
            {formatFieldErrors(field.state.meta.errors)}
          </FieldError>
        </Field>
      )}
    </form.Field>
  );

  const renderCredentialField = ({
    name,
    label,
    type,
    description,
  }: {
    name: "username" | "password" | "token";
    label: string;
    type: "text" | "password";
    description?: string;
  }) => (
    <form.Field name={name}>
      {(field) => (
        <Field
          name={field.name}
          invalid={!field.state.meta.isValid}
          dirty={field.state.meta.isDirty}
          touched={field.state.meta.isTouched}
        >
          <FieldLabel>{label}</FieldLabel>
          <Input
            name={field.name}
            type={type}
            value={field.state.value}
            onValueChange={field.handleChange}
            onBlur={field.handleBlur}
            // żeby przeglądarka nie wstawiła tu danych logowania do naszej aplikacji
            autoComplete={type === "password" ? "new-password" : "off"}
            spellCheck={false}
            aria-invalid={!field.state.meta.isValid || undefined}
          />
          {description ? (
            <FieldDescription>{description}</FieldDescription>
          ) : null}
          <FieldError match={!field.state.meta.isValid}>
            {formatFieldErrors(field.state.meta.errors)}
          </FieldError>
        </Field>
      )}
    </form.Field>
  );

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
      {serverErrors ? (
        <Alert variant="error">
          <CircleAlertIcon />
          <AlertTitle>{serverErrors.title}</AlertTitle>
          {serverErrors.details.length > 0 ? (
            <AlertDescription>
              <ul className="list-disc ps-4">
                {serverErrors.details.map((detail) => (
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
          <CardDescription>What should we watch?</CardDescription>
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
                  placeholder="Marketing website"
                  autoComplete="off"
                  aria-invalid={!field.state.meta.isValid || undefined}
                />
                <FieldError match={!field.state.meta.isValid}>
                  {formatFieldErrors(field.state.meta.errors)}
                </FieldError>
              </Field>
            )}
          </form.Field>

          <form.Field name="target">
            {(field) => (
              <Field
                name={field.name}
                invalid={!field.state.meta.isValid}
                dirty={field.state.meta.isDirty}
                touched={field.state.meta.isTouched}
              >
                <FieldLabel>URL</FieldLabel>
                <Input
                  name={field.name}
                  type="url"
                  inputMode="url"
                  value={field.state.value}
                  onValueChange={field.handleChange}
                  onBlur={field.handleBlur}
                  placeholder="https://example.com/health"
                  autoComplete="off"
                  spellCheck={false}
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
          <CardTitle>Checks</CardTitle>
          <CardDescription>
            How often to check and when to open an incident.
          </CardDescription>
        </CardHeader>
        <CardPanel className="grid gap-4 sm:grid-cols-2">
          <form.Field name="intervalSeconds">
            {(field) => (
              <Field name={field.name}>
                <FieldLabel>Check interval</FieldLabel>
                <Select
                  items={intervalOptions}
                  value={field.state.value}
                  onValueChange={(value) => {
                    if (typeof value === "number") field.handleChange(value);
                  }}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectPopup>
                    {intervalOptions.map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectPopup>
                </Select>
              </Field>
            )}
          </form.Field>

          {renderNumberField({
            name: "timeoutMs",
            label: "Timeout (ms)",
            description: "Request is marked as failed after this time.",
            min: 100,
            step: 500,
          })}
          {renderNumberField({
            name: "alertThreshold",
            label: "Alert threshold",
            description: "Failed checks in a row before an incident opens.",
            min: 1,
            max: 100,
          })}
          {renderNumberField({
            name: "recoveryThreshold",
            label: "Recovery threshold",
            description: "Successful checks in a row before it resolves.",
            min: 1,
            max: 100,
          })}
        </CardPanel>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>HTTP request</CardTitle>
          <CardDescription>
            How the request is sent and which responses count as up.
          </CardDescription>
        </CardHeader>
        <CardPanel className="grid gap-4 sm:grid-cols-2">
          <form.Field name="method">
            {(field) => (
              <Field name={field.name}>
                <FieldLabel>Method</FieldLabel>
                <Select
                  value={field.state.value}
                  onValueChange={(value) => {
                    if (value) field.handleChange(value);
                  }}
                >
                  <SelectTrigger>
                    <SelectValue className="uppercase" />
                  </SelectTrigger>
                  <SelectPopup>
                    {httpMethods.map((method) => (
                      <SelectItem key={method} value={method}>
                        <span className="uppercase">{method}</span>
                      </SelectItem>
                    ))}
                  </SelectPopup>
                </Select>
              </Field>
            )}
          </form.Field>

          <form.Field name="acceptedStatusCodes">
            {(field) => (
              <Field
                name={field.name}
                invalid={!field.state.meta.isValid}
                dirty={field.state.meta.isDirty}
                touched={field.state.meta.isTouched}
              >
                <FieldLabel>Accepted status codes</FieldLabel>
                <Input
                  name={field.name}
                  value={field.state.value}
                  onValueChange={field.handleChange}
                  onBlur={field.handleBlur}
                  placeholder="200-299"
                  autoComplete="off"
                  spellCheck={false}
                  aria-invalid={!field.state.meta.isValid || undefined}
                />
                <FieldDescription>
                  Comma-separated codes or ranges, e.g. 200-299, 301.
                </FieldDescription>
                <FieldError match={!field.state.meta.isValid}>
                  {formatFieldErrors(field.state.meta.errors)}
                </FieldError>
              </Field>
            )}
          </form.Field>

          <form.Field name="followRedirects">
            {(field) => (
              <Field name={field.name} className="sm:col-span-2">
                <FieldLabel className="flex items-center gap-3">
                  <Switch
                    checked={field.state.value}
                    onCheckedChange={field.handleChange}
                  />
                  Follow redirects
                </FieldLabel>
                <FieldDescription>
                  When off, a 3xx response is checked against the accepted
                  status codes as is.
                </FieldDescription>
              </Field>
            )}
          </form.Field>
        </CardPanel>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Authentication</CardTitle>
          <CardDescription>
            Credentials are encrypted and never shown again after saving.
          </CardDescription>
        </CardHeader>
        <CardPanel className="grid gap-4">
          <form.Field name="authType">
            {(field) => (
              <RadioGroup
                aria-label="Authentication type"
                className="flex-row flex-wrap gap-x-6"
                value={field.state.value}
                onValueChange={(value) =>
                  field.handleChange(value as (typeof httpAuthTypes)[number])
                }
              >
                {httpAuthTypes.map((type) => (
                  <Label key={type} className="flex items-center gap-2">
                    <Radio value={type} />
                    {authTypeLabels[type]}
                  </Label>
                ))}
              </RadioGroup>
            )}
          </form.Field>

          <form.Subscribe selector={(state) => state.values.authType}>
            {(authType) =>
              authType === "Basic" ? (
                <div className="grid gap-4 sm:grid-cols-2">
                  {renderCredentialField({
                    name: "username",
                    label: "Username",
                    type: "text",
                  })}
                  {renderCredentialField({
                    name: "password",
                    label: "Password",
                    type: "password",
                  })}
                </div>
              ) : authType === "Bearer" ? (
                renderCredentialField({
                  name: "token",
                  label: "Token",
                  type: "password",
                  description: 'Sent as "Authorization: Bearer <token>".',
                })
              ) : null
            }
          </form.Subscribe>
        </CardPanel>
      </Card>

      <div className="flex justify-end gap-2">
        <Button variant="ghost" render={<Link href={monitorsHref} />}>
          Cancel
        </Button>
        <form.Subscribe selector={(state) => state.isSubmitting}>
          {(isSubmitting) => (
            <Button type="submit" loading={isSubmitting}>
              Create monitor
            </Button>
          )}
        </form.Subscribe>
      </div>
    </form>
  );
}
