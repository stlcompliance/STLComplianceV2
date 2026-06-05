import type { LoadArrHandoffSessionResponse } from '../api/client'

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

export function toStoredSession(session: LoadArrHandoffSessionResponse): StoredLoadArrSession {
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: session.accessTokenExpiresAt,
    userId: session.userId,
    personId: session.personId,
    tenantId: session.tenantId,
    tenantSlug: session.tenantSlug,
    tenantDisplayName: session.tenantDisplayName,
    displayName: session.displayName,
    email: session.email,
  }
}

export function loadSession(): StoredLoadArrSession | null {
  let raw: string | null = null
  try {
    raw = sessionStorage.getItem(STORAGE_KEY)
  } catch {
    return null
  }
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
  try {
    sessionStorage.removeItem(STORAGE_KEY)
  } catch {
    // Ignore storage access issues in restricted browser contexts.
  }
}

export function saveSession(session: StoredLoadArrSession): void {
  try {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
  } catch {
    // Ignore storage access issues in restricted browser contexts.
  }
}
