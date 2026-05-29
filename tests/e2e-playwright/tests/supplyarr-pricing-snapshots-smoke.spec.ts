import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr pricing snapshots workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('supplyarr'))) {
      testInfo.skip(true, 'SupplyArr frontend (5179) is unreachable.')
    }
  })

  test('pricing route loads snapshot panels via sidebar and direct URL', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    const pricingNav = page.getByRole('link', { name: 'Pricing' })
    await expect(pricingNav).toBeVisible({ timeout: 15_000 })
    await pricingNav.click()
    await expect(page).toHaveURL(/\/pricing/)

    const workspace = page.getByTestId('supplyarr-pricing-snapshots-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const pricingPanel = workspace.getByTestId('pricing-lead-time-panel')
    await expect(pricingPanel).toBeVisible()
    await expect(pricingPanel.getByRole('heading', { name: 'Pricing & lead time' })).toBeVisible()

    await expect(pricingPanel.getByText('Loading snapshots…')).not.toBeVisible({
      timeout: 15_000,
    })

    const pricingHistoryReady =
      (await pricingPanel.getByText(/current/i).count()) > 0 ||
      (await pricingPanel.getByText(/No pricing snapshots yet/i).count()) > 0
    expect(pricingHistoryReady).toBeTruthy()

    const leadTimeHistoryReady =
      (await pricingPanel.getByText(/days/i).count()) > 0 ||
      (await pricingPanel.getByText(/No lead-time snapshots yet/i).count()) > 0
    expect(leadTimeHistoryReady).toBeTruthy()

    await pricingPanel.getByRole('checkbox', { name: 'Show current snapshots only' }).uncheck()

    const availabilityPanel = workspace.getByTestId('availability-snapshots-panel')
    await availabilityPanel.scrollIntoViewIfNeeded()
    await expect(availabilityPanel).toBeVisible()
    await expect(availabilityPanel.getByRole('heading', { name: 'Vendor availability' })).toBeVisible()

    await expect(availabilityPanel.getByText('Loading availability snapshots…')).not.toBeVisible({
      timeout: 15_000,
    })

    const availabilityHistoryReady =
      (await availabilityPanel.getByText(/current/i).count()) > 0 ||
      (await availabilityPanel.getByText(/No availability snapshots yet/i).count()) > 0
    expect(availabilityHistoryReady).toBeTruthy()

    const recordPricing = pricingPanel.getByRole('button', { name: 'Record pricing' })
    const recordLeadTime = pricingPanel.getByRole('button', { name: 'Record lead time' })
    const recordAvailability = availabilityPanel.getByRole('button', {
      name: 'Record availability',
    })

    if ((await recordPricing.count()) > 0) {
      await expect(recordPricing).toBeVisible()
      await expect(recordLeadTime).toBeVisible()
    }
    if ((await recordAvailability.count()) > 0) {
      await expect(recordAvailability).toBeVisible()
    }

    await page.goto(new URL('/pricing', page.url()).toString())
    await expect(workspace).toBeVisible({ timeout: 15_000 })
    await expect(pricingPanel.getByRole('heading', { name: 'Pricing & lead time' })).toBeVisible()
  })
})
