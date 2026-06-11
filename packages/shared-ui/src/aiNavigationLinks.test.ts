import { describe, expect, it } from 'vitest'
import { buildAiNavigationLinks } from './aiNavigationLinks'

describe('buildAiNavigationLinks', () => {
  it('includes entitled cross-product route hints such as StaffArr roles', () => {
    const links = buildAiNavigationLinks({
      currentProductKey: 'nexarr',
      entitlements: ['nexarr', 'staffarr'],
      suiteHomeUrl: 'https://app.stlcompliance.com/app',
      productLaunchUrls: {
        staffarr: 'https://app.stlcompliance.com/staffarr/launch',
      },
    })

    expect(links).toContainEqual(
      expect.objectContaining({
        label: 'StaffArr roles',
        productKey: 'staffarr',
        route: '/roles',
        href: 'https://app.stlcompliance.com/staffarr/roles',
      }),
    )
  })

  it('does not include cross-product hints without entitlement', () => {
    const links = buildAiNavigationLinks({
      currentProductKey: 'nexarr',
      entitlements: ['nexarr'],
      suiteHomeUrl: 'https://app.stlcompliance.com/app',
      productLaunchUrls: {
        staffarr: 'https://app.stlcompliance.com/staffarr/launch',
      },
    })

    expect(links).not.toContainEqual(
      expect.objectContaining({
        label: 'StaffArr roles',
      }),
    )
  })

  it('adds current product navigation items as direct page links', () => {
    const links = buildAiNavigationLinks({
      currentProductKey: 'staffarr',
      entitlements: ['staffarr'],
      suiteHomeUrl: 'https://app.stlcompliance.com/app',
      productLaunchUrls: {
        staffarr: 'https://app.stlcompliance.com/staffarr/launch',
      },
      currentNavItems: [{ label: 'Readiness', to: '/readiness' }],
    })

    expect(links).toContainEqual(
      expect.objectContaining({
        label: 'StaffArr Readiness',
        productKey: 'staffarr',
        route: '/readiness',
        href: 'https://app.stlcompliance.com/staffarr/readiness',
      }),
    )
  })
})
