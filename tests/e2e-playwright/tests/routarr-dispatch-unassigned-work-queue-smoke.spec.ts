import { test, expect } from '@playwright/test'

import { ensureRoutArrFieldInboxFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch unassigned work queue @requires-live', () => {
  let mayHaveUnassignedTrips = false

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
      mayHaveUnassignedTrips = true
    } catch {
      mayHaveUnassignedTrips = false
    }
  })

  test('handoff opens unassigned work queue with assign controls or empty state', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Unassigned work queue' })).toBeVisible()

    const tripRows = panel.locator('[data-testid^="unassigned-trip-"]')
    const listEmpty = panel.getByText('No unassigned active trips in this window.')
    const assignDriverSelect = panel.getByLabel('Bulk assign driver', { exact: false })
    const bulkAssignButton = panel.getByTestId('bulk-assign-unassigned')

    const hasRows = (await tripRows.count()) > 0
    const hasEmpty = (await listEmpty.count()) > 0
    expect(hasRows || hasEmpty).toBeTruthy()

    if (hasRows) {
      const perTripAssign = panel.getByRole('button', { name: 'Assign' }).first()
      await expect(perTripAssign).toBeVisible()
      if (mayHaveUnassignedTrips) {
        await expect(assignDriverSelect).toBeVisible()
        await expect(bulkAssignButton).toBeVisible()
      } else {
        const hasBulkControls =
          (await assignDriverSelect.count()) > 0 && (await bulkAssignButton.count()) > 0
        expect(hasBulkControls).toBeTruthy()
      }
    }
  })
})
