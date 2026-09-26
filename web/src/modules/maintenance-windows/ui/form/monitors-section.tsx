import {
  Card,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import { MonitorPicker } from "@/modules/monitors/ui/monitor-picker";
import { withForm } from "../../lib/form";
import { maintenanceWindowFormOptions } from "../../schemas/create-maintenance-window";
import { InlineFieldError } from "./field-shell";

export const MonitorsSection = withForm({
  ...maintenanceWindowFormOptions,
  props: { organizationId: "" },
  render: function Render({ form, organizationId }) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Monitors</CardTitle>
          <CardDescription>
            Monitors covered by this window. Checks still run — their results
            are marked as maintenance.
          </CardDescription>
        </CardHeader>
        <CardPanel>
          <form.Field name="monitorIds">
            {(field) => (
              <div className="flex flex-col gap-2">
                <MonitorPicker
                  organizationId={organizationId}
                  value={field.state.value}
                  onValueChange={field.handleChange}
                />
                <InlineFieldError field={field} />
              </div>
            )}
          </form.Field>
        </CardPanel>
      </Card>
    );
  },
});
