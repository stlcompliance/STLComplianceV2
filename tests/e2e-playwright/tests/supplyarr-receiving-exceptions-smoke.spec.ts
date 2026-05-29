import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr receiving exceptions @requires-live', () => {
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

  test('receiving route loads exception list and resolution controls via sidebar', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    const receivingNav = page.getByRole('link', { name: 'Receiving' })
    await expect(receivingNav).toBeVisible({ timeout: 15_000 })
    await receivingNav.click()
    await expect(page).toHaveURL(/\/receiving/)

    const workspace = page.getByTestId('supplyarr-receiving-workspace')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })
    await expect(workspace.getByRole('heading', { name: 'Receiving' })).toBeVisible()

    await expect(workspace.getByTestId('receiving-loading')).not.toBeVisible({
      timeout: 15_000,
    })

    const listReady =
      (await workspace.getByTestId('receiving-receipt-list').locator('button').count()) > 0 ||
      (await workspace.getByText(/No receiving receipts yet/i).count()) > 0
    expect(listReady).toBeTruthy()

    const firstRow = workspace.getByTestId('receiving-receipt-list').locator('button').first()
    if ((await firstRow.count()) > 0) {
      await firstRow.click()
      await expect(workspace.getByTestId('receiving-receipt-detail')).toBeVisible()
      await expect(workspace.getByTestId('receiving-exception-panel')).toBeVisible()
      await expect(workspace.getByTestId('receiving-exception-filter')).toBeVisible()

      const exceptionList = workspace.getByTestId('receiving-exception-list')
      const emptyState = workspace.getByTestId('receiving-exception-empty')
      const hasExceptions = (await exceptionList.locator('li').count()) > 0
      const hasEmpty = (await emptyState.count()) > 0
      expect(hasExceptions || hasEmpty).toBeTruthy()

      if (hasExceptions) {
        const resolveButton = exceptionList
          .locator('[data-testid^="receiving-exception-resolve-button-"]')
          .first()
        if ((await resolveButton.count()) > 0) {
          await expect(resolveButton).toBeVisible()
        }
        await expect(exceptionList.getByTestId('receiving-exception-workflow-timeline').first()).toBeVisible()
      }

      const recordForm = workspace.getByTestId('receiving-exception-record-form')
      if ((await recordForm.count()) > 0) {
        await expect(recordForm.getByTestId('receiving-exception-type-select')).toBeVisible()
        await expect(recordForm.getByTestId('receiving-exception-quantity-input')).toBeVisible()
        await expect(recordForm.getByTestId('receiving-exception-record-button')).toBeDisabled()
        await recordForm.getByTestId('receiving-exception-quantity-input').fill('1')
        await expect(recordForm.getByTestId('receiving-exception-record-button')).toBeEnabled()
      }
    }

    await expect(workspace.getByTestId('receiving-create-form')).toBeVisible()

    await page.goto(new URL('/receiving', page.url()).toString())
    await expect(workspace).toBeVisible({ timeout: 15_000 })
    await expect(workspace.getByRole('heading', { name: 'Receiving' })).toBeVisible()
  })
})
