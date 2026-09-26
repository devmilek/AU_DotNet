import { api } from "@/lib/api/client";
import { unwrap } from "@/lib/api/errors";

export const LOGO_MAX_BYTES = 2 * 1024 * 1024;
export const LOGO_TYPES = ["image/png", "image/jpeg", "image/webp"];
export const LOGO_ACCEPT = LOGO_TYPES.join(",");

export function validateLogoFile(file: File): string | null {
  if (!LOGO_TYPES.includes(file.type)) {
    return "The logo must be a PNG, JPG or WebP image.";
  }
  if (file.size > LOGO_MAX_BYTES) {
    return `The logo can be at most ${LOGO_MAX_BYTES / 1024 / 1024} MB.`;
  }
  return null;
}

export function logoInitials(name: string): string | null {
  const letters = name.replace(/[^\p{L}\p{N}]/gu, "").slice(0, 2);
  return letters ? letters.toUpperCase() : null;
}

export function uploadOrganizationLogo(organizationId: string, file: File) {
  return unwrap(
    api.PUT("/api/organizations/{orgId}/logo", {
      params: { path: { orgId: organizationId } },
      body: { file: "" },
      bodySerializer: () => {
        const formData = new FormData();
        formData.append("file", file);
        return formData;
      },
    }),
    "Could not upload the logo.",
  );
}
