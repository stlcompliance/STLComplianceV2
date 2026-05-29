import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('MaintainArr defect and inspection evidence @requires-live', () => {
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

  test('defects route shows evidence panel after defect select', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    const defectsNav = page.getByRole('link', { name: 'Defects' })
    await expect(defectsNav).toBeVisible({ timeout: 15_000 })
    await defectsNav.click()
    await expect(page).toHaveURL(/\/defects/)

    const workspace = page.getByTestId('maintainarr-defects-workspace')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const evidencePanel = workspace.getByTestId('defect-evidence-panel')
    await expect(evidencePanel).toBeVisible()
    await expect(evidencePanel.getByTestId('defect-evidence-empty')).toBeVisible()

    const list = workspace.getByTestId('defect-list')
    const firstTitle = list.locator('button').first()
    if ((await firstTitle.count()) > 0) {
      await firstTitle.click()
      await expect(evidencePanel.getByTestId('defect-evidence-empty')).not.toBeVisible()
    }
  })

  test('inspections route shows run evidence panel when run is open', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    const inspectionsNav = page.getByRole('link', { name: 'Inspections' })
    await expect(inspectionsNav).toBeVisible({ timeout: 15_000 })
    await inspectionsNav.click()
    await expect(page).toHaveURL(/\/inspections/)

    const workspace = page.getByTestId('maintainarr-inspections-workspace')
    await workspace.scrollIntoViewIfNeeded()
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const openButton = workspace.getByRole('button', { name: /^Open$|^Viewing$/ }).first()
    if ((await openButton.count()) > 0) {
      await openButton.click()
      await expect(workspace.getByTestId('inspection-run-evidence-panel')).toBeVisible({
        timeout: 15_000,
      })
    }
  })
})
