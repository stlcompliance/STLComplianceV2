import { test, expect } from '@playwright/test'
import {
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { handoffUrlPattern } from '../support/productFrontends.js'

test.describe('Suite frontend NexArr journey @requires-live', () => {
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

  test('login → dashboard → StaffArr launch surface', async ({ page }) => {
    await signInFromSuite(page)

    await page.goto('/app/staffarr/launch')
    await expect(
      page.getByRole('button', { name: 'Launch product (handoff)' }),
    ).toBeVisible({ timeout: 15_000 })
  })

  test('handoff issues redirect to StaffArr product app', async ({ page }) => {
    await signInFromSuite(page)

    await page.goto('/app/staffarr/launch')
    const launchButton = page.getByRole('button', {
      name: 'Launch product (handoff)',
    })
    await expect(launchButton).toBeVisible({ timeout: 15_000 })

    await Promise.all([
      page.waitForURL(handoffUrlPattern({ productKey: 'staffarr', port: 5175, baseUrl: 'http://localhost:5175' }), {
        timeout: 20_000,
      }),
      launchButton.click(),
    ])
  })
})
