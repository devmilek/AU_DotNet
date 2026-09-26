import type { components } from "@/lib/api/schema";

type PhaseTimings = components["schemas"]["PhaseTimingsResponse"];
type ResponseTimePoint = components["schemas"]["ResponseTimePointResponse"];

export const responsePhases = [
  { key: "dnsMs", label: "DNS", color: "oklch(62.3% 0.214 259.815)" },
  { key: "connectMs", label: "Connect", color: "oklch(69.6% 0.17 162.48)" },
  { key: "tlsMs", label: "TLS", color: "oklch(76.9% 0.188 70.08)" },
  { key: "ttfbMs", label: "TTFB", color: "oklch(60.6% 0.25 292.717)" },
  { key: "transferMs", label: "Transfer", color: "oklch(64.5% 0.246 16.439)" },
] as const;

export type PhaseKey = (typeof responsePhases)[number]["key"];

export type ResponsePhase = (typeof responsePhases)[number];

export function presentPhases(points: ResponseTimePoint[]): ResponsePhase[] {
  return responsePhases.filter((phase) =>
    points.some((point) => point.phases?.[phase.key] != null),
  );
}

export function phaseValues(
  phases: PhaseTimings | null,
  present: ResponsePhase[],
): Record<PhaseKey, number | null> {
  return Object.fromEntries(
    responsePhases.map((phase) => [
      phase.key,
      phases && present.includes(phase) ? (phases[phase.key] ?? 0) : null,
    ]),
  ) as Record<PhaseKey, number | null>;
}
