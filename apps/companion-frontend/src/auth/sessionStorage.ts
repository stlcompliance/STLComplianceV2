import type { CompanionSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.companion.session'

export interface StoredCompanionSession {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  displayName: string
  email: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
}

export function toStoredSession(session: CompanionSessionResponse): StoredCompanionSession {
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: session.accessExpiresAt,
    refreshToken: session.refreshToken,
    userId: session.userId,
    personId: session.personId,
    tenantId: session.tenantId,
    tenantSlug: session.tenantSlug,
    displayName: session.displayName,
    email: session.email,
    tenantRoleKey: session.tenantRoleKey,
    isPlatformAdmin: session.isPlatformAdmin,
    entitlements: session.entitlements,
  }
}

export function loadSession(): StoredCompanionSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredCompanionSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredCompanionSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}
