import { test, expect } from '@playwright/test'

import { ensureRoutArrFieldInboxFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch proof/DVIR read @requires-live', () => {
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
      const fixture = await ensureRoutArrFieldInboxFixture()
      fixtureTripId = fixture.tripId
    } catch {
      fixtureTripId = null
    }
  })

  test('handoff opens trip proof/DVIR read panel with lookup and execution summary or empty proof/DVIR', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('trip-proof-dvir-read-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Trip proof & DVIR' })).toBeVisible()

    const tripInput = panel.getByPlaceholder('Paste trip GUID')
    const loadButton = panel.getByRole('button', { name: 'Load execution' })
    await expect(tripInput).toBeVisible()
    await expect(loadButton).toBeVisible()
    await expect(loadButton).toBeDisabled()

    if (!fixtureTripId) {
      await tripInput.fill('00000000-0000-0000-0000-000000000001')
      await expect(loadButton).toBeEnabled()
      return
    }

    await tripInput.fill(fixtureTripId)
    await expect(loadButton).toBeEnabled()
    await loadButton.click()

    await expect(panel.getByText(/pre DVIR/)).toBeVisible({ timeout: 15_000 })

    const proofRows = panel.locator('[data-testid^="proof-row-"]')
    const dvirRows = panel.locator('[data-testid^="dvir-row-"]')
    const hasProofRows = (await proofRows.count()) > 0
    const hasDvirRows = (await dvirRows.count()) > 0
    const hasEmptyProof = (await panel.getByText('No proof captured.').count()) > 0
    const hasEmptyDvir = (await panel.getByText('No DVIR submitted.').count()) > 0

    expect(hasProofRows || hasEmptyProof).toBeTruthy()
    expect(hasDvirRows || hasEmptyDvir).toBeTruthy()
  })
})
