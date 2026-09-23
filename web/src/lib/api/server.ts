import "server-only";

import createClient from "openapi-fetch";
import { cookies } from "next/headers";
import { cache } from "react";
import type { paths } from "./schema";

export const getServerApi = cache(async () => {
  const baseUrl = process.env.API_URL;
  if (!baseUrl) {
    throw new Error("Missing API_URL environment variable.");
  }

  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();

  return createClient<paths>({
    baseUrl,
    headers: cookieHeader ? { Cookie: cookieHeader } : {},
  });
});
