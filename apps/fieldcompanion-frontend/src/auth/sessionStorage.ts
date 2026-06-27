import type { FieldCompanionSessionResponse } from '../api/types'
import { clearOfflineQueueState } from '../lib/offlineQueue'
import { isFieldCompanionAccessTokenExpired } from '../lib/sessionSafety'
import { clearSubmissionState } from '../lib/submissionState'

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
    if (
      typeof parsed.accessToken === 'string' &&
      parsed.accessToken.length > 0 &&
      !isFieldCompanionAccessTokenExpired(parsed)
    ) {
      volatileAccessToken = parsed.accessToken
    } else if (typeof parsed.accessToken === 'string' && parsed.accessToken.length > 0) {
      volatileAccessToken = null
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
  const accessToken = volatileAccessToken
  volatileAccessToken = null
  sessionStorage.removeItem(STORAGE_KEY)
  clearOfflineQueueState()
  clearSubmissionState()

  if (!accessToken) {
    return
  }

  // Best-effort cleanup: logout should also revoke any stale push subscription.
  void import('../lib/pushNotifications')
    .then(({ removeFieldCompanionPushSubscription }) => removeFieldCompanionPushSubscription(accessToken))
    .catch(() => {})
}

export function getAccessToken(session: StoredFieldCompanionSession | null): string | null {
  if (!session || isFieldCompanionAccessTokenExpired(session)) {
    volatileAccessToken = null
    return null
  }

  return typeof session.accessToken === 'string' && session.accessToken.length > 0
    ? session.accessToken
    : volatileAccessToken
}
