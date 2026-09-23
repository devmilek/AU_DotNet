import { z } from "zod";

export const passwordSchema = z
  .string()
  .min(1, "Password is required.")
  .min(8, "Password must be at least 8 characters.");

export const emailSchema = z.email("Please enter a valid email address.");
