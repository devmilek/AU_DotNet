import { z } from "zod";
import type { components } from "@/lib/api/schema";

export const MAX_EMAIL_RECIPIENTS = 20;

export const createEmailChannelSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required.")
    .max(200, "Name must be at most 200 characters."),
  // obiekty zamiast stringów — pola tablicowe TanStack Form potrzebują stabilnych elementów
  recipients: z
    .array(
      z.object({
        email: z.string().trim().pipe(z.email("Enter a valid email address.")),
      }),
    )
    .min(1, "Add at least one recipient.")
    .max(MAX_EMAIL_RECIPIENTS, `At most ${MAX_EMAIL_RECIPIENTS} recipients.`)
    .refine(
      (recipients) =>
        new Set(recipients.map((r) => r.email.trim().toLowerCase())).size ===
        recipients.length,
      "Each address can be added only once.",
    ),
  monitorIds: z.array(z.string()),
});

export type CreateEmailChannelValues = z.input<typeof createEmailChannelSchema>;

export const createEmailChannelDefaults: CreateEmailChannelValues = {
  name: "",
  recipients: [{ email: "" }],
  monitorIds: [],
};

export function toCreateChannelRequest(
  values: CreateEmailChannelValues,
): components["schemas"]["CreateNotificationChannelRequest"] {
  return {
    name: values.name.trim(),
    type: "Email",
    email: { to: values.recipients.map((r) => r.email.trim()) },
    monitorIds: values.monitorIds,
  };
}
