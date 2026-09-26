import {
  Card,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import {
  Field,
  FieldDescription,
  FieldLabel,
} from "@/components/ui/field";
import { Switch } from "@/components/ui/switch";
import { withForm } from "../../lib/form";
import { maintenanceWindowFormOptions } from "../../schemas/create-maintenance-window";

export const BehaviorSection = withForm({
  ...maintenanceWindowFormOptions,
  render: function Render({ form }) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>During maintenance</CardTitle>
        </CardHeader>
        <CardPanel className="grid gap-5">
          <form.Field name="suppressNotifications">
            {(field) => (
              <Field name={field.name}>
                <FieldLabel className="flex items-center gap-3">
                  <Switch
                    checked={field.state.value}
                    onCheckedChange={field.handleChange}
                  />
                  Mute alerts
                </FieldLabel>
                <FieldDescription>
                  Incidents opened during the window don’t notify your
                  channels.
                </FieldDescription>
              </Field>
            )}
          </form.Field>

          <form.Field name="excludeFromSla">
            {(field) => (
              <Field name={field.name}>
                <FieldLabel className="flex items-center gap-3">
                  <Switch
                    checked={field.state.value}
                    onCheckedChange={field.handleChange}
                  />
                  Exclude from uptime
                </FieldLabel>
                <FieldDescription>
                  Downtime during the window doesn’t lower your uptime and SLA.
                </FieldDescription>
              </Field>
            )}
          </form.Field>
        </CardPanel>
      </Card>
    );
  },
});
