import type { OrdArrHandoffSessionResponse } from '../api/client'

const STORAGE_KEY = 'stl.ordarr.session'

export interface StoredOrdArrSession {
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
  entitlements: string[]
}

export function toStoredSession(session: OrdArrHandoffSessionResponse): StoredOrdArrSession {
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
    entitlements: session.entitlements,
  }
}

export function loadSession(): StoredOrdArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as StoredOrdArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredOrdArrSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}
