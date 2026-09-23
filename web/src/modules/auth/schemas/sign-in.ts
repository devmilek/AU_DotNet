import { z } from "zod";
import { emailSchema, passwordSchema } from "./shared";

export const signInSchema = z.object({
  email: emailSchema,
  password: passwordSchema,
});

export type SignInValues = z.infer<typeof signInSchema>;
