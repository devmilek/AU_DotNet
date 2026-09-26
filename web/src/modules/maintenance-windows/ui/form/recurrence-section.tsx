import { CodeIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Radio, RadioGroup } from "@/components/ui/radio-group";
import {
  Select,
  SelectItem,
  SelectPopup,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { withForm } from "../../lib/form";
import {
  buildRecurrenceRule,
  describeMonthlyMode,
  FREQUENCIES,
  type Frequency,
  frequencyLabels,
  intervalUnits,
  MAX_OCCURRENCES,
  MONTHLY_MODES,
  type MonthlyMode,
  type RecurrenceEnd,
  WEEKDAY_CODES,
  type WeekdayCode,
  weekdayName,
} from "../../lib/recurrence";
import { type DateKey, toWallClock } from "../../lib/wall-clock";
import { maintenanceWindowFormOptions } from "../../schemas/create-maintenance-window";
import { DatePicker } from "./date-picker";
import { FieldShell, InlineFieldError } from "./field-shell";
import { NumberInput } from "./number-input";

const frequencyItems = FREQUENCIES.map((frequency) => ({
  value: frequency,
  label: frequencyLabels[frequency],
}));

const rulePresets = [
  { label: "First Sunday of the month", rule: "FREQ=MONTHLY;BYDAY=1SU" },
  { label: "Every weekday", rule: "FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR" },
  { label: "Every other Saturday", rule: "FREQ=WEEKLY;INTERVAL=2;BYDAY=SA" },
  { label: "Last Friday of the quarter", rule: "FREQ=MONTHLY;INTERVAL=3;BYDAY=-1FR" },
];

export const RecurrenceSection = withForm({
  ...maintenanceWindowFormOptions,
  render: function Render({ form }) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Repeat</CardTitle>
          <CardDescription>
            Build the rule step by step or write your own RRULE.
          </CardDescription>
        </CardHeader>
        <form.Subscribe selector={(state) => state.values.schedule}>
          {(schedule) => {
            const { frequency } = schedule.recurrence;
            const start = toWallClock(
              schedule.startDate,
              schedule.startTime || "00:00",
            );
            const repeats = frequency !== "none";
            const builtIn = repeats && frequency !== "custom";
            const generatedRule = builtIn
              ? buildRecurrenceRule(schedule.recurrence, start)
              : null;
            const unit = intervalUnits[frequency];

            return (
              <CardPanel className="grid gap-5">
                <form.Field name="schedule.recurrence.frequency">
                  {(field) => (
                    <FieldShell field={field} label="Frequency">
                      <Select
                        items={frequencyItems}
                        value={field.state.value}
                        onValueChange={(value) => {
                          if (value) field.handleChange(value as Frequency);
                        }}
                      >
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectPopup>
                          {frequencyItems.map((item) => (
                            <SelectItem key={item.value} value={item.value}>
                              {item.label}
                            </SelectItem>
                          ))}
                        </SelectPopup>
                      </Select>
                    </FieldShell>
                  )}
                </form.Field>

                {unit ? (
                  <form.Field name="schedule.recurrence.interval">
                    {(field) => (
                      <FieldShell field={field} label="Repeat every">
                        <div className="flex items-center gap-3">
                          <NumberInput
                            className="w-32"
                            aria-label="Interval"
                            value={field.state.value}
                            onValueChange={field.handleChange}
                            onBlur={field.handleBlur}
                            min={1}
                            max={99}
                            invalid={!field.state.meta.isValid}
                          />
                          <span className="text-muted-foreground text-sm">
                            {field.state.value === 1 ? unit[0] : unit[1]}
                          </span>
                        </div>
                      </FieldShell>
                    )}
                  </form.Field>
                ) : null}

                {frequency === "weekly" ? (
                  <form.Field name="schedule.recurrence.weekdays">
                    {(field) => (
                      <FieldShell field={field} label="On">
                        <ToggleGroup
                          multiple
                          variant="outline"
                          aria-label="Days of the week"
                          className="flex-wrap"
                          value={field.state.value}
                          onValueChange={(value) =>
                            field.handleChange(value as WeekdayCode[])
                          }
                        >
                          {WEEKDAY_CODES.map((code) => (
                            <ToggleGroupItem
                              key={code}
                              value={code}
                              aria-label={weekdayName(code)}
                              className="min-w-11"
                            >
                              {weekdayName(code, "short")}
                            </ToggleGroupItem>
                          ))}
                        </ToggleGroup>
                      </FieldShell>
                    )}
                  </form.Field>
                ) : null}

                {frequency === "monthly" ? (
                  <form.Field name="schedule.recurrence.monthlyMode">
                    {(field) => (
                      <FieldShell field={field} label="On">
                        <RadioGroup
                          aria-label="Day of the month"
                          value={field.state.value}
                          onValueChange={(value) =>
                            field.handleChange(value as MonthlyMode)
                          }
                        >
                          {MONTHLY_MODES.map((mode) => (
                            <Label key={mode} className="flex items-center gap-2">
                              <Radio value={mode} />
                              {describeMonthlyMode(mode, start)}
                            </Label>
                          ))}
                        </RadioGroup>
                      </FieldShell>
                    )}
                  </form.Field>
                ) : null}

                {frequency === "custom" ? (
                  <form.Field name="schedule.recurrence.customRule">
                    {(field) => (
                      <FieldShell
                        field={field}
                        label="RRULE"
                        description="An RFC 5545 rule without DTSTART — the start date above is used."
                      >
                        <Textarea
                          name={field.name}
                          className="font-mono"
                          rows={2}
                          spellCheck={false}
                          autoComplete="off"
                          placeholder="FREQ=MONTHLY;BYDAY=1SU"
                          value={field.state.value}
                          onChange={(event) =>
                            field.handleChange(event.target.value)
                          }
                          onBlur={field.handleBlur}
                          aria-invalid={!field.state.meta.isValid || undefined}
                        />
                        <div className="flex flex-wrap gap-1.5">
                          {rulePresets.map((preset) => (
                            <Button
                              key={preset.rule}
                              size="xs"
                              variant="outline"
                              onClick={() => field.handleChange(preset.rule)}
                            >
                              {preset.label}
                            </Button>
                          ))}
                        </div>
                      </FieldShell>
                    )}
                  </form.Field>
                ) : null}

                {builtIn ? (
                  <RecurrenceEndFields
                    form={form}
                    ends={schedule.recurrence.ends}
                    minDate={schedule.startDate}
                  />
                ) : null}

                {generatedRule ? (
                  <div className="flex items-center justify-between gap-2 rounded-lg bg-muted/72 py-1.5 ps-3 pe-1.5">
                    <code className="truncate font-mono text-muted-foreground text-xs">
                      {generatedRule}
                    </code>
                    <Button
                      size="xs"
                      variant="ghost"
                      onClick={() => {
                        form.setFieldValue(
                          "schedule.recurrence.customRule",
                          generatedRule,
                        );
                        form.setFieldValue(
                          "schedule.recurrence.frequency",
                          "custom",
                        );
                      }}
                    >
                      <CodeIcon />
                      Edit as RRULE
                    </Button>
                  </div>
                ) : null}
              </CardPanel>
            );
          }}
        </form.Subscribe>
      </Card>
    );
  },
});

const RecurrenceEndFields = withForm({
  ...maintenanceWindowFormOptions,
  props: { ends: "never" as RecurrenceEnd, minDate: "" as DateKey },
  render: function Render({ form, ends, minDate }) {
    return (
      <form.Field name="schedule.recurrence.ends">
        {(endsField) => (
          <FieldShell field={endsField} label="Ends">
            <RadioGroup
              aria-label="Ends"
              className="gap-3"
              value={endsField.state.value}
              onValueChange={(value) =>
                endsField.handleChange(value as RecurrenceEnd)
              }
            >
              <Label className="flex min-h-9 items-center gap-2 sm:min-h-8">
                <Radio value="never" />
                Never
              </Label>

              <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
                <Label className="flex w-16 items-center gap-2">
                  <Radio value="onDate" />
                  On
                </Label>
                <form.Field name="schedule.recurrence.endDate">
                  {(field) => (
                    <div className="flex min-w-0 flex-1 flex-col gap-1">
                      <DatePicker
                        label="End date"
                        value={field.state.value}
                        onValueChange={(value) => {
                          field.handleChange(value);
                          endsField.handleChange("onDate");
                        }}
                        minDate={minDate}
                        invalid={ends === "onDate" && !field.state.meta.isValid}
                      />
                      <InlineFieldError field={field} />
                    </div>
                  )}
                </form.Field>
              </div>

              <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
                <Label className="flex w-16 items-center gap-2">
                  <Radio value="afterCount" />
                  After
                </Label>
                <form.Field name="schedule.recurrence.count">
                  {(field) => (
                    <div className="flex flex-col gap-1">
                      <div className="flex items-center gap-3">
                        <NumberInput
                          className="w-32"
                          aria-label="Number of occurrences"
                          value={field.state.value}
                          onValueChange={(value) => {
                            field.handleChange(value);
                            endsField.handleChange("afterCount");
                          }}
                          onBlur={field.handleBlur}
                          min={1}
                          max={MAX_OCCURRENCES}
                          invalid={
                            ends === "afterCount" && !field.state.meta.isValid
                          }
                        />
                        <span className="text-muted-foreground text-sm">
                          {field.state.value === 1 ? "occurrence" : "occurrences"}
                        </span>
                      </div>
                      <InlineFieldError field={field} />
                    </div>
                  )}
                </form.Field>
              </div>
            </RadioGroup>
          </FieldShell>
        )}
      </form.Field>
    );
  },
});
