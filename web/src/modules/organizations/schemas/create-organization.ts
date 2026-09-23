import { z } from "zod";

export const createOrganizationSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required.")
    .min(2, "Name must be at least 2 characters.")
    .max(80, "Name must be at most 80 characters."),
});

export type CreateOrganizationValues = z.infer<typeof createOrganizationSchema>;
