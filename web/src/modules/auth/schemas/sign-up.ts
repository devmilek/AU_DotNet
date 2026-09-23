import { z } from "zod";
import { emailSchema, passwordSchema } from "./shared";

export const signUpSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required.")
    .min(2, "Name must be at least 2 characters."),
  email: emailSchema,
  password: passwordSchema,
});

export type SignUpValues = z.infer<typeof signUpSchema>;
