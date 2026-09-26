import type { components } from "@/lib/api/schema";

type ApiMonitorType = components["schemas"]["MonitorType"];

export const monitorTypes = ["http", "tcp", "ping", "ssl"] as const;
export type MonitorTypeParam = (typeof monitorTypes)[number];

export const monitorTypeLabels: Record<MonitorTypeParam, string> = {
  http: "HTTP",
  tcp: "TCP",
  ping: "Ping",
  ssl: "SSL",
};

export const sortByOptions = ["createdAt", "status"] as const;
export type SortByParam = (typeof sortByOptions)[number];

export const sortByLabels: Record<SortByParam, string> = {
  createdAt: "Created",
  status: "Status",
};

export type SortOrderParam = "asc" | "desc";

export type MonitorsFilters = {
  search: string;
  types: MonitorTypeParam[];
  sortBy: SortByParam;
  sortOrder: SortOrderParam;
};

export type MonitorsParams = MonitorsFilters & { page: number };

export const defaultMonitorsFilters: MonitorsFilters = {
  search: "",
  types: [],
  sortBy: "createdAt",
  sortOrder: "desc",
};

export const MONITORS_PAGE_SIZE = 20;

function isOneOf<T extends string>(
  values: readonly T[],
  value: string | null,
): value is T {
  return value !== null && (values as readonly string[]).includes(value);
}

// typy zawsze w tej samej kolejności, żeby ten sam filtr dawał ten sam URL i query key
export function normalizeTypes(types: Iterable<string>): MonitorTypeParam[] {
  const selected = new Set(Array.from(types, (type) => type.toLowerCase()));
  return monitorTypes.filter((type) => selected.has(type));
}

export function parseMonitorsParams(
  searchParams: Pick<URLSearchParams, "get">,
): MonitorsParams {
  const sortBy = searchParams.get("sortBy");
  const sortOrder = searchParams.get("sortOrder");
  const page = Number(searchParams.get("page"));

  return {
    search: searchParams.get("search")?.trim() ?? "",
    types: normalizeTypes(searchParams.get("type")?.split(",") ?? []),
    sortBy: isOneOf(sortByOptions, sortBy)
      ? sortBy
      : defaultMonitorsFilters.sortBy,
    sortOrder: isOneOf(["asc", "desc"], sortOrder)
      ? sortOrder
      : defaultMonitorsFilters.sortOrder,
    page: Number.isInteger(page) && page > 1 ? page : 1,
  };
}

// wartości domyślne pomijamy, żeby URL był krótki
export function buildMonitorsSearch(params: MonitorsParams): string {
  const query = new URLSearchParams();
  const search = params.search.trim();

  if (search) query.set("search", search);
  if (params.types.length > 0) query.set("type", params.types.join(","));
  if (params.sortBy !== defaultMonitorsFilters.sortBy) {
    query.set("sortBy", params.sortBy);
  }
  if (params.sortOrder !== defaultMonitorsFilters.sortOrder) {
    query.set("sortOrder", params.sortOrder);
  }
  if (params.page > 1) query.set("page", String(params.page));

  const value = query.toString();
  return value ? `?${value}` : "";
}

export function areFiltersEqual(a: MonitorsFilters, b: MonitorsFilters) {
  return (
    a.search.trim() === b.search.trim() &&
    a.sortBy === b.sortBy &&
    a.sortOrder === b.sortOrder &&
    a.types.join(",") === b.types.join(",")
  );
}

export function hasActiveFilters(filters: MonitorsFilters) {
  return filters.search.trim() !== "" || filters.types.length > 0;
}

export function toApiQuery(params: MonitorsParams) {
  return {
    page: params.page,
    pageSize: MONITORS_PAGE_SIZE,
    sortBy: params.sortBy === "status" ? "Status" : "CreatedAt",
    sortOrder: params.sortOrder === "asc" ? "Asc" : "Desc",
    type: params.types.length > 0 ? params.types.join(",") : undefined,
    search: params.search.trim() || undefined,
  } as const;
}

export function monitorTypeLabel(type: ApiMonitorType | undefined) {
  if (!type) return "Unknown";
  return monitorTypeLabels[type.toLowerCase() as MonitorTypeParam] ?? type;
}
