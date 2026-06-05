import { test, expect } from '@playwright/test'
import { ensureTrainArrFieldInboxFixture } from '../support/e2eApi.js'
import {
  isFieldCompanionFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { FieldCompanionFrontend, handoffUrlPattern } from '../support/productFrontends.js'

test.describe('Field Companion field task evidence capture @requires-live', () => {
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

  test('uploads photo evidence for TrainArr assignment via Field Companion API', async ({ page }, testInfo) => {
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

    await expect(page.getByRole('heading', { name: 'Field inbox' })).toBeVisible({ timeout: 20_000 })

    const trainarrFilter = page.getByRole('button', { name: /TrainArr/i })
    if (await trainarrFilter.isVisible()) {
      await trainarrFilter.click()
    }

    const evidencePanel = page.getByTestId('fieldcompanion-field-evidence-panel').first()
    await expect(evidencePanel).toBeVisible({ timeout: 15_000 })

    await page.getByTestId('fieldcompanion-evidence-kind-photo').first().click()
    await page.getByTestId('fieldcompanion-evidence-file-input').first().setInputFiles({
      name: 'fieldcompanion-e2e-photo.jpg',
      mimeType: 'image/jpeg',
      buffer: Buffer.from([0xff, 0xd8, 0xff, 0xd9]),
    })

    await page.getByTestId('fieldcompanion-evidence-submit').first().click()
    await expect(page.getByTestId('fieldcompanion-evidence-success').first()).toBeVisible({
      timeout: 20_000,
    })
  })
})
