let cachedTimeZones: string[] | null = null;
const labels = new Map<string, string>();

export function browserTimeZone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
}

export function listTimeZones(): string[] {
  if (!cachedTimeZones) {
    const zones = Intl.supportedValuesOf("timeZone");
    cachedTimeZones = zones.includes("UTC") ? zones : ["UTC", ...zones];
  }
  return cachedTimeZones;
}

export function timeZoneOffsetLabel(timeZone: string, at = new Date()): string {
  const offset = new Intl.DateTimeFormat("en-US", {
    timeZone,
    timeZoneName: "longOffset",
  })
    .formatToParts(at)
    .find((part) => part.type === "timeZoneName")?.value;

  if (!offset || offset === "GMT") return "UTC";
  return offset.replace("GMT", "UTC");
}

export function formatTimeZone(timeZone: string): string {
  let label = labels.get(timeZone);
  if (!label) {
    label = `${timeZone.replaceAll("_", " ")} (${timeZoneOffsetLabel(timeZone)})`;
    labels.set(timeZone, label);
  }
  return label;
}
