import { afterEach, describe, expect, it, vi } from 'vitest'

import type { FieldCompanionSessionResponse } from './types'
import { clearSession, saveSession, toStoredSession } from '../auth/sessionStorage'
import { redeemHandoff, renewFieldCompanionSession } from './client'

const sampleSession: FieldCompanionSessionResponse = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  accessExpiresAt: '2026-06-19T12:00:00.000Z',
  refreshExpiresAt: '2026-06-20T12:00:00.000Z',
  sessionId: 'session-id',
  userId: 'user-id',
  personId: 'person-id',
  email: 'user@example.com',
  displayName: 'User Example',
  tenantId: 'tenant-id',
  tenantSlug: 'tenant-slug',
  tenantDisplayName: 'Tenant Display',
  tenantRoleKey: 'tenant_member',
  isPlatformAdmin: false,
  entitlements: ['fieldcompanion'],
  themePreference: 'dark',
  callbackUrl: 'http://localhost:5181/launch',
}

describe('redeemHandoff', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('posts the handoff code to the Field Companion auth endpoint', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          accessToken: 'access-token',
          refreshToken: 'refresh-token',
          accessExpiresAt: '2026-06-19T12:00:00.000Z',
          refreshExpiresAt: '2026-06-20T12:00:00.000Z',
          sessionId: 'session-id',
          userId: 'user-id',
          personId: 'person-id',
          email: 'user@example.com',
          displayName: 'User Example',
          tenantId: 'tenant-id',
          tenantSlug: 'tenant-slug',
          tenantDisplayName: 'Tenant Display',
          tenantRoleKey: 'tenant_member',
          isPlatformAdmin: false,
          entitlements: ['fieldcompanion'],
          themePreference: 'dark',
          callbackUrl: 'http://localhost:5181/launch',
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    await redeemHandoff('handoff-code-123')

    expect(fetchMock).toHaveBeenCalledTimes(1)
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/fieldcompanion/auth/handoff/redeem',
      expect.objectContaining({
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-Stl-Cookie-Session': 'true',
        },
        body: JSON.stringify({ handoffCode: 'handoff-code-123' }),
      }),
    )
  })
})

describe('renewFieldCompanionSession', () => {
  afterEach(() => {
    clearSession()
    vi.restoreAllMocks()
  })

  it('posts the cookie-session renewal request to the shared auth endpoint', async () => {
    saveSession(toStoredSession(sampleSession))

    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          accessToken: 'renewed-access-token',
          accessTokenExpiresAt: '2026-06-19T13:00:00.000Z',
          refreshToken: '',
          refreshTokenExpiresAt: '2026-06-20T13:00:00.000Z',
          sessionId: 'session-id',
          userId: 'user-id',
          tenantId: 'tenant-id',
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    await renewFieldCompanionSession()

    expect(fetchMock).toHaveBeenCalledTimes(1)
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/auth/renew',
      expect.objectContaining({
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-Stl-Cookie-Session': 'true',
        },
      }),
    )
  })
})
