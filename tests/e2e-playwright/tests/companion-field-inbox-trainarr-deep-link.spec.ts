import { test, expect } from '@playwright/test'
import { ensureTrainArrFieldInboxFixture } from '../support/e2eApi.js'
import {
  isCompanionFrontendReachable,
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { companionFrontend, handoffProductFrontends, handoffUrlPattern } from '../support/productFrontends.js'

test.describe('Field Companion field inbox → TrainArr deep links @requires-live', () => {
  let assignmentId: string

  test.beforeAll(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(true, 'Suite and NexArr API must be reachable for live E2E.')
    }
    if (!(await isCompanionFrontendReachable()) || !(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'Field Companion (5181) and TrainArr (5176) previews must be running.')
    }

    const fixture = await ensureTrainArrFieldInboxFixture()
    assignmentId = fixture.trainingAssignmentId
  })

  test('inbox Open in TrainArr navigates to assignment workspace', async ({ page }, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }

    const trainarr = handoffProductFrontends.find((p) => p.productKey === 'trainarr')!

    await signInFromSuite(page)
    await page.goto('/app/field-companion/launch')

    const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
    await expect(launchButton).toBeVisible({ timeout: 15_000 })

    await Promise.all([
      page.waitForURL(handoffUrlPattern(companionFrontend), { timeout: 30_000 }),
      launchButton.click(),
    ])

    await expect(page.getByRole('heading', { name: 'Field inbox' })).toBeVisible({ timeout: 20_000 })

    const trainarrFilter = page.getByRole('button', { name: /TrainArr/i })
    if (await trainarrFilter.isVisible()) {
      await trainarrFilter.click()
    }

    const openLink = page.getByRole('link', { name: /Open in TrainArr/i }).first()
    await expect(openLink).toBeVisible({ timeout: 15_000 })

    const href = await openLink.getAttribute('href')
    expect(href).toBeTruthy()
    expect(href).toMatch(new RegExp(`/assignments/${assignmentId}`, 'i'))

    await Promise.all([
      page.waitForURL(new RegExp(`/assignments/${assignmentId}`, 'i'), { timeout: 30_000 }),
      openLink.click(),
    ])

    await expect(page).toHaveURL(handoffUrlPattern(trainarr))
    await expect(page.getByTestId('assignment-workspace')).toBeVisible({ timeout: 15_000 })
  })
})
