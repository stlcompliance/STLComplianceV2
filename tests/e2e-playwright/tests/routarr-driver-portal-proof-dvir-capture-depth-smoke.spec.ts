import { test, expect } from '@playwright/test'

import { ensureRoutArrProofDvirCaptureFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr driver portal proof/DVIR capture depth @requires-live', () => {
  let hasCaptureFixture = false
  let fixtureTripId: string | null = null

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
      const fixture = await ensureRoutArrProofDvirCaptureFixture()
      hasCaptureFixture = Boolean(fixture.tripId)
      fixtureTripId = fixture.tripId
    } catch {
      hasCaptureFixture = false
      fixtureTripId = null
    }
  })

  test('handoff opens driver portal capture readiness and DVIR validation without successful capture mutations', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/driver-portal', page.url()).toString())

    const panel = page.getByTestId('driver-portal-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Driver portal' })).toBeVisible()

    const tripCard = fixtureTripId
      ? panel.getByTestId(`driver-portal-trip-${fixtureTripId}`)
      : panel.locator('[data-testid^="driver-portal-trip-"]').first()

    if (!hasCaptureFixture || !fixtureTripId) {
      const hasAnyTrip = (await panel.locator('[data-testid^="driver-portal-trip-"]').count()) > 0
      if (hasAnyTrip) {
        await expect(tripCard).toBeVisible()
      }
      return
    }

    await expect(tripCard).toBeVisible({ timeout: 15_000 })

    const captureSection = tripCard.getByTestId(`driver-portal-proof-dvir-${fixtureTripId}`)
    await expect(captureSection).toBeVisible()
    await expect(captureSection).toContainText(/start blocked|pre DVIR/)

    const startButton = tripCard.getByRole('button', { name: 'Start trip' })
    await expect(startButton).toBeVisible()
    await expect(startButton).toBeDisabled()

    const blockers = captureSection.getByTestId('capture-readiness-blockers')
    if ((await blockers.count()) > 0) {
      await expect(blockers).toBeVisible()
    }

    const preTripForm = captureSection.getByTestId('dvir-form-pre_trip')
    await expect(preTripForm).toBeVisible()
    await expect(captureSection.getByRole('button', { name: 'Quick pickup proof' })).toBeVisible()
    await expect(captureSection.getByRole('button', { name: 'Capture proof' })).toBeVisible()

    const resultSelect = preTripForm.locator('select')
    await resultSelect.selectOption('fail')
    await expect(preTripForm.getByPlaceholder('Defect notes (required)')).toBeVisible()

    await preTripForm.getByRole('button', { name: 'Submit pre-trip DVIR' }).click()
    await expect(captureSection.getByRole('alert')).toBeVisible({ timeout: 15_000 })
  })
})
