import type React from "react";
import {
  Card,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { withForm } from "../../lib/form";
import { maintenanceWindowFormOptions } from "../../schemas/create-maintenance-window";
import { FieldShell } from "./field-shell";

export const GeneralSection = withForm({
  ...maintenanceWindowFormOptions,
  props: { extra: null as React.ReactNode },
  render: function Render({ form, extra }) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>General</CardTitle>
          <CardDescription>
            Shown on every occurrence, e.g. on your status page.
          </CardDescription>
        </CardHeader>
        <CardPanel className="grid gap-4">
          <form.Field name="name">
            {(field) => (
              <FieldShell field={field} label="Name">
                <Input
                  name={field.name}
                  value={field.state.value}
                  onValueChange={field.handleChange}
                  onBlur={field.handleBlur}
                  placeholder="Database upgrade"
                  autoComplete="off"
                  aria-invalid={!field.state.meta.isValid || undefined}
                />
              </FieldShell>
            )}
          </form.Field>

          <form.Field name="description">
            {(field) => (
              <FieldShell
                field={field}
                label="Description"
                description="Optional. What happens and what users may notice."
              >
                <Textarea
                  name={field.name}
                  value={field.state.value}
                  onChange={(event) => field.handleChange(event.target.value)}
                  onBlur={field.handleBlur}
                  placeholder="PostgreSQL is upgraded to a new major version. The API may respond slowly."
                  rows={3}
                  aria-invalid={!field.state.meta.isValid || undefined}
                />
              </FieldShell>
            )}
          </form.Field>

          {extra}
        </CardPanel>
      </Card>
    );
  },
});
