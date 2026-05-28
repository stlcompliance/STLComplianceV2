import { test, expect } from '@playwright/test'

import {
  demoTenant,
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { handoffProductFrontends, handoffUrlPattern } from '../support/productFrontends.js'

test.describe('Product shell tenant chrome after handoff @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Use scripts/ops/e2e-stack-up.ps1 and e2e-frontends-preview.ps1.',
      )
    }
  })

  for (const product of handoffProductFrontends) {
    test(`${product.productKey} handoff shows tenant name and slug in workspace shell`, async ({
      page,
    }, testInfo) => {
      if (!(await isHandoffFrontendReachable(product.productKey))) {
        testInfo.skip(
          true,
          `${product.productKey} frontend (${product.baseUrl}) is unreachable.`,
        )
      }

      await signInFromSuite(page)
      await page.goto(`/app/${product.productKey}/launch`)

      const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
      await expect(launchButton).toBeVisible({ timeout: 15_000 })

      await Promise.all([
        page.waitForURL(handoffUrlPattern(product), { timeout: 30_000 }),
        launchButton.click(),
      ])

      await expect(page).not.toHaveURL(/handoff=/, { timeout: 25_000 })

      const tenantName = page.getByTestId('workspace-tenant-display-name')
      const tenantSlug = page.getByTestId('workspace-tenant-slug')
      await expect(tenantName).toBeVisible({ timeout: 15_000 })
      await expect(tenantSlug).toBeVisible({ timeout: 15_000 })
      await expect(tenantName).toHaveText(demoTenant.displayName)
      await expect(tenantSlug).toHaveText(demoTenant.slug)
      await expect(page.getByTestId('workspace-user-display-name')).toContainText(
        demoTenant.userDisplayName,
      )
    })
  }
})
