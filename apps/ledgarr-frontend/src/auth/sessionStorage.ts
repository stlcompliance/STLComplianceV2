import type { LedgArrHandoffSessionResponse } from '../api/client'

const STORAGE_KEY = 'stl.ledgarr.session'

export interface StoredLedgArrSession {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  displayName: string
  email: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  launchableProductKeys: string[]
}

export function toStoredSession(session: LedgArrHandoffSessionResponse): StoredLedgArrSession {
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
    tenantRoleKey: session.tenantRoleKey,
    isPlatformAdmin: session.isPlatformAdmin,
    launchableProductKeys: session.launchableProductKeys,
  }
}

export function loadSession(): StoredLedgArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as StoredLedgArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredLedgArrSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}
