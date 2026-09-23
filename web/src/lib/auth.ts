import "server-only";

import { cache } from "react";
import { redirect } from "next/navigation";
import { getServerApi } from "./api/server";

export const getCurrentUser = cache(async () => {
  const api = await getServerApi();
  const { data, response } = await api.GET("/api/auth/me");

  if (response.status === 401) return null;
  if (!data) throw new Error("Nie udało się pobrać zalogowanego użytkownika.");

  return data;
});

export async function requireUser() {
  const user = await getCurrentUser();
  if (!user) redirect("/sign-in");
  return user;
}

export const getOrganizations = cache(async () => {
  const api = await getServerApi();
  const { data, response, error } = await api.GET("/api/organizations");

  if (response.status === 401) redirect("/sign-in");
  if (!data) {
    throw new Error(error?.title ?? "Nie udało się pobrać organizacji.");
  }

  return data;
});
