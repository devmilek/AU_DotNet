import { z } from "zod";
import {
  SLUG_MAX_LENGTH,
  SLUG_MIN_LENGTH,
  SLUG_PATTERN,
} from "../lib/slug";

export const createOrganizationSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required.")
    .min(2, "Name must be at least 2 characters.")
    .max(80, "Name must be at most 80 characters."),
  slug: z
    .string()
    .min(1, "Slug is required.")
    .min(SLUG_MIN_LENGTH, `Slug must be at least ${SLUG_MIN_LENGTH} characters.`)
    .max(SLUG_MAX_LENGTH, `Slug must be at most ${SLUG_MAX_LENGTH} characters.`)
    .regex(
      SLUG_PATTERN,
      "Use lowercase letters, digits and single hyphens (not at the start or end).",
    ),
});

export type CreateOrganizationValues = z.infer<typeof createOrganizationSchema>;
