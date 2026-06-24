import { expect, test } from '@playwright/test'

import { isLiveModeEnabled, isLiveStackReachable, signInFromSuite } from '../support/liveProbe.js'

function collectRoles(node: any, roles = new Set<string>()): Set<string> {
  if (!node) {
    return roles
  }

  if (typeof node.role === 'string') {
    roles.add(node.role)
  }

  if (Array.isArray(node.children)) {
    for (const child of node.children) {
      collectRoles(child, roles)
    }
  }

  return roles
}

test.describe('Suite frontend accessibility smoke @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Start scripts/ops/e2e-stack-up.ps1 and e2e-frontends-preview.ps1.',
      )
    }
  })

  test('exposes landmarks and a keyboard focus target', async ({ page }) => {
    await signInFromSuite(page)
    await page.goto('/app')

    await expect(page.getByRole('navigation', { name: 'Suite navigation' })).toBeVisible()
    await expect(page.getByRole('main')).toBeVisible()

    const snapshot = await page.accessibility.snapshot({ interestingOnly: false })
    const roles = collectRoles(snapshot)

    expect(roles.has('navigation')).toBe(true)
    expect(roles.has('main')).toBe(true)
    expect(roles.has('heading')).toBe(true)

    await page.keyboard.press('Tab')
    const activeTagName = await page.evaluate(() => document.activeElement?.tagName ?? '')
    expect(activeTagName).not.toBe('BODY')
  })
})
