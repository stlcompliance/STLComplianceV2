import { test, expect } from '@playwright/test'

import { ensureRoutArrFieldInboxFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch closeout panel @requires-live', () => {
  let hasOpenTripFixture = false

  test.beforeAll(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Use scripts/ops/e2e-stack-up.ps1 and e2e-frontends-preview.ps1.',
      )
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    try {
      await ensureRoutArrFieldInboxFixture()
      hasOpenTripFixture = true
    } catch {
      hasOpenTripFixture = false
    }
  })

  test('handoff opens dispatch closeout panel with checklist or summary and preview-only closeout', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('dispatch-closeout-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'End-of-day closeout' })).toBeVisible()

    await expect(panel.getByLabel('Remaining trips')).toBeVisible()
    await expect(panel.getByLabel('Open stops')).toBeVisible()

    const previewButton = panel.getByRole('button', { name: 'Preview closeout' })
    const applyButton = panel.getByRole('button', { name: 'Apply closeout' })
    await expect(previewButton).toBeVisible()
    await expect(applyButton).toBeVisible()
    await expect(applyButton).toBeDisabled()

    const checklistHeading = panel.getByText('Trip closeout checklist')
    const hasChecklist = (await checklistHeading.count()) > 0

    if (hasOpenTripFixture) {
      await expect(checklistHeading).toBeVisible({ timeout: 15_000 })
    } else if (hasChecklist) {
      await expect(checklistHeading).toBeVisible()
    }

    await previewButton.click()
    await expect(panel.getByText(/Preview \(all open\)|Preview \(\d+ selected\)/)).toBeVisible({
      timeout: 15_000,
    })
  })
})
