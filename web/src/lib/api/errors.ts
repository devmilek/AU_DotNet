import type { components } from "./schema";

type ProblemDetails = components["schemas"]["ProblemDetails"];

/** Błąd odpowiedzi API z kodem HTTP i (jeśli jest) treścią ProblemDetails. */
export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly problem?: ProblemDetails,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

type FetchResult = { data?: unknown; error?: unknown; response: Response };

function toApiError(
  { error, response }: FetchResult,
  fallbackMessage: string,
): ApiError {
  const problem = error as ProblemDetails | undefined;
  return new ApiError(
    problem?.detail ?? problem?.title ?? fallbackMessage,
    response.status,
    problem,
  );
}

/** Zwraca `data` z odpowiedzi openapi-fetch albo rzuca {@link ApiError}. */
export async function unwrap<TResult extends FetchResult>(
  request: Promise<TResult>,
  fallbackMessage: string,
): Promise<NonNullable<TResult["data"]>> {
  const result = await request;
  if (!result.response.ok || result.data === undefined) {
    throw toApiError(result, fallbackMessage);
  }
  return result.data as NonNullable<TResult["data"]>;
}

/** Dla endpointów bez treści (204/202) — sprawdza tylko status. */
export async function ensureOk(
  request: Promise<FetchResult>,
  fallbackMessage: string,
): Promise<void> {
  const result = await request;
  if (!result.response.ok) {
    throw toApiError(result, fallbackMessage);
  }
}
