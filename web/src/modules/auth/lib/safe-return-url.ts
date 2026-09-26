export function safeReturnUrl(value: string | string[] | undefined): string | null {
  const url = Array.isArray(value) ? value[0] : value;
  if (!url || !url.startsWith("/") || url.startsWith("//") || url.startsWith("/\\")) {
    return null;
  }
  return url;
}
