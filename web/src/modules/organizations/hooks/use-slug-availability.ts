"use client";

import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/errors";
import { isValidSlug } from "../lib/slug";
import { organizationKeys } from "./keys";

const DEBOUNCE_MS = 400;

export type SlugStatus = "idle" | "checking" | "available" | "taken";

export function useSlugAvailability(slug: string): SlugStatus {
  const [debouncedSlug, setDebouncedSlug] = useState(slug);

  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedSlug(slug), DEBOUNCE_MS);
    return () => clearTimeout(timeout);
  }, [slug]);

  const enabled = isValidSlug(debouncedSlug);

  const query = useQuery({
    queryKey: organizationKeys.slugAvailability(debouncedSlug),
    queryFn: () =>
      unwrap(
        api.GET("/api/organizations/slug-availability", {
          params: { query: { slug: debouncedSlug } },
        }),
        "Could not check slug availability.",
      ),
    enabled,
    staleTime: 30_000,
  });

  if (!isValidSlug(slug)) return "idle";
  if (slug !== debouncedSlug || query.isFetching) return "checking";
  if (!query.data) return "idle";
  return query.data.available ? "available" : "taken";
}
