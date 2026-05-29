import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr purchasing PO workflow @requires-live', () => {
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

  test('purchasing route loads PO panel with line detail and cancel controls via sidebar', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    const purchasingNav = page.getByRole('link', { name: 'Purchasing' })
    await expect(purchasingNav).toBeVisible({ timeout: 15_000 })
    await purchasingNav.click()
    await expect(page).toHaveURL(/\/purchasing/)

    const workspace = page.getByTestId('supplyarr-purchasing-po-workspace')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })
    await expect(workspace.getByRole('heading', { name: 'Purchase orders' })).toBeVisible()

    await expect(workspace.getByTestId('purchase-order-loading')).not.toBeVisible({
      timeout: 15_000,
    })

    const listReady =
      (await workspace.getByTestId('purchase-order-list').locator('button').count()) > 0 ||
      (await workspace.getByText(/No purchase orders yet/i).count()) > 0
    expect(listReady).toBeTruthy()

    const firstRow = workspace.getByTestId('purchase-order-list').locator('button').first()
    if ((await firstRow.count()) > 0) {
      await firstRow.click()
      await expect(workspace.getByTestId('purchase-order-detail')).toBeVisible()
      const lineList = workspace.getByTestId('purchase-order-line-list')
      const hasLines = (await lineList.locator('li').count()) > 0
      if (hasLines) {
        await expect(lineList.locator('li').first()).toContainText(/ordered/)
      }
      const cancelButton = workspace.getByTestId('purchase-order-cancel-button')
      if ((await cancelButton.count()) > 0) {
        await expect(workspace.getByTestId('purchase-order-cancellation-reason-input')).toBeVisible()
        await expect(cancelButton).toBeDisabled()
        await workspace.getByTestId('purchase-order-cancellation-reason-input').fill('E2E smoke check')
        await expect(cancelButton).toBeEnabled()
      }
    }

    await expect(workspace.getByTestId('purchase-order-create-form')).toBeVisible()

    await page.goto(new URL('/purchasing', page.url()).toString())
    await expect(workspace).toBeVisible({ timeout: 15_000 })
    await expect(workspace.getByRole('heading', { name: 'Purchase orders' })).toBeVisible()
  })
})
