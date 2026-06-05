import { test, expect } from '@playwright/test'
import { ensureTrainArrFieldInboxFixture } from '../support/e2eApi.js'
import {
  isFieldCompanionFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { FieldCompanionFrontend, handoffUrlPattern } from '../support/productFrontends.js'

test.describe('Field Companion field scan resolve @requires-live', () => {
  test.beforeAll(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(true, 'Suite and NexArr API must be reachable for live E2E.')
    }
    if (!(await isFieldCompanionFrontendReachable())) {
      testInfo.skip(true, 'Field Companion preview (5181) must be running.')
    }

    await ensureTrainArrFieldInboxFixture()
  })

  test('resolves manual scan code to inbox task', async ({ page }, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }

    await signInFromSuite(page)
    await page.goto('/app/field-companion/launch')

    const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
    await expect(launchButton).toBeVisible({ timeout: 15_000 })
    await Promise.all([
      page.waitForURL(handoffUrlPattern(FieldCompanionFrontend), { timeout: 30_000 }),
      launchButton.click(),
    ])

    const scanPanel = page.getByTestId('fieldcompanion-field-scan-panel')
    await expect(scanPanel).toBeVisible({ timeout: 15_000 })

    const firstTask = page.getByTestId('fieldcompanion-field-inbox-task').first()
    await expect(firstTask).toBeVisible({ timeout: 15_000 })
    const taskKey = await firstTask.getAttribute('data-task-key')
    test.skip(!taskKey, 'Field inbox must expose a task key for scan resolve.')

    await page.getByTestId('fieldcompanion-scan-manual-input').fill(taskKey!)
    await page.getByTestId('fieldcompanion-scan-submit').click()

    await expect(page.getByTestId('fieldcompanion-scan-result')).toBeVisible({ timeout: 15_000 })
    await expect(firstTask).toHaveAttribute('data-task-key', taskKey!)
  })
})
