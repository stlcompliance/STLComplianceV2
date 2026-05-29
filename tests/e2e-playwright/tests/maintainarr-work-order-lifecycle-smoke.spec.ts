import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('MaintainArr work order lifecycle @requires-live', () => {
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
      testInfo.skip(true, 'MaintainArr frontend (5177) is unreachable.')
    }
  })

  test('work orders route shows lifecycle panel with signals after work order select', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    const workOrdersNav = page.getByRole('link', { name: 'Work orders' })
    await expect(workOrdersNav).toBeVisible({ timeout: 15_000 })
    await workOrdersNav.click()
    await expect(page).toHaveURL(/\/work-orders/)

    const workspace = page.getByTestId('maintainarr-work-orders-workspace')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const lifecyclePanel = workspace.getByTestId('work-order-lifecycle-panel')
    await expect(lifecyclePanel).toBeVisible()
    await expect(lifecyclePanel.getByRole('heading', { name: /work order lifecycle/i })).toBeVisible()
    await expect(lifecyclePanel.getByTestId('work-order-lifecycle-empty')).toBeVisible()

    const list = workspace.getByTestId('work-order-list')
    await expect(list).toBeVisible()

    const firstSelect = list.getByRole('button', { name: /view/i }).first()
    if ((await firstSelect.count()) > 0) {
      await firstSelect.click()
      await expect(lifecyclePanel.getByTestId('work-order-lifecycle-empty')).not.toBeVisible({
        timeout: 15_000,
      })

      const lifecycleReady =
        (await lifecyclePanel.getByTestId('work-order-lifecycle-content').count()) > 0 ||
        (await lifecyclePanel.getByTestId('work-order-lifecycle-loading').count()) > 0
      expect(lifecycleReady).toBeTruthy()

      if ((await lifecyclePanel.getByTestId('work-order-lifecycle-content').count()) > 0) {
        await expect(lifecyclePanel.getByTestId('work-order-lifecycle-stepper')).toBeVisible()
        await expect(lifecyclePanel.getByTestId('work-order-completion-signals')).toBeVisible()
        await expect(workspace.getByTestId('work-order-labor-evidence-panel')).toBeVisible()
      }
    }
  })
})
