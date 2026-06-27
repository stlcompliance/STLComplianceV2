import type { StoredFieldCompanionSession } from '../auth/sessionStorage'

export const FIELD_COMPANION_SESSION_REFRESH_WARNING_MINUTES = 15
const ACCESS_TOKEN_EXPIRY_SKEW_SECONDS = 30
const ACCESS_TOKEN_EXPIRY_SKEW_MS = ACCESS_TOKEN_EXPIRY_SKEW_SECONDS * 1000
const SESSION_REFRESH_WARNING_MS = FIELD_COMPANION_SESSION_REFRESH_WARNING_MINUTES * 60_000

export type FieldCompanionSessionHealthTone = 'success' | 'warning' | 'danger'

export interface FieldCompanionSessionHealthSnapshot {
  accessTokenExpiresAt: string
  isAccessExpired: boolean
  isAccessExpiringSoon: boolean
  renewalDeadlineMs: number | null
  statusLabel: string
  tone: FieldCompanionSessionHealthTone
  warningWindowLabel: string
}

export function getFieldCompanionSessionAccessTokenRenewalDeadlineMs(
  session: Pick<StoredFieldCompanionSession, 'accessTokenExpiresAt'>,
): number | null {
  const expiresAtMs = Date.parse(session.accessTokenExpiresAt)
  if (Number.isNaN(expiresAtMs)) {
    return null
  }

  return expiresAtMs - ACCESS_TOKEN_EXPIRY_SKEW_MS
}

export function isFieldCompanionAccessTokenExpired(
  session: Pick<StoredFieldCompanionSession, 'accessTokenExpiresAt'>,
  now = new Date(),
): boolean {
  const renewalDeadlineMs = getFieldCompanionSessionAccessTokenRenewalDeadlineMs(session)
  if (renewalDeadlineMs === null) {
    return true
  }

  return now.getTime() >= renewalDeadlineMs
}

export function summarizeFieldCompanionSession(
  session: Pick<StoredFieldCompanionSession, 'accessTokenExpiresAt'>,
  now = new Date(),
): FieldCompanionSessionHealthSnapshot {
  const renewalDeadlineMs = getFieldCompanionSessionAccessTokenRenewalDeadlineMs(session)
  if (renewalDeadlineMs === null) {
    return {
      accessTokenExpiresAt: session.accessTokenExpiresAt,
      isAccessExpired: true,
      isAccessExpiringSoon: false,
      renewalDeadlineMs: null,
      statusLabel: 'Session needs refresh',
      tone: 'danger',
      warningWindowLabel: `${FIELD_COMPANION_SESSION_REFRESH_WARNING_MINUTES}m`,
    }
  }

  const isAccessExpired = now.getTime() >= renewalDeadlineMs
  const expiresAtMs = Date.parse(session.accessTokenExpiresAt)
  const isAccessExpiringSoon =
    !isAccessExpired && expiresAtMs - now.getTime() <= SESSION_REFRESH_WARNING_MS

  return {
    accessTokenExpiresAt: session.accessTokenExpiresAt,
    isAccessExpired,
    isAccessExpiringSoon,
    renewalDeadlineMs,
    statusLabel: isAccessExpired
      ? 'Session expired'
      : isAccessExpiringSoon
        ? 'Refresh recommended'
        : 'Session active',
    tone: isAccessExpired ? 'danger' : isAccessExpiringSoon ? 'warning' : 'success',
    warningWindowLabel: `${FIELD_COMPANION_SESSION_REFRESH_WARNING_MINUTES}m`,
  }
}
