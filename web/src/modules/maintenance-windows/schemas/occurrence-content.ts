import { z } from "zod";
import {
  MAX_DESCRIPTION_LENGTH,
  MAX_NAME_LENGTH,
} from "./create-maintenance-window";

export const occurrenceContentSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required.")
    .max(MAX_NAME_LENGTH, `Name must be at most ${MAX_NAME_LENGTH} characters.`),
  description: z
    .string()
    .trim()
    .max(
      MAX_DESCRIPTION_LENGTH,
      `Description must be at most ${MAX_DESCRIPTION_LENGTH} characters.`,
    ),
});

export type OccurrenceContentValues = z.input<typeof occurrenceContentSchema>;
