"use client";

import {
  ArrowDownToLineIcon,
  ArrowUpToLineIcon,
  WavesIcon,
} from "lucide-react";
import { useState } from "react";
import { Area, AreaChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardPanel,
  CardTitle,
} from "@/components/ui/card";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
} from "@/components/ui/chart";
import {
  Select,
  SelectItem,
  SelectPopup,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTab } from "@/components/ui/tabs";
import type { components } from "@/lib/api/schema";
import { cn } from "@/lib/utils";
import type { MonitorRef } from "@/modules/monitors/hooks/keys";
import { useMonitorResponseTimes } from "@/modules/monitors/hooks/use-monitor-details";
import { formatMilliseconds } from "@/modules/monitors/lib/format";
import {
  type PhaseKey,
  phaseValues,
  presentPhases,
  type ResponsePhase,
  responsePhases,
} from "@/modules/monitors/lib/response-phases";
import {
  type ResponseTimeRange,
  responseTimeRangeLabel,
  responseTimeRanges,
} from "@/modules/monitors/lib/response-time-range";

type ResponseTimePoint = components["schemas"]["ResponseTimePointResponse"];

type ChartMode = "breakdown" | "total";

type ChartPoint = {
  time: number;
  averageMs: number | null;
  minimumMs: number | null;
  maximumMs: number | null;
} & Record<PhaseKey, number | null>;

const chartConfig = {
  averageMs: { label: "Average", color: "var(--chart-1)" },
  ...Object.fromEntries(
    responsePhases.map((phase) => [
      phase.key,
      { label: phase.label, color: phase.color },
    ]),
  ),
} satisfies ChartConfig;

const rangeItems = Object.fromEntries(
  responseTimeRanges.map((range) => [range.value, range.label]),
);

function toChartPoint(
  point: ResponseTimePoint,
  present: ResponsePhase[],
): ChartPoint {
  return {
    time: new Date(point.timestamp).getTime(),
    averageMs: point.averageMs,
    minimumMs: point.minimumMs,
    maximumMs: point.maximumMs,
    ...phaseValues(point.phases, present),
  };
}

// oś X: godziny dla doby, daty dla dłuższych zakresów
function tickFormatter(range: ResponseTimeRange) {
  const formatter = new Intl.DateTimeFormat(
    undefined,
    range === "Last24Hours"
      ? { hour: "2-digit", minute: "2-digit" }
      : { day: "numeric", month: "short" },
  );
  return (value: number) => formatter.format(value);
}

const tooltipDateFormatter = new Intl.DateTimeFormat(undefined, {
  day: "numeric",
  month: "short",
  hour: "2-digit",
  minute: "2-digit",
});

export function ResponseTimeCard({
  monitorRef,
  range,
  onRangeChange,
}: {
  monitorRef: MonitorRef;
  range: ResponseTimeRange;
  onRangeChange: (range: ResponseTimeRange) => void;
}) {
  const responseTimes = useMonitorResponseTimes(monitorRef, range);
  const [selectedMode, setSelectedMode] = useState<ChartMode>("breakdown");
  const present = presentPhases(responseTimes.data?.points ?? []);
  const mode: ChartMode = present.length > 0 ? selectedMode : "total";
  const data = responseTimes.data?.points.map((point) =>
    toChartPoint(point, present),
  );
  const summary = responseTimes.data?.summary;
  const hasData = summary?.averageMs != null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Response time</CardTitle>
        <CardDescription>
          {mode === "breakdown"
            ? "Where the time goes in successful checks over the "
            : "Successful checks over the "}
          {responseTimeRangeLabel(range).toLowerCase()}.
        </CardDescription>
        <CardAction className="flex items-center gap-2">
          {present.length > 0 ? (
            <Tabs
              value={mode}
              onValueChange={(value) => setSelectedMode(value as ChartMode)}
            >
              <TabsList size="sm">
                <TabsTab value="breakdown">Breakdown</TabsTab>
                <TabsTab value="total">Total</TabsTab>
              </TabsList>
            </Tabs>
          ) : null}
          <Select
            items={rangeItems}
            value={range}
            onValueChange={(value) => {
              if (value) onRangeChange(value as ResponseTimeRange);
            }}
          >
            <SelectTrigger aria-label="Time range" className="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectPopup align="end">
              {responseTimeRanges.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectPopup>
          </Select>
        </CardAction>
      </CardHeader>

      <CardPanel className="flex flex-col gap-6">
        <div
          className={cn(
            "relative transition-opacity",
            responseTimes.isPlaceholderData && "opacity-64",
          )}
        >
          {data ? (
            <ChartContainer
              config={chartConfig}
              className="aspect-auto h-72 w-full"
            >
              <AreaChart
                accessibilityLayer
                data={data}
                margin={{ left: 4, right: 12, top: 8 }}
              >
                <defs>
                  <linearGradient id="fillAverage" x1="0" y1="0" x2="0" y2="1">
                    <stop
                      offset="5%"
                      stopColor="var(--color-averageMs)"
                      stopOpacity={0.5}
                    />
                    <stop
                      offset="95%"
                      stopColor="var(--color-averageMs)"
                      stopOpacity={0.05}
                    />
                  </linearGradient>
                </defs>
                <CartesianGrid vertical={false} />
                <XAxis
                  dataKey="time"
                  type="number"
                  scale="time"
                  domain={["dataMin", "dataMax"]}
                  tickFormatter={tickFormatter(range)}
                  tickLine={false}
                  axisLine={false}
                  tickMargin={8}
                  minTickGap={40}
                />
                <YAxis
                  tickFormatter={(value: number) => `${value} ms`}
                  tickLine={false}
                  axisLine={false}
                  width={64}
                />
                <ChartTooltip
                  cursor={{ stroke: "var(--border)" }}
                  content={
                    <ResponseTimeTooltip
                      phases={mode === "breakdown" ? present : []}
                    />
                  }
                />
                {mode === "breakdown"
                  ? present.map((phase) => (
                      <Area
                        key={phase.key}
                        dataKey={phase.key}
                        stackId="phases"
                        type="monotone"
                        stroke={`var(--color-${phase.key})`}
                        fill={`var(--color-${phase.key})`}
                        fillOpacity={0.32}
                        strokeWidth={1.5}
                        connectNulls={false}
                        isAnimationActive={false}
                      />
                    ))
                  : null}
                <Area
                  hide={mode === "breakdown"}
                  dataKey="averageMs"
                  type="monotone"
                  stroke="var(--color-averageMs)"
                  strokeWidth={2}
                  fill="url(#fillAverage)"
                  // brak danych w kubełku = przerwa na wykresie, a nie linia do zera
                  connectNulls={false}
                  isAnimationActive={false}
                />
              </AreaChart>
            </ChartContainer>
          ) : responseTimes.isError ? (
            <p className="flex h-72 items-center justify-center text-destructive-foreground text-sm">
              {responseTimes.error.message}
            </p>
          ) : (
            <Skeleton className="h-72 w-full" />
          )}

          {data && !hasData ? (
            <p className="absolute inset-0 flex items-center justify-center text-muted-foreground text-sm">
              No successful checks in this period yet.
            </p>
          ) : null}
        </div>

        {mode === "breakdown" ? (
          <PhaseLegend phases={present} averages={summary?.phases ?? null} />
        ) : null}

        <dl className="grid grid-cols-3 border-t pt-5">
          <SummaryStat
            icon={<WavesIcon className="text-muted-foreground" />}
            label="Average"
            value={summary?.averageMs}
            loading={!summary}
          />
          <SummaryStat
            icon={<ArrowDownToLineIcon className="text-success" />}
            label="Minimum"
            value={summary?.minimumMs}
            loading={!summary}
            className="border-s ps-5"
          />
          <SummaryStat
            icon={<ArrowUpToLineIcon className="text-destructive" />}
            label="Maximum"
            value={summary?.maximumMs}
            loading={!summary}
            className="border-s ps-5"
          />
        </dl>
      </CardPanel>
    </Card>
  );
}

function SummaryStat({
  icon,
  label,
  value,
  loading,
  className,
}: {
  icon: React.ReactNode;
  label: string;
  value: number | null | undefined;
  loading: boolean;
  className?: string;
}) {
  return (
    <div className={cn("flex flex-col gap-1", className)}>
      {/* dt przed dd dla czytników ekranu; wizualnie wartość jest na górze */}
      <dt className="order-last text-muted-foreground text-sm">{label}</dt>
      <dd className="flex items-center gap-2 font-heading text-xl tabular-nums [&_svg]:size-5">
        {icon}
        {loading ? (
          <Skeleton className="h-7 w-20" />
        ) : (
          formatMilliseconds(value)
        )}
      </dd>
    </div>
  );
}

function PhaseLegend({
  phases,
  averages,
}: {
  phases: ResponsePhase[];
  averages: components["schemas"]["PhaseTimingsResponse"] | null;
}) {
  return (
    <ul
      className="-mt-2 flex flex-wrap gap-x-5 gap-y-2 text-sm"
      aria-label="Average time per phase"
    >
      {phases.map((phase) => (
        <li key={phase.key} className="flex items-center gap-2">
          <span
            className="size-2.5 rounded-sm"
            style={{ backgroundColor: phase.color }}
          />
          <span className="text-muted-foreground">{phase.label}</span>
          <span className="font-mono tabular-nums">
            {formatMilliseconds(averages?.[phase.key])}
          </span>
        </li>
      ))}
    </ul>
  );
}

function ResponseTimeTooltip({
  active,
  payload,
  phases,
}: {
  active?: boolean;
  payload?: { payload: ChartPoint }[];
  phases: ResponsePhase[];
}) {
  const point = payload?.[0]?.payload;
  if (!active || !point) return null;

  return (
    <div className="grid min-w-40 gap-1.5 rounded-lg border bg-background px-2.5 py-1.5 text-xs shadow-xl">
      <span className="font-medium">
        {tooltipDateFormatter.format(point.time)}
      </span>
      {point.averageMs == null ? (
        <span className="text-muted-foreground">No successful checks</span>
      ) : phases.length > 0 ? (
        <>
          {phases.map((phase) => (
            <TooltipRow
              key={phase.key}
              label={phase.label}
              value={point[phase.key]}
              color={phase.color}
            />
          ))}
          <span className="mt-0.5 border-t pt-1.5">
            <TooltipRow label="Total" value={point.averageMs} />
          </span>
        </>
      ) : (
        <>
          <TooltipRow label="Average" value={point.averageMs} />
          <TooltipRow label="Minimum" value={point.minimumMs} />
          <TooltipRow label="Maximum" value={point.maximumMs} />
        </>
      )}
    </div>
  );
}

function TooltipRow({
  label,
  value,
  color,
}: {
  label: string;
  value: number | null;
  color?: string;
}) {
  return (
    <span className="flex items-center justify-between gap-4">
      <span className="flex items-center gap-2 text-muted-foreground">
        {color ? (
          <span
            className="size-2.5 rounded-sm"
            style={{ backgroundColor: color }}
          />
        ) : null}
        {label}
      </span>
      <span className="font-mono tabular-nums">
        {formatMilliseconds(value)}
      </span>
    </span>
  );
}
