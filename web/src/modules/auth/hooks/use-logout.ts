"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api/client";
import { authKeys } from "./keys";
import { organizationKeys } from "@/modules/organizations/hooks/keys";

export function useLogout(redirectTo = "/sign-in") {
  const router = useRouter();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      const { response, error } = await api.POST("/api/auth/logout");

      if (!response.ok) {
        throw new Error(error?.title ?? "Nie udało się wylogować.");
      }
    },
    onSuccess: async () => {
      await queryClient.removeQueries({ queryKey: authKeys.all });
      await queryClient.removeQueries({ queryKey: organizationKeys.all });
      router.push(redirectTo);
      router.refresh();
    },
  });
}
