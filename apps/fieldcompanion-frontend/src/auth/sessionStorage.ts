import type { FieldCompanionSessionResponse } from '../api/types'
import { clearOfflineQueueState } from '../lib/offlineQueue'

const STORAGE_KEY = 'stl.fieldcompanion.session'

export interface StoredFieldCompanionSession {
  accessToken?: string
  accessTokenExpiresAt: string
  sessionId: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  displayName: string
  email: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
}

let volatileAccessToken: string | null = null

export function toStoredSession(session: FieldCompanionSessionResponse): StoredFieldCompanionSession {
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: session.accessExpiresAt,
    sessionId: session.sessionId,
    userId: session.userId,
    personId: session.personId,
    tenantId: session.tenantId,
    tenantSlug: session.tenantSlug,
    tenantDisplayName: session.tenantDisplayName,
    displayName: session.displayName,
    email: session.email,
    tenantRoleKey: session.tenantRoleKey,
    isPlatformAdmin: session.isPlatformAdmin,
  }
}

export function loadSession(): StoredFieldCompanionSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    const parsed = JSON.parse(raw) as StoredFieldCompanionSession
    if (typeof parsed.accessToken === 'string' && parsed.accessToken.length > 0) {
      volatileAccessToken = parsed.accessToken
    }

    const { accessToken: _accessToken, ...persisted } = parsed
    if (Object.prototype.hasOwnProperty.call(parsed, 'accessToken')) {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(persisted))
    }
    return persisted
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredFieldCompanionSession): void {
  volatileAccessToken =
    typeof session.accessToken === 'string' && session.accessToken.length > 0
      ? session.accessToken
      : null

  const { accessToken: _accessToken, ...persisted } = session
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(persisted))
}

export function clearSession(): void {
  volatileAccessToken = null
  sessionStorage.removeItem(STORAGE_KEY)
  clearOfflineQueueState()
}

export function getAccessToken(session: StoredFieldCompanionSession | null): string | null {
  return typeof session?.accessToken === 'string' && session.accessToken.length > 0
    ? session.accessToken
    : volatileAccessToken
}
