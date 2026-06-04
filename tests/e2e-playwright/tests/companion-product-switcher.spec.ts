import { test, expect } from '@playwright/test'
import { ensureTrainArrFieldInboxFixture } from '../support/e2eApi.js'
import {
  isCompanionFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import {
  companionFrontend,
  handoffProductFrontends,
  handoffUrlPattern,
} from '../support/productFrontends.js'

const trainarrFrontend = handoffProductFrontends.find((entry) => entry.productKey === 'trainarr')!

test.describe('Field Companion product switcher @requires-live', () => {
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

  test('handoffs from Field Companion to TrainArr via product switcher', async ({ page }, testInfo) => {
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

    const switcher = page.getByRole('button', { name: /Field Companion/i })
    await expect(switcher).toBeVisible({ timeout: 15_000 })
    await switcher.click()
    await page.getByRole('menuitem', { name: /TrainArr/i }).click()

    await expect(page).toHaveURL(handoffUrlPattern(trainarrFrontend), { timeout: 30_000 })
  })
})
