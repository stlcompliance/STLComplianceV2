export function isProductWorkspaceAuthError(error: unknown): boolean {
  if (typeof error !== 'object' || error === null || !('status' in error)) {
    return false
  }

  const status = (error as { status: unknown }).status
  return status === 401 || status === 403
}

export function resolveProductWorkspaceBootstrapError(
  error: unknown,
): 'forbidden' | 'expired' | null {
  if (!isProductWorkspaceAuthError(error)) {
    return null
  }

  const status = (error as { status: number }).status
  return status === 403 ? 'forbidden' : 'expired'
}
