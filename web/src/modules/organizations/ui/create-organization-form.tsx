"use client";

import { revalidateLogic, useForm } from "@tanstack/react-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { CircleAlertIcon, CircleCheckIcon, TriangleAlertIcon } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
  InputGroupText,
} from "@/components/ui/input-group";
import { Spinner } from "@/components/ui/spinner";
import { toastManager } from "@/components/ui/toast";
import {
  Popover,
  PopoverPopup,
  PopoverTrigger,
} from "@/components/ui/popover";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/errors";
import { formatFieldErrors } from "@/modules/auth/lib/format-field-errors";
import { organizationKeys } from "@/modules/organizations/hooks/keys";
import {
  type SlugStatus,
  useSlugAvailability,
} from "@/modules/organizations/hooks/use-slug-availability";
import { uploadOrganizationLogo } from "@/modules/organizations/lib/logo";
import { sanitizeSlugInput, slugify } from "@/modules/organizations/lib/slug";
import {
  type CreateOrganizationValues,
  createOrganizationSchema,
} from "@/modules/organizations/schemas/create-organization";
import {
  type LogoSelection,
  OrganizationLogoPicker,
} from "@/modules/organizations/ui/organization-logo-picker";

const defaultValues: CreateOrganizationValues = {
  name: "",
  slug: "",
};

type CreateOrganizationInput = CreateOrganizationValues & {
  logo: File | null;
};

export function CreateOrganizationForm() {
  const [formError, setFormError] = useState<string | null>(null);
  const [logo, setLogo] = useState<LogoSelection | null>(null);
  const [slugEdited, setSlugEdited] = useState(false);
  const router = useRouter();
  const queryClient = useQueryClient();

  const createOrganization = useMutation({
    mutationFn: async ({ name, slug, logo }: CreateOrganizationInput) => {
      const organization = await unwrap(
        api.POST("/api/organizations", { body: { name, slug } }),
        "Could not create the organization.",
      );

      let logoUploaded = true;
      if (logo) {
        try {
          await uploadOrganizationLogo(organization.id, logo);
        } catch {
          logoUploaded = false;
        }
      }

      return { organization, logoUploaded };
    },
    onSuccess: async ({ organization, logoUploaded }) => {
      await queryClient.invalidateQueries({ queryKey: organizationKeys.all });

      if (!logoUploaded) {
        toastManager.add({
          type: "warning",
          title: "Organization created without a logo",
          description: "The logo upload failed. You can add it later in settings.",
        });
      }

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
        await createOrganization.mutateAsync({ ...value, logo: logo?.file ?? null });
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

      <form.Subscribe
        selector={(state) => [state.values.name, state.isSubmitting] as const}
      >
        {([name, isSubmitting]) => (
          <OrganizationLogoPicker
            name={name}
            value={logo}
            onChange={setLogo}
            disabled={isSubmitting}
          />
        )}
      </form.Subscribe>

      <form.Field
        name="name"
        listeners={{
          onChange: ({ value }) => {
            if (!slugEdited) {
              form.setFieldValue("slug", slugify(value));
            }
          },
        }}
      >
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

      <form.Field name="slug">
        {(field) => (
          <Field
            name={field.name}
            invalid={!field.state.meta.isValid}
            dirty={field.state.meta.isDirty}
            touched={field.state.meta.isTouched}
          >
            <FieldLabel>Slug</FieldLabel>
            <InputGroup>
              <InputGroupAddon>
                <InputGroupText>/</InputGroupText>
              </InputGroupAddon>
              <InputGroupInput
                name={field.name}
                value={field.state.value}
                onValueChange={(value) => {
                  const slug = sanitizeSlugInput(value);
                  setSlugEdited(slug !== "");
                  field.handleChange(slug);
                }}
                onBlur={field.handleBlur}
                placeholder="acme-inc"
                autoComplete="off"
                spellCheck={false}
                aria-invalid={!field.state.meta.isValid || undefined}
              />
              <InputGroupAddon align="inline-end">
                <SlugStatusIndicator slug={field.state.value} />
              </InputGroupAddon>
            </InputGroup>
            <FieldError match={!field.state.meta.isValid}>
              {formatFieldErrors(field.state.meta.errors)}
            </FieldError>
            <FieldDescription>
              Used in your organization&apos;s URLs, e.g. /
              {field.state.value || "acme-inc"}/monitors
            </FieldDescription>
          </Field>
        )}
      </form.Field>

      <form.Subscribe selector={(state) => state.isSubmitting}>
        {(isSubmitting) => (
          <Button className="w-full" type="submit" loading={isSubmitting}>
            Continue
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}

function SlugStatusIndicator({ slug }: { slug: string }) {
  const status: SlugStatus = useSlugAvailability(slug);

  if (status === "checking") {
    return <Spinner className="size-4 text-muted-foreground" />;
  }

  if (status === "available") {
    return (
      <CircleCheckIcon
        className="size-4 text-success-foreground"
        aria-label="Slug is available"
      />
    );
  }

  if (status === "taken") {
    return (
      <Popover>
        <PopoverTrigger
          openOnHover
          delay={0}
          render={
            <button
              type="button"
              className="flex cursor-help rounded-sm text-warning-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring"
              aria-label="Slug is taken"
            />
          }
        >
          <TriangleAlertIcon className="size-4" />
        </PopoverTrigger>
        <PopoverPopup tooltipStyle side="top" className="max-w-64">
          <span className="font-medium">/{slug}</span> is already taken. If you
          continue, we&apos;ll add a few random characters to the end, e.g. /
          {slug}-x7k2p1.
        </PopoverPopup>
      </Popover>
    );
  }

  return null;
}
