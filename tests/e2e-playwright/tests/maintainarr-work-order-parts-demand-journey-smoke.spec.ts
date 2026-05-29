import { test, expect } from '@playwright/test'

import { ensureMaintainArrPartsDemandJourneyFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('MaintainArr work order parts demand → SupplyArr journey @requires-live', () => {
  let hasJourneyFixture = false
  let fixtureWorkOrderNumber = ''
  let fixturePartNumber = ''

  test.beforeAll(async () => {
    if (!isLiveModeEnabled()) {
      return
    }
    if (!(await isLiveStackReachable())) {
      return
    }
    try {
      const fixture = await ensureMaintainArrPartsDemandJourneyFixture()
      hasJourneyFixture = Boolean(fixture.workOrderId)
      fixtureWorkOrderNumber = fixture.workOrderNumber
      fixturePartNumber = fixture.partNumber
    } catch {
      hasJourneyFixture = false
    }
  })

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
    if (!(await isHandoffFrontendReachable('maintainarr'))) {
      testInfo.skip(true, 'MaintainArr frontend (5178) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('supplyarr'))) {
      testInfo.skip(true, 'SupplyArr frontend (5179) is unreachable.')
    }
  })

  test('publish parts demand from work orders shows SupplyArr ref in UI', async ({ page }) => {
    test.skip(!hasJourneyFixture, 'Parts demand journey fixture could not be seeded.')

    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    await page.goto(new URL('/work-orders', page.url()).toString())

    const row = page.getByRole('row', { name: new RegExp(fixtureWorkOrderNumber) })
    await expect(row).toBeVisible({ timeout: 15_000 })
    await row.click()

    const panel = page.getByTestId('work-order-parts-demand-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel).toContainText(fixturePartNumber)
    await expect(panel).toContainText('pending')

    await panel.getByTestId('work-order-parts-demand-publish').click()

    await expect(panel).toContainText('published', { timeout: 20_000 })
    await expect(panel).toContainText('SupplyArr ref', { timeout: 20_000 })
  })
})
