"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import {
  type MonitorsParams,
  toApiQuery,
} from "@/modules/monitors/lib/search-params";
import { monitorKeys } from "./keys";

export function useMonitors(organizationId: string, params: MonitorsParams) {
  return useQuery({
    queryKey: monitorKeys.list(organizationId, params),
    queryFn: async () => {
      const { data, response, error } = await api.GET("/api/{orgId}/monitors", {
        params: {
          path: { orgId: organizationId },
          query: toApiQuery(params),
        },
      });

      if (response.status === 401) {
        throw new Error("Unauthorized");
      }

      if (!data) {
        throw new Error(error?.title ?? "Nie udało się pobrać monitorów.");
      }

      return data;
    },
    // przy zmianie filtrów/strony zostawiamy poprzednią listę zamiast skeletona
    placeholderData: keepPreviousData,
  });
}
