import type { FieldCompanionSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.fieldcompanion.session'

export interface StoredFieldCompanionSession {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
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

export function toStoredSession(session: FieldCompanionSessionResponse): StoredFieldCompanionSession {
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: session.accessExpiresAt,
    refreshToken: session.refreshToken,
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

export function loadSession(): StoredFieldCompanionSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredFieldCompanionSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredFieldCompanionSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}
