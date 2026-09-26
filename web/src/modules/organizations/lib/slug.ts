export const SLUG_MIN_LENGTH = 3;
export const SLUG_MAX_LENGTH = 50;
export const SLUG_PATTERN = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;

function stripDiacritics(value: string): string {
  return value
    .normalize("NFD")
    .replace(/\p{Mn}/gu, "")
    .replace(/ł/g, "l")
    .replace(/Ł/g, "L");
}

export function sanitizeSlugInput(value: string): string {
  return stripDiacritics(value)
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+/, "")
    .slice(0, SLUG_MAX_LENGTH);
}

export function slugify(value: string): string {
  return sanitizeSlugInput(value).replace(/-+$/, "");
}

export function isValidSlug(value: string): boolean {
  return (
    value.length >= SLUG_MIN_LENGTH &&
    value.length <= SLUG_MAX_LENGTH &&
    SLUG_PATTERN.test(value)
  );
}
