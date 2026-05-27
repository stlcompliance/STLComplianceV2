import { afterEach, describe, expect, it } from 'vitest'
import type { AuthTokenResponse } from '../api/types'
import {
  clearAuthSession,
  isAccessTokenExpired,
  loadAuthSession,
  saveAuthSession,
  toStoredSession,
} from './authStorage'

const sampleTokens: AuthTokenResponse = {
  accessToken: 'access',
  refreshToken: 'refresh',
  accessTokenExpiresAt: new Date(Date.now() + 60_000).toISOString(),
  refreshTokenExpiresAt: new Date(Date.now() + 86_400_000).toISOString(),
  sessionId: '11111111-1111-1111-1111-111111111199',
  userId: '22222222-2222-2222-2222-222222222201',
  tenantId: '11111111-1111-1111-1111-111111111101',
}

afterEach(() => {
  clearAuthSession()
})

describe('authStorage', () => {
  it('round-trips session in sessionStorage', () => {
    const session = toStoredSession(sampleTokens)
    saveAuthSession(session)
    expect(loadAuthSession()).toEqual(session)
  })

  it('detects expired access tokens with skew', () => {
    const session = toStoredSession({
      ...sampleTokens,
      accessTokenExpiresAt: new Date(Date.now() - 1_000).toISOString(),
    })
    expect(isAccessTokenExpired(session, 0)).toBe(true)
  })
})
