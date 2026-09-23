export function formatFieldErrors(errors: unknown[]): string {
  return errors
    .map((error) => {
      if (typeof error === "string") return error;
      if (
        error &&
        typeof error === "object" &&
        "message" in error &&
        typeof error.message === "string"
      ) {
        return error.message;
      }
      return null;
    })
    .filter(Boolean)
    .join(", ");
}
