import { describe, expect, it } from 'vitest'
import type {
  EntitlementSummary,
  MeResponse,
  NavigationItem,
  TenantSummary,
} from '../api/types'
import type { StoredAuthSession } from '../auth/authStorage'
import {
  buildQuickLaunchProducts,
  buildWhatINeedActions,
  findCurrentTenant,
  isTenantActive,
  summarizeSession,
} from './dashboard'

const baseSession: StoredAuthSession = {
  accessToken: 'a',
  refreshToken: 'r',
  accessTokenExpiresAt: new Date(Date.now() + 60 * 60_000).toISOString(),
  refreshTokenExpiresAt: new Date(Date.now() + 7 * 24 * 60 * 60_000).toISOString(),
  sessionId: 'sess-1',
  userId: 'user-1',
  tenantId: 'tenant-1',
}

const me: MeResponse = {
  userId: 'user-1',
  email: 'admin@demo.stl',
  displayName: 'Demo Admin',
  isPlatformAdmin: true,
  tenantId: 'tenant-1',
  tenantSlug: 'demo-stl',
  tenantDisplayName: 'Demo STL',
  entitlements: ['nexarr', 'staffarr'],
}

describe('findCurrentTenant', () => {
  it('returns the tenant matching active context', () => {
    const tenants: TenantSummary[] = [
      {
        tenantId: 'tenant-1',
        slug: 'demo-stl',
        displayName: 'Demo STL',
        status: 'Active',
        roleKey: 'tenant_admin',
      },
      {
        tenantId: 'tenant-2',
        slug: 'other',
        displayName: 'Other',
        status: 'Active',
        roleKey: 'member',
      },
    ]
    expect(findCurrentTenant(tenants, 'tenant-1')?.slug).toBe('demo-stl')
    expect(findCurrentTenant(tenants, 'missing')).toBeUndefined()
  })
})

describe('isTenantActive', () => {
  it('is case-insensitive on status', () => {
    expect(
      isTenantActive({
        tenantId: 't',
        slug: 's',
        displayName: 'D',
        status: 'ACTIVE',
        roleKey: 'member',
      }),
    ).toBe(true)
    expect(
      isTenantActive({
        tenantId: 't',
        slug: 's',
        displayName: 'D',
        status: 'Suspended',
        roleKey: 'member',
      }),
    ).toBe(false)
  })
})

describe('buildQuickLaunchProducts', () => {
  it('marks in-suite vs external and entitlement', () => {
    const nav: NavigationItem[] = [
      {
        productKey: 'nexarr',
        displayName: 'NexArr',
        routePath: '/app/nexarr',
        sortOrder: 0,
      },
      {
        productKey: 'staffarr',
        displayName: 'StaffArr',
        routePath: '/app/staffarr',
        sortOrder: 1,
      },
    ]
    const products = buildQuickLaunchProducts(nav, ['nexarr', 'staffarr'])
    expect(products).toHaveLength(2)
    expect(products[0]).toMatchObject({ productKey: 'nexarr', inSuite: true, entitled: true })
    expect(products[1]).toMatchObject({ productKey: 'staffarr', inSuite: false, entitled: true })
  })
})

describe('summarizeSession', () => {
  it('flags access token expiring within 15 minutes', () => {
    const soon: StoredAuthSession = {
      ...baseSession,
      accessTokenExpiresAt: new Date(Date.now() + 5 * 60_000).toISOString(),
    }
    const summary = summarizeSession(soon)
    expect(summary.isAccessExpiringSoon).toBe(true)
    expect(summary.accessExpiresInMinutes).toBeLessThanOrEqual(5)
  })

  it('does not flag distant expiry', () => {
    const summary = summarizeSession(baseSession)
    expect(summary.isAccessExpiringSoon).toBe(false)
  })
})

describe('buildWhatINeedActions', () => {
  const tenants: TenantSummary[] = [
    {
      tenantId: 'tenant-1',
      slug: 'demo-stl',
      displayName: 'Demo STL',
      status: 'Active',
      roleKey: 'tenant_admin',
    },
  ]

  const entitlements: EntitlementSummary[] = [
    { productKey: 'nexarr', displayName: 'NexArr', status: 'Active' },
    { productKey: 'staffarr', displayName: 'StaffArr', status: 'Active' },
  ]

  const navigation: NavigationItem[] = [
    {
      productKey: 'nexarr',
      displayName: 'NexArr',
      routePath: '/app/nexarr',
      sortOrder: 0,
    },
    {
      productKey: 'staffarr',
      displayName: 'StaffArr',
      routePath: '/app/staffarr',
      sortOrder: 1,
    },
  ]

  it('includes launch actions and platform admin link', () => {
    const actions = buildWhatINeedActions({
      me,
      tenants,
      entitlements,
      navigationProducts: navigation,
    })
    expect(actions.some((a) => a.id === 'hub-nexarr')).toBe(true)
    expect(actions.some((a) => a.id === 'launch-staffarr')).toBe(true)
    expect(actions.some((a) => a.id === 'platform-admin')).toBe(true)
  })

  it('warns when tenant is suspended', () => {
    const actions = buildWhatINeedActions({
      me,
      tenants: [{ ...tenants[0], status: 'Suspended' }],
      entitlements,
      navigationProducts: navigation,
    })
    expect(actions.some((a) => a.id === 'tenant-not-active')).toBe(true)
  })

  it('warns non-admins with no entitlements', () => {
    const actions = buildWhatINeedActions({
      me: { ...me, isPlatformAdmin: false, entitlements: [] },
      tenants,
      entitlements: [],
      navigationProducts: [],
    })
    expect(actions.some((a) => a.id === 'no-entitlements')).toBe(true)
    expect(actions.some((a) => a.id === 'platform-admin')).toBe(false)
  })
})
