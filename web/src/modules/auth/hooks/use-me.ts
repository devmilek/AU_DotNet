"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { authKeys } from "./keys";

export function useMe() {
  return useQuery({
    queryKey: authKeys.me(),
    queryFn: async () => {
      const { data, response, error } = await api.GET("/api/auth/me");

      if (response.status === 401) {
        throw new Error("Unauthorized");
      }

      if (!data) {
        throw new Error(error?.title ?? "Nie udało się pobrać użytkownika.");
      }

      return data;
    },
  });
}
