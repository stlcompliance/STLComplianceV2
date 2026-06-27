import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { HandoffSessionResponse } from '../api/types'
import {
  canAssignDrivers,
  canCreateTrips,
  canExportDispatchReports,
  canManageDriverAvailability,
  canManageNotificationSettings,
  canManageTrips,
  canPerformTrips,
  canReadDispatchReports,
  canReadTripVisibility,
  canViewAllTrips,
  clearSession,
  loadSession,
  saveSession,
  toStoredSession,
} from './sessionStorage'

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

describe('routarr sessionStorage', () => {
  const sampleSession: HandoffSessionResponse = {
    accessToken: 'access-token',
    accessTokenExpiresAt: new Date(Date.now() + 60_000).toISOString(),
    userId: 'user-1',
    personId: 'person-1',
    email: 'user@example.com',
    displayName: 'RoutArr User',
    tenantId: 'tenant-1',
    tenantSlug: 'tenant-one',
    tenantDisplayName: 'Tenant One',
    sessionId: 'session-1',
    tenantRoleKey: 'routarr_admin',
    isPlatformAdmin: true,
    launchableProductKeys: ['routarr'],
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
      'stl.routarr.session',
      JSON.stringify(stored),
    )
  })

  it('removes the stored session when cleared', () => {
    const stored = toStoredSession(sampleSession)

    saveSession(stored)
    clearSession()

    expect(loadSession()).toBeNull()
    expect(sessionStorageMock.removeItem).toHaveBeenCalledWith('stl.routarr.session')
  })

  it('grants platform admins full RoutArr workflow access', () => {
    expect(canManageNotificationSettings('tenant_member', true)).toBe(true)
    expect(canCreateTrips('tenant_member', true)).toBe(true)
    expect(canAssignDrivers('tenant_member', true)).toBe(true)
    expect(canManageTrips('tenant_member', true)).toBe(true)
    expect(canPerformTrips('tenant_member', true)).toBe(true)
    expect(canViewAllTrips('tenant_member', true)).toBe(true)
    expect(canManageDriverAvailability('tenant_member', true)).toBe(true)
    expect(canReadTripVisibility('tenant_member', true)).toBe(true)
    expect(canReadDispatchReports('tenant_member', true)).toBe(true)
    expect(canExportDispatchReports('tenant_member', true)).toBe(true)
  })

  it('keeps dispatcher access narrower than manager access', () => {
    expect(canCreateTrips('routarr_dispatcher', false)).toBe(true)
    expect(canAssignDrivers('routarr_dispatcher', false)).toBe(true)
    expect(canPerformTrips('routarr_dispatcher', false)).toBe(true)
    expect(canViewAllTrips('routarr_dispatcher', false)).toBe(true)
    expect(canManageTrips('routarr_dispatcher', false)).toBe(false)
    expect(canManageNotificationSettings('routarr_dispatcher', false)).toBe(false)
    expect(canManageDriverAvailability('routarr_dispatcher', false)).toBe(true)
    expect(canReadTripVisibility('routarr_dispatcher', false)).toBe(true)
    expect(canReadDispatchReports('routarr_dispatcher', false)).toBe(true)
    expect(canExportDispatchReports('routarr_dispatcher', false)).toBe(false)
  })
})
