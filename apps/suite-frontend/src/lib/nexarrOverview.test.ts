import { describe, expect, it } from 'vitest'

import type { NavigationSurfaceItem, UserSessionSummary } from '../api/types'
import { countActiveSessions, listEnabledSurfaces } from './nexarrOverview'

describe('countActiveSessions', () => {
  it('counts only active sessions', () => {
    const sessions: UserSessionSummary[] = [
      {
        sessionId: 's1',
        createdAt: '2026-01-01T00:00:00Z',
        expiresAt: '2026-02-01T00:00:00Z',
        revokedAt: null,
        userAgent: null,
        ipAddress: null,
        activeTenantId: 'tenant-1',
        isCurrent: true,
        isActive: true,
        isRemembered: false,
      },
      {
        sessionId: 's2',
        createdAt: '2026-01-01T00:00:00Z',
        expiresAt: '2026-01-02T00:00:00Z',
        revokedAt: '2026-01-02T00:00:00Z',
        userAgent: null,
        ipAddress: null,
        activeTenantId: 'tenant-1',
        isCurrent: false,
        isActive: false,
        isRemembered: false,
      },
    ]

    expect(countActiveSessions(sessions)).toBe(1)
  })
})

describe('listEnabledSurfaces', () => {
  it('returns enabled surfaces sorted by sort order', () => {
    const surfaces: NavigationSurfaceItem[] = [
      {
        surfaceKey: 'identity',
        label: 'Identity & sessions',
        relativePath: 'identity',
        iconKey: 'auth',
        sortOrder: 10,
        isEnabled: true,
        permissionHint: null,
      },
      {
        surfaceKey: 'overview',
        label: 'Overview',
        relativePath: '',
        iconKey: 'dashboard',
        sortOrder: 0,
        isEnabled: true,
        permissionHint: null,
      },
      {
        surfaceKey: 'tenants',
        label: 'Tenants',
        relativePath: 'tenants',
        iconKey: 'sites',
        sortOrder: 20,
        isEnabled: false,
        permissionHint: 'Requires platform administrator access.',
      },
    ]

    expect(listEnabledSurfaces(surfaces).map((surface) => surface.surfaceKey)).toEqual([
      'overview',
      'identity',
    ])
  })
})
