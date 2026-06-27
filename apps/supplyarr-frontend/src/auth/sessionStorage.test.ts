import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { HandoffSessionResponse } from '../api/types'
import { clearSession, loadSession, saveSession, toStoredSession } from './sessionStorage'

function createSessionStorageMock() {
  const store = new Map<string, string>()

  return {
    getItem: vi.fn((key: string) => store.get(key) ?? null),
    setItem: vi.fn((key: string, value: string) => {
      store.set(key, value)
    }),
    removeItem: vi.fn((key: string) => {
      store.delete(key)
    }),
  }
}

describe('supplyarr sessionStorage', () => {
  const sampleSession: HandoffSessionResponse = {
    accessToken: 'access-token',
    accessTokenExpiresAt: new Date(Date.now() + 60_000).toISOString(),
    userId: 'user-1',
    personId: 'person-1',
    email: 'user@example.com',
    displayName: 'Supply User',
    tenantId: 'tenant-1',
    tenantSlug: 'tenant-one',
    tenantDisplayName: 'Tenant One',
    sessionId: 'session-1',
    tenantRoleKey: 'supplyarr_admin',
    isPlatformAdmin: true,
    launchableProductKeys: ['supplyarr'],
    themePreference: 'dark',
    callbackUrl: null,
  }

  let sessionStorageMock: ReturnType<typeof createSessionStorageMock>

  beforeEach(() => {
    sessionStorageMock = createSessionStorageMock()
    vi.stubGlobal('sessionStorage', sessionStorageMock)
  })

  afterEach(() => {
    clearSession()
    vi.unstubAllGlobals()
  })

  it('round-trips sessions through sessionStorage', () => {
    const stored = toStoredSession(sampleSession)

    saveSession(stored)

    expect(loadSession()).toEqual(stored)
    expect(sessionStorageMock.setItem).toHaveBeenCalledWith(
      'stl.supplyarr.session',
      JSON.stringify(stored),
    )
  })

  it('removes the stored session when cleared', () => {
    const stored = toStoredSession(sampleSession)

    saveSession(stored)
    clearSession()

    expect(loadSession()).toBeNull()
    expect(sessionStorageMock.removeItem).toHaveBeenCalledWith('stl.supplyarr.session')
  })
})
