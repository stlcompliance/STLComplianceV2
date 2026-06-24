export function formatLoadArrMutationFailure(action: string): string {
  return `${action} failed. The API write was not confirmed, so no local record was created.`
}
