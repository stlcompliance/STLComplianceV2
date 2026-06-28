export function formatLoadArrMutationFailure(action: string): string {
  return `${action} failed. The API write was not confirmed, so no local record was created.`
}

export function formatLoadArrDependencyFailure(surface: string, status?: number): string {
  if (status === 401) {
    return `${surface} is unavailable because the current session is no longer valid. Sign in again and retry.`
  }

  if (status === 403) {
    return `${surface} is unavailable because this user does not have permission to read the current LoadArr data.`
  }

  if (status === 404) {
    return `${surface} is unavailable because the requested LoadArr resource was not found.`
  }

  return `${surface} is unavailable right now. LoadArr did not return authoritative data, so this view stays hidden until the API responds.`
}
