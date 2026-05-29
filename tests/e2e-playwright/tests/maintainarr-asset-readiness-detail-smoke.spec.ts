import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('MaintainArr asset readiness detail @requires-live', () => {
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

  test('assets route shows readiness detail panel with blockers or signals after asset select', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    const assetsNav = page.getByRole('link', { name: 'Assets' })
    await expect(assetsNav).toBeVisible({ timeout: 15_000 })
    await assetsNav.click()
    await expect(page).toHaveURL(/\/assets/)

    const workspace = page.getByTestId('maintainarr-assets-workspace')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const detailPanel = workspace.getByTestId('asset-readiness-detail-panel')
    await expect(detailPanel).toBeVisible()
    await expect(detailPanel.getByRole('heading', { name: /asset readiness detail/i })).toBeVisible()
    await expect(detailPanel.getByTestId('asset-readiness-detail-empty')).toBeVisible()

    const registry = workspace.getByTestId('asset-registry-panel')
    await expect(registry).toBeVisible()

    const firstRow = workspace.getByTestId('asset-registry-list').locator('button').first()
    if ((await firstRow.count()) > 0) {
      await firstRow.click()
      await expect(detailPanel.getByTestId('asset-readiness-detail-loading')).not.toBeVisible({
        timeout: 15_000,
      })

      const detailReady =
        (await detailPanel.getByTestId('asset-readiness-detail-content').count()) > 0 ||
        (await detailPanel.getByTestId('asset-readiness-detail-unavailable').count()) > 0
      expect(detailReady).toBeTruthy()

      if ((await detailPanel.getByTestId('asset-readiness-detail-content').count()) > 0) {
        await expect(detailPanel.getByTestId('asset-readiness-signals')).toBeVisible()
        await expect(detailPanel.getByTestId('asset-readiness-blockers')).toBeVisible()
        await expect(detailPanel.getByTestId('asset-readiness-detail-status')).toBeVisible()
      }
    }
  })
})
