import { z } from "zod";
import type { components } from "@/lib/api/schema";

type HttpCheckMethod = components["schemas"]["HttpCheckMethod"];
type HttpAuthType = components["schemas"]["HttpAuthType"];
type StatusCodeRange = components["schemas"]["StatusCodeRangeSettings"];

export const httpMethods = [
  "Get",
  "Head",
  "Post",
  "Put",
  "Patch",
  "Delete",
  "Options",
] as const satisfies readonly HttpCheckMethod[];

export const httpAuthTypes = [
  "None",
  "Basic",
  "Bearer",
] as const satisfies readonly HttpAuthType[];

// minimalny interval na backendzie to 5 s
export const intervalOptions = [
  { value: 30, label: "30 seconds" },
  { value: 60, label: "1 minute" },
  { value: 180, label: "3 minutes" },
  { value: 300, label: "5 minutes" },
  { value: 600, label: "10 minutes" },
  { value: 1800, label: "30 minutes" },
  { value: 3600, label: "1 hour" },
] as const;

/** Parsuje "200-299, 301, 418" na zakresy; zwraca null, jeśli format jest niepoprawny. */
export function parseStatusCodes(input: string): StatusCodeRange[] | null {
  const parts = input
    .split(",")
    .map((part) => part.trim())
    .filter(Boolean);

  if (parts.length === 0) return null;

  const ranges: StatusCodeRange[] = [];
  for (const part of parts) {
    const match = /^(\d{3})(?:\s*-\s*(\d{3}))?$/.exec(part);
    if (!match) return null;

    const from = Number(match[1]);
    const to = Number(match[2] ?? match[1]);
    if (from < 100 || to > 599 || from > to) return null;

    ranges.push({ from, to });
  }

  return ranges;
}

export const createHttpMonitorSchema = z
  .object({
    name: z
      .string()
      .trim()
      .min(1, "Name is required.")
      .min(3, "Name must be at least 3 characters.")
      .max(100, "Name must be at most 100 characters."),
    target: z
      .string()
      .trim()
      .min(1, "URL is required.")
      .max(2048, "URL must be at most 2048 characters.")
      .refine((value) => {
        try {
          const url = new URL(value);
          return url.protocol === "http:" || url.protocol === "https:";
        } catch {
          return false;
        }
      }, "Enter a full URL starting with http:// or https://."),
    intervalSeconds: z.number().int().min(5),
    timeoutMs: z
      .number({ error: "Timeout is required." })
      .int()
      .min(100, "Timeout must be at least 100 ms."),
    alertThreshold: z
      .number({ error: "Required." })
      .int()
      .min(1, "Must be at least 1.")
      .max(100, "Must be at most 100."),
    recoveryThreshold: z
      .number({ error: "Required." })
      .int()
      .min(1, "Must be at least 1.")
      .max(100, "Must be at most 100."),
    method: z.enum(httpMethods),
    followRedirects: z.boolean(),
    acceptedStatusCodes: z
      .string()
      .refine(
        (value) => parseStatusCodes(value) !== null,
        "Use codes or ranges between 100 and 599, e.g. 200-299, 301.",
      )
      .refine(
        (value) => (parseStatusCodes(value)?.length ?? 0) <= 20,
        "At most 20 ranges.",
      ),
    authType: z.enum(httpAuthTypes),
    username: z.string(),
    password: z.string(),
    token: z.string(),
  })
  .superRefine((values, ctx) => {
    // timeout musi się zmieścić w intervalu, inaczej checki się nakładają
    if (values.timeoutMs >= values.intervalSeconds * 1000) {
      ctx.addIssue({
        code: "custom",
        path: ["timeoutMs"],
        message: "Timeout must be shorter than the check interval.",
      });
    }

    if (values.authType === "Basic") {
      if (!values.username.trim()) {
        ctx.addIssue({
          code: "custom",
          path: ["username"],
          message: "Username is required.",
        });
      } else if (values.username.includes(":")) {
        ctx.addIssue({
          code: "custom",
          path: ["username"],
          message: "Username cannot contain a colon.",
        });
      }

      if (!values.password) {
        ctx.addIssue({
          code: "custom",
          path: ["password"],
          message: "Password is required.",
        });
      }
    }

    if (values.authType === "Bearer" && !values.token.trim()) {
      ctx.addIssue({
        code: "custom",
        path: ["token"],
        message: "Token is required.",
      });
    }
  });

export type CreateHttpMonitorValues = z.input<typeof createHttpMonitorSchema>;

export const createHttpMonitorDefaults: CreateHttpMonitorValues = {
  name: "",
  target: "",
  intervalSeconds: 60,
  timeoutMs: 5000,
  alertThreshold: 3,
  recoveryThreshold: 3,
  method: "Get",
  followRedirects: true,
  acceptedStatusCodes: "200-299",
  authType: "None",
  username: "",
  password: "",
  token: "",
};

export function toCreateMonitorRequest(
  values: CreateHttpMonitorValues,
): components["schemas"]["CreateMonitorRequest"] {
  return {
    name: values.name.trim(),
    type: "Http",
    target: values.target.trim(),
    intervalSeconds: values.intervalSeconds,
    timeoutMs: values.timeoutMs,
    alertThreshold: values.alertThreshold,
    recoveryThreshold: values.recoveryThreshold,
    http: {
      method: values.method,
      followRedirects: values.followRedirects,
      acceptedStatusCodes: parseStatusCodes(values.acceptedStatusCodes),
      auth:
        values.authType === "None"
          ? null
          : {
              type: values.authType,
              username:
                values.authType === "Basic" ? values.username.trim() : null,
              password: values.authType === "Basic" ? values.password : null,
              token: values.authType === "Bearer" ? values.token.trim() : null,
            },
    },
  };
}
