import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr party registry maintenance @requires-live', () => {
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

  test('parties route loads registry panels with lifecycle controls via sidebar', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    const partiesNav = page.getByRole('link', { name: 'Parties' })
    await expect(partiesNav).toBeVisible({ timeout: 15_000 })
    await partiesNav.click()
    await expect(page).toHaveURL(/\/parties/)

    const workspace = page.getByTestId('supplyarr-party-registry-workspace')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const vendorPanel = workspace.getByTestId('party-registry-panel-vendors')
    await expect(vendorPanel).toBeVisible()
    await expect(vendorPanel.getByRole('heading', { name: 'Vendors' })).toBeVisible()
    await expect(workspace.getByTestId('party-registry-panel-suppliers')).toBeVisible()
    await expect(workspace.getByTestId('party-registry-panel-dealers')).toBeVisible()

    await expect(vendorPanel.getByTestId('party-registry-loading')).not.toBeVisible({
      timeout: 15_000,
    })

    const listReady =
      (await vendorPanel.getByTestId('party-registry-list').locator('button').count()) > 0 ||
      (await vendorPanel.getByText(/No records yet/i).count()) > 0
    expect(listReady).toBeTruthy()

    const firstRow = vendorPanel.getByTestId('party-registry-list').locator('button').first()
    if ((await firstRow.count()) > 0) {
      await firstRow.click()
      await expect(vendorPanel.getByTestId('party-registry-detail')).toBeVisible()
      await expect(vendorPanel.getByTestId('party-registry-lifecycle-timeline')).toBeVisible()
      await expect(vendorPanel.getByTestId('party-registry-edit-form')).toBeVisible()
      await expect(vendorPanel.getByTestId('party-registry-contact-form')).toBeVisible()
    }

    const createForm = vendorPanel.getByTestId('party-registry-create-form')
    if ((await createForm.count()) > 0) {
      await expect(createForm.getByTestId('party-registry-create-button')).toBeDisabled()
    }

    await page.goto(new URL('/parties', page.url()).toString())
    await expect(workspace).toBeVisible({ timeout: 15_000 })
    await expect(vendorPanel).toBeVisible()
  })
})
