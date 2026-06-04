import { test, expect } from '@playwright/test'
import { ensureTrainArrFieldInboxFixture } from '../support/e2eApi.js'
import {
  isCompanionFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { companionFrontend, handoffUrlPattern } from '../support/productFrontends.js'

test.describe('Field Companion field submission state @requires-live', () => {
  test.beforeAll(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(true, 'Suite and NexArr API must be reachable for live E2E.')
    }
    if (!(await isCompanionFrontendReachable())) {
      testInfo.skip(true, 'Field Companion preview (5181) must be running.')
    }

    await ensureTrainArrFieldInboxFixture()
  })

  test('shows submission toast and status chip after acknowledge sync', async ({ page }, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }

    await signInFromSuite(page)
    await page.goto('/app/field-companion/launch')

    const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
    await expect(launchButton).toBeVisible({ timeout: 15_000 })
    await Promise.all([
      page.waitForURL(handoffUrlPattern(companionFrontend), { timeout: 30_000 }),
      launchButton.click(),
    ])

    const acknowledgeButton = page.getByTestId('companion-acknowledge-task').first()
    await expect(acknowledgeButton).toBeVisible({ timeout: 15_000 })
    await acknowledgeButton.click()

    await expect(page.getByTestId('companion-submission-toast')).toBeVisible({ timeout: 15_000 })
    await expect(page.getByTestId('companion-task-submission-status').first()).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByTestId('companion-submission-chip-acknowledge').first()).toContainText(
      /submitted|synced|syncing/i,
      { timeout: 20_000 },
    )
  })
})
