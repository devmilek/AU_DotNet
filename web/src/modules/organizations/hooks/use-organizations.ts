"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { organizationKeys } from "./keys";

export function useOrganizations() {
  return useQuery({
    queryKey: organizationKeys.list(),
    queryFn: async () => {
      const { data, response, error } = await api.GET("/api/organizations");

      if (response.status === 401) {
        throw new Error("Unauthorized");
      }

      if (!data) {
        throw new Error(error?.title ?? "Nie udało się pobrać organizacji.");
      }

      return data;
    },
  });
}
