import { test, expect } from '@playwright/test'
import { ensureTrainArrFieldInboxFixture } from '../support/e2eApi.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { handoffProductFrontends, handoffUrlPattern } from '../support/productFrontends.js'

test.describe('TrainArr assignment deep-link routes @requires-live', () => {
  let assignmentId: string

  test.beforeAll(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(true, 'Suite and NexArr API must be reachable for live E2E.')
    }
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) preview must be running.')
    }

    const fixture = await ensureTrainArrFieldInboxFixture()
    assignmentId = fixture.trainingAssignmentId
  })

  test('evidence deep link opens assignment workspace', async ({ page }, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }

    const trainarr = handoffProductFrontends.find((p) => p.productKey === 'trainarr')!

    await signInFromSuite(page)
    await page.goto('/app/trainarr/launch')

    const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
    await expect(launchButton).toBeVisible({ timeout: 15_000 })

    await Promise.all([
      page.waitForURL(handoffUrlPattern(trainarr), { timeout: 30_000 }),
      launchButton.click(),
    ])

    await expect(page).not.toHaveURL(/handoff=/, { timeout: 25_000 })

    await page.goto(`/assignments/${assignmentId}/evidence`)

    await expect(page.getByTestId('assignment-workspace')).toBeVisible({ timeout: 15_000 })
    await expect(page.getByTestId('assignment-evidence-section')).toBeVisible()
  })
})
