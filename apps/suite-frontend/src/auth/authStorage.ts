import type { AuthTokenResponse } from '../api/types'

const STORAGE_KEY = 'stl.suite.auth'

export interface StoredAuthSession {
  accessToken?: string
  refreshToken?: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
  sessionId: string
  userId: string
  tenantId: string
}

let volatileAccessToken: string | null = null
let volatileRefreshToken: string | null = null

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
    const parsed = JSON.parse(raw) as StoredAuthSession
    if (typeof parsed.accessToken === 'string' && parsed.accessToken.length > 0) {
      volatileAccessToken = parsed.accessToken
    }
    if (typeof parsed.refreshToken === 'string' && parsed.refreshToken.length > 0) {
      volatileRefreshToken = parsed.refreshToken
    }

    const { accessToken: _accessToken, refreshToken: _refreshToken, ...persisted } = parsed
    if (Object.prototype.hasOwnProperty.call(parsed, 'accessToken')
      || Object.prototype.hasOwnProperty.call(parsed, 'refreshToken')) {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(persisted))
    }
    return persisted
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveAuthSession(session: StoredAuthSession): void {
  volatileAccessToken =
    typeof session.accessToken === 'string' && session.accessToken.length > 0
      ? session.accessToken
      : null
  volatileRefreshToken =
    typeof session.refreshToken === 'string' && session.refreshToken.length > 0
      ? session.refreshToken
      : null

  const { accessToken: _accessToken, refreshToken: _refreshToken, ...persisted } = session
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(persisted))
}

export function clearAuthSession(): void {
  volatileAccessToken = null
  volatileRefreshToken = null
  sessionStorage.removeItem(STORAGE_KEY)
}

export function getAccessToken(session: StoredAuthSession | null): string | null {
  return typeof session?.accessToken === 'string' && session.accessToken.length > 0
    ? session.accessToken
    : volatileAccessToken
}

export function getRefreshToken(session: StoredAuthSession | null): string | null {
  return typeof session?.refreshToken === 'string' && session.refreshToken.length > 0
    ? session.refreshToken
    : volatileRefreshToken
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
