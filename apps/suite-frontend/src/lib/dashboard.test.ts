import { describe, expect, it } from 'vitest'
import type { MeResponse, NavigationItem, TenantSummary } from '../api/types'
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
  requiresPasswordChange: false,
  tenantId: 'tenant-1',
  tenantSlug: 'demo-stl',
  tenantDisplayName: 'Demo STL',
  entitlements: [],
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
  it('marks in-suite vs external and uses canonical catalog metadata', () => {
    const nav: NavigationItem[] = [
      {
        productKey: 'nexarr',
        displayName: 'NexArr',
        routePath: '/app/nexarr',
        sortOrder: 0,
        surfaces: [],
      },
      {
        productKey: 'staffarr',
        displayName: 'StaffArr',
        routePath: '/app/staffarr',
        sortOrder: 1,
        surfaces: [
          {
            surfaceKey: 'launch',
            label: 'Open StaffArr app',
            relativePath: 'launch',
            iconKey: 'staffarr',
            sortOrder: 90,
            isEnabled: true,
            permissionHint: null,
          },
        ],
      },
    ]
    const products = buildQuickLaunchProducts(nav)
    expect(products).toHaveLength(2)
    expect(products[0]).toMatchObject({ productKey: 'nexarr', inSuite: true, launchable: false })
    expect(products[1]).toMatchObject({
      productKey: 'staffarr',
      inSuite: false,
      launchable: true,
    })
  })

  it('marks recognized products without an enabled launch surface as not launchable', () => {
    const products = buildQuickLaunchProducts(
      [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr worker facade',
          routePath: '/app/staffarr',
          sortOrder: 0,
          surfaces: [
            {
              surfaceKey: 'overview',
              label: 'Overview',
              relativePath: '',
              iconKey: 'dashboard',
              sortOrder: 0,
              isEnabled: true,
              permissionHint: null,
            },
          ],
        },
      ],
    )

    expect(products[0]).toMatchObject({
      productKey: 'staffarr',
      displayName: 'StaffArr',
      launchable: false,
    })
  })

  it('filters unsupported navigation products out of quick launch', () => {
    const products = buildQuickLaunchProducts(
      [
        {
          productKey: 'shared-worker',
          displayName: 'STL Shared Worker',
          routePath: '/app/shared-worker',
          sortOrder: 0,
          surfaces: [],
        },
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          routePath: '/app/staffarr',
          sortOrder: 1,
          surfaces: [],
        },
      ],
    )

    expect(products.map((product) => product.productKey)).toEqual(['staffarr'])
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

  const navigation: NavigationItem[] = [
    {
      productKey: 'nexarr',
      displayName: 'NexArr',
      routePath: '/app/nexarr',
      sortOrder: 0,
      surfaces: [],
    },
    {
      productKey: 'staffarr',
      displayName: 'StaffArr',
      routePath: '/app/staffarr',
      sortOrder: 1,
      surfaces: [
        {
          surfaceKey: 'launch',
          label: 'Open StaffArr app',
          relativePath: 'launch',
          iconKey: 'staffarr',
          sortOrder: 90,
          isEnabled: true,
          permissionHint: null,
        },
      ],
    },
  ]

  it('includes launch actions and platform admin link', () => {
    const actions = buildWhatINeedActions({
      me,
      tenants,
      navigationProducts: navigation,
    })
    expect(actions.some((a) => a.id === 'hub-nexarr')).toBe(true)
    expect(actions.some((a) => a.id === 'launch-staffarr')).toBe(true)
    expect(actions.some((a) => a.id === 'platform-admin')).toBe(true)
  })

  it('avoids launch CTA metadata for non-launchable external products', () => {
    const actions = buildWhatINeedActions({
      me,
      tenants,
      navigationProducts: [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          routePath: '/app/staffarr',
          sortOrder: 1,
          surfaces: [
            {
              surfaceKey: 'overview',
              label: 'Overview',
              relativePath: '',
              iconKey: 'dashboard',
              sortOrder: 0,
              isEnabled: true,
              permissionHint: null,
            },
          ],
        },
      ],
    })

    expect(actions).toContainEqual(
      expect.objectContaining({
        id: 'open-staffarr',
        href: '/app/staffarr',
        productKey: undefined,
      }),
    )
  })

  it('warns when tenant is suspended', () => {
    const actions = buildWhatINeedActions({
      me,
      tenants: [{ ...tenants[0], status: 'Suspended' }],
      navigationProducts: navigation,
    })
    expect(actions.some((a) => a.id === 'tenant-not-active')).toBe(true)
  })
})
