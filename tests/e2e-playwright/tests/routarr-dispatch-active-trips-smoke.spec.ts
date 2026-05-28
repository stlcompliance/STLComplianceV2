import { test, expect } from '@playwright/test'

import { ensureRoutArrFieldInboxFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch active trips @requires-live', () => {
  let mayHaveActiveTrips = false

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
      mayHaveActiveTrips = true
    } catch {
      mayHaveActiveTrips = false
    }
  })

  test('handoff opens active trips panel with list/map toggle and trips or empty state', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('active-trips-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Active trips' })).toBeVisible()

    const listButton = panel.getByRole('button', { name: 'list' })
    const mapButton = panel.getByRole('button', { name: 'map' })
    await expect(listButton).toBeVisible()
    await expect(mapButton).toBeVisible()
    await expect(listButton).toHaveClass(/bg-emerald-700/)

    const tripRows = panel.locator('[data-testid^="active-trip-row-"]')
    const listEmpty = panel.getByText('No dispatched or in-progress trips in this window.')

    if (mayHaveActiveTrips) {
      const rowCount = await tripRows.count()
      const emptyVisible = (await listEmpty.count()) > 0
      expect(rowCount > 0 || emptyVisible).toBeTruthy()
    } else {
      const hasRows = (await tripRows.count()) > 0
      const hasEmpty = (await listEmpty.count()) > 0
      expect(hasRows || hasEmpty).toBeTruthy()
    }

    await mapButton.click()
    await expect(mapButton).toHaveClass(/bg-emerald-700/)

    const mapStrip = panel.getByTestId('active-trips-map')
    await expect(mapStrip).toBeVisible()

    const mapBlocks = panel.locator('[data-testid^="active-trip-map-"]')
    const mapEmpty = mapStrip.getByText('No active trips in window')
    const hasMapBlocks = (await mapBlocks.count()) > 0
    const hasMapEmpty = (await mapEmpty.count()) > 0
    expect(hasMapBlocks || hasMapEmpty).toBeTruthy()

    await listButton.click()
    await expect(listButton).toHaveClass(/bg-emerald-700/)
    await expect(mapStrip).not.toBeVisible()
  })
})
