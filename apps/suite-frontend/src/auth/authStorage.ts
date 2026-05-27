import type { AuthTokenResponse } from '../api/types'

const STORAGE_KEY = 'stl.suite.auth'

export interface StoredAuthSession {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
  sessionId: string
  userId: string
  tenantId: string
}

export function toStoredSession(tokens: AuthTokenResponse): StoredAuthSession {
  return {
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken,
    accessTokenExpiresAt: tokens.accessTokenExpiresAt,
    refreshTokenExpiresAt: tokens.refreshTokenExpiresAt,
    sessionId: tokens.sessionId,
    userId: tokens.userId,
    tenantId: tokens.tenantId,
  }
}

export function loadAuthSession(): StoredAuthSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredAuthSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveAuthSession(session: StoredAuthSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearAuthSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function isAccessTokenExpired(
  session: StoredAuthSession,
  skewSeconds = 30,
): boolean {
  const expiresAt = Date.parse(session.accessTokenExpiresAt)
  if (Number.isNaN(expiresAt)) {
    return true
  }
  return Date.now() >= expiresAt - skewSeconds * 1000
}
