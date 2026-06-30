import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr supplier directory maintenance @requires-live', () => {
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

  test('suppliers route loads the supplier directory workspace via sidebar', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    const suppliersNav = page.getByRole('link', { name: 'Suppliers' })
    await expect(suppliersNav).toBeVisible({ timeout: 15_000 })
    await suppliersNav.click()
    await expect(page).toHaveURL(/\/suppliers/)

    const workspace = page.getByTestId('supplyarr-supplier-directory')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    await expect(workspace.getByRole('heading', { name: 'Supplier directory' })).toBeVisible()
    await expect(workspace.getByText(/supplier identities/i)).toBeVisible()
    await expect(workspace.getByText(/sub-units/i)).toBeVisible()
    await expect(workspace.getByText(/Add supplier or sub-unit|Create supplier identity or sub-unit/i)).toBeVisible()

    const listReady =
      (await workspace.locator('button').count()) > 0 ||
      (await workspace.getByText(/No suppliers yet/i).count()) > 0
    expect(listReady).toBeTruthy()

    const firstRow = workspace.locator('button').first()
    if ((await firstRow.count()) > 0) {
      await firstRow.click()
      await expect(page.getByTestId('supplyarr-supplier-profile')).toBeVisible()
      await expect(page.getByText('Supplier snapshot')).toBeVisible()
      await expect(page.getByText('Sub-units')).toBeVisible()
    }

    await page.goto(new URL('/suppliers', page.url()).toString())
    await expect(workspace).toBeVisible({ timeout: 15_000 })
    await expect(workspace.getByRole('heading', { name: 'Supplier directory' })).toBeVisible()
  })
})
