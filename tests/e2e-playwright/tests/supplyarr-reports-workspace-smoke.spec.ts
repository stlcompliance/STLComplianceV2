import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr reports workspace @requires-live', () => {
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

  test('reports vendor and purchasing panels filter and export controls', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    await page.goto(new URL('/reports', page.url()).toString())

    const vendorPanel = page.getByTestId('vendor-reports-panel')
    await expect(vendorPanel).toBeVisible({ timeout: 15_000 })
    await expect(vendorPanel.getByRole('heading', { name: 'Vendor reports' })).toBeVisible()

    const vendorExport = vendorPanel.getByRole('button', { name: 'Export CSV' })
    await expect(vendorExport).toBeVisible()
    await expect(vendorExport).toBeEnabled()

    await vendorPanel.locator('select').selectOption('approved')
    await vendorPanel.getByRole('checkbox').check()

    await expect(vendorPanel.getByText('Loading vendor report summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const vendorHasData =
      (await vendorPanel.locator('span.rounded-md').count()) > 0 ||
      (await vendorPanel.getByText(/No vendors match/i).count()) > 0
    expect(vendorHasData).toBeTruthy()

    const purchasingPanel = page.getByTestId('purchasing-reports-panel')
    await purchasingPanel.scrollIntoViewIfNeeded()
    await expect(purchasingPanel).toBeVisible({ timeout: 15_000 })
    await expect(purchasingPanel.getByRole('heading', { name: 'Purchasing reports' })).toBeVisible()

    const purchasingExport = purchasingPanel.getByRole('button', { name: 'Export CSV' })
    await expect(purchasingExport).toBeVisible()
    await expect(purchasingExport).toBeEnabled()

    await purchasingPanel.getByRole('checkbox').check()

    await expect(purchasingPanel.getByText('Loading purchasing summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const purchasingHasData =
      (await purchasingPanel.getByText(/^PRs:/i).count()) > 0 ||
      (await purchasingPanel.getByText(/No documents match/i).count()) > 0
    expect(purchasingHasData).toBeTruthy()
  })
})
