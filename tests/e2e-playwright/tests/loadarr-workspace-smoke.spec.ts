import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('LoadArr workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('loadarr'))) {
      testInfo.skip(true, 'LoadArr frontend (5182) is unreachable.')
    }
  })

  test('handoff opens the warehouse workspace with core execution routes', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'loadarr')

    await page.goto(new URL('/inventory', page.url()).toString())

    await expect(page.getByRole('heading', { name: 'Warehouse execution' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Warehouse metrics')).toBeVisible()
    await expect(page.getByText('Active locations')).toBeVisible()

    await page.goto(new URL('/receiving', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Manual receiving' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Guided receiving workflow')).toBeVisible()
    await expect(page.getByLabel('Receiving type')).toBeVisible()

    await page.goto(new URL('/unexplained', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Unexplained inventory' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Unexplained inventory workflow')).toBeVisible()

    await page.goto(new URL('/handoffs', page.url()).toString())
    await expect(page.getByLabel('Route and product handoffs')).toBeVisible()
  })
})
