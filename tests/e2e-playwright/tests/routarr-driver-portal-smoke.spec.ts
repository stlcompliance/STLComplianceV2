import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr driver portal @requires-live', () => {
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
  })

  test('handoff opens driver portal with today and upcoming schedule sections', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/driver-portal', page.url()).toString())

    const panel = page.getByTestId('driver-portal-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Driver portal' })).toBeVisible()

    const todaySection = panel.getByTestId('driver-portal-today')
    const upcomingSection = panel.getByTestId('driver-portal-upcoming')
    await expect(todaySection).toBeVisible()
    await expect(upcomingSection).toBeVisible()
    await expect(todaySection.getByRole('heading', { name: 'Today' })).toBeVisible()
    await expect(upcomingSection.getByRole('heading', { name: 'Upcoming' })).toBeVisible()

    const todayTrips = todaySection.locator('[data-testid^="driver-portal-trip-"]')
    const upcomingTrips = upcomingSection.locator('[data-testid^="driver-portal-trip-"]')
    const todayEmpty = todaySection.getByText('No trips scheduled or active for today.')
    const upcomingEmpty = upcomingSection.getByText(
      'No upcoming assigned trips in the next week.',
    )

    const hasTodayTrips = (await todayTrips.count()) > 0
    const hasTodayEmpty = (await todayEmpty.count()) > 0
    const hasUpcomingTrips = (await upcomingTrips.count()) > 0
    const hasUpcomingEmpty = (await upcomingEmpty.count()) > 0

    expect(hasTodayTrips || hasTodayEmpty).toBeTruthy()
    expect(hasUpcomingTrips || hasUpcomingEmpty).toBeTruthy()

    const dispatchButton = panel.getByRole('button', { name: 'Dispatch' })
    const startButton = panel.getByRole('button', { name: 'Start trip' })
    if ((await dispatchButton.count()) > 0) {
      await expect(dispatchButton.first()).toBeVisible()
    }
    if ((await startButton.count()) > 0) {
      await expect(startButton.first()).toBeVisible()
    }
  })
})
