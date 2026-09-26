import {
  Card,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { withForm } from "../../lib/form";
import { formatDuration, formatTime } from "../../lib/format";
import { weekdayCodeOf } from "../../lib/recurrence";
import { addMinutes, type DateKey, toWallClock } from "../../lib/wall-clock";
import {
  durationOf,
  maintenanceWindowFormOptions,
} from "../../schemas/create-maintenance-window";
import { DatePicker } from "./date-picker";
import { FieldShell } from "./field-shell";
import { NumberInput } from "./number-input";
import { TimeZoneCombobox } from "./time-zone-combobox";

export const ScheduleSection = withForm({
  ...maintenanceWindowFormOptions,
  props: { minDate: "" as DateKey },
  render: function Render({ form, minDate }) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>When</CardTitle>
          <CardDescription>
            Wall-clock time in the chosen time zone — it stays the same across
            daylight saving changes.
          </CardDescription>
        </CardHeader>
        <CardPanel className="grid gap-4">
          <form.Field name="schedule.timeZoneId">
            {(field) => (
              <FieldShell field={field} label="Time zone">
                <TimeZoneCombobox
                  value={field.state.value}
                  onValueChange={field.handleChange}
                  onBlur={field.handleBlur}
                  invalid={!field.state.meta.isValid}
                />
              </FieldShell>
            )}
          </form.Field>

          <div className="grid gap-4 sm:grid-cols-[minmax(0,1fr)_8rem]">
            <form.Field
              name="schedule.startDate"
              listeners={{
                onChange: ({ value }) => {
                  const weekdays = form.getFieldValue(
                    "schedule.recurrence.weekdays",
                  );
                  if (weekdays.length === 1) {
                    form.setFieldValue("schedule.recurrence.weekdays", [
                      weekdayCodeOf(toWallClock(value, "00:00")),
                    ]);
                  }
                },
              }}
            >
              {(field) => (
                <FieldShell field={field} label="Starts on">
                  <DatePicker
                    label="Start date"
                    value={field.state.value}
                    onValueChange={field.handleChange}
                    minDate={minDate || undefined}
                    invalid={!field.state.meta.isValid}
                  />
                </FieldShell>
              )}
            </form.Field>

            <form.Field name="schedule.startTime">
              {(field) => (
                <FieldShell field={field} label="At">
                  <Input
                    name={field.name}
                    type="time"
                    step={300}
                    value={field.state.value}
                    onValueChange={field.handleChange}
                    onBlur={field.handleBlur}
                    aria-invalid={!field.state.meta.isValid || undefined}
                  />
                </FieldShell>
              )}
            </form.Field>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <form.Field name="schedule.durationHours">
              {(field) => (
                <FieldShell field={field} label="Duration (hours)">
                  <NumberInput
                    value={field.state.value}
                    onValueChange={field.handleChange}
                    onBlur={field.handleBlur}
                    min={0}
                    max={720}
                    invalid={!field.state.meta.isValid}
                  />
                </FieldShell>
              )}
            </form.Field>
            <form.Field name="schedule.durationMinutes">
              {(field) => (
                <FieldShell field={field} label="Minutes">
                  <NumberInput
                    value={field.state.value}
                    onValueChange={field.handleChange}
                    onBlur={field.handleBlur}
                    min={0}
                    max={59}
                    step={5}
                    invalid={!field.state.meta.isValid}
                  />
                </FieldShell>
              )}
            </form.Field>
          </div>

          <form.Subscribe selector={(state) => state.values.schedule}>
            {(schedule) => <DurationSummary schedule={schedule} />}
          </form.Subscribe>
        </CardPanel>
      </Card>
    );
  },
});

function DurationSummary({
  schedule,
}: {
  schedule: Parameters<typeof durationOf>[0];
}) {
  const minutes = durationOf(schedule);
  if (!Number.isFinite(minutes) || minutes < 1 || !schedule.startTime) {
    return null;
  }

  const start = toWallClock(schedule.startDate || "2000-01-01", schedule.startTime);
  const end = addMinutes(start, minutes);
  const endsNextDay = end.getUTCDate() !== start.getUTCDate() || minutes >= 1440;

  return (
    <p className="text-muted-foreground text-sm" aria-live="polite">
      Lasts {formatDuration(minutes)} and ends at{" "}
      <span className="font-medium text-foreground tabular-nums">
        {formatTime(end, "UTC")}
      </span>
      {endsNextDay ? " on a later day" : null}.
    </p>
  );
}
