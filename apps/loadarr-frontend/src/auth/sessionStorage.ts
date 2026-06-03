export interface StoredLoadArrSession {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  displayName: string
  email: string
}

const STORAGE_KEY = 'stl.loadarr.session'

export function loadSession(): StoredLoadArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as StoredLoadArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}
