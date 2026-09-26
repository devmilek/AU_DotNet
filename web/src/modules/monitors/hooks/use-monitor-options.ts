"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/errors";
import { monitorKeys } from "./keys";

/** Maksymalny rozmiar strony w API — wystarczy do wyboru monitorów w formularzach. */
const OPTIONS_LIMIT = 100;

/** Lekka lista monitorów organizacji do pickerów (alfabetycznie, bez paginacji w UI). */
export function useMonitorOptions(organizationId: string) {
  return useQuery({
    queryKey: monitorKeys.options(organizationId),
    queryFn: async () => {
      const page = await unwrap(
        api.GET("/api/{orgId}/monitors", {
          params: {
            path: { orgId: organizationId },
            query: {
              pageSize: OPTIONS_LIMIT,
              sortBy: "CreatedAt",
              sortOrder: "Asc",
            },
          },
        }),
        "Nie udało się pobrać monitorów.",
      );

      return {
        monitors: page.items
          .map(({ monitor }) => monitor)
          .sort((a, b) => a.name.localeCompare(b.name)),
        truncated: page.totalCount > page.items.length,
      };
    },
  });
}
