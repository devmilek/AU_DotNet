import { z } from "zod";

export const MAX_ORGANIZATION_NAME_LENGTH = 100;

export const organizationNameSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required.")
    .max(
      MAX_ORGANIZATION_NAME_LENGTH,
      `Name must be at most ${MAX_ORGANIZATION_NAME_LENGTH} characters.`,
    ),
});

export const inviteMemberSchema = z.object({
  email: z.string().trim().pipe(z.email("Enter a valid email address.")),
  role: z.number(),
});
