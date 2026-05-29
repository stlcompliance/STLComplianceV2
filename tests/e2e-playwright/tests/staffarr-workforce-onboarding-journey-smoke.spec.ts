import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('StaffArr workforce onboarding journey @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('staffarr'))) {
      testInfo.skip(true, 'StaffArr frontend (5175) is unreachable.')
    }
  })

  test('people workspace shows create person and onboarding journey panels', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'staffarr')

    await page.goto(new URL('/people', page.url()).toString())

    const createPanel = page.getByTestId('create-person-panel')
    await expect(createPanel).toBeVisible({ timeout: 15_000 })
    await expect(createPanel.getByRole('heading', { name: 'Create person' })).toBeVisible()

    const journeyPanel = page.getByTestId('workforce-onboarding-journey-panel')
    await expect(journeyPanel).toBeVisible({ timeout: 15_000 })
    await expect(
      journeyPanel.getByRole('heading', { name: /New employee → qualified worker/i }),
    ).toBeVisible()

    await expect(journeyPanel.getByTestId('workforce-onboarding-journey-summary')).toBeVisible({
      timeout: 15_000,
    })

    const steps = journeyPanel.getByTestId('workforce-onboarding-journey-steps')
    await expect(steps.getByTestId('workforce-onboarding-step-workforce_profile')).toBeVisible()
  })
})
