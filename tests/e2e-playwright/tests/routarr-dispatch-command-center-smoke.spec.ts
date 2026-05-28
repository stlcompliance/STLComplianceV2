import { test, expect } from '@playwright/test'

import { ensureRoutArrFieldInboxFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch command center @requires-live', () => {
  let tripColumnHasData = false

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
      tripColumnHasData = true
    } catch {
      tripColumnHasData = false
    }
  })

  test('handoff opens dispatch command center with scope toggle and status columns', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('dispatch-command-center-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Dispatch command center' })).toBeVisible()

    const dailyScope = panel.getByRole('button', { name: 'daily' })
    const weeklyScope = panel.getByRole('button', { name: 'weekly' })
    await expect(dailyScope).toBeVisible()
    await expect(weeklyScope).toBeVisible()

    await weeklyScope.click()
    await expect(weeklyScope).toHaveClass(/bg-sky-700/)

    const firstColumn = panel.locator('[data-testid^="trip-column-"]').first()
    await expect(firstColumn).toBeVisible({ timeout: 15_000 })

    if (tripColumnHasData) {
      await expect(panel.locator('p.font-medium.text-slate-100').first()).toBeVisible({
        timeout: 15_000,
      })
    } else {
      const hasTripCard = (await panel.locator('p.font-medium.text-slate-100').count()) > 0
      const hasEmptyState = (await panel.getByText('No trips').count()) > 0
      expect(hasTripCard || hasEmptyState).toBeTruthy()
    }

    await dailyScope.click()
    await expect(dailyScope).toHaveClass(/bg-sky-700/)
  })
})
