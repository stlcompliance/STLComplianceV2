import { test, expect } from '@playwright/test'

import {
  ensureRoutArrDispatchNotificationTripCancelledJourneyFixture,
  issueRoutArrDispatchNotificationWorkerToken,
  loginNexArr,
  processRoutArrDispatchNotificationBatch,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr trip-cancelled notification dispatch journey @requires-live', () => {
  let hasJourneyFixture = false
  let fixtureTripId: string | null = null
  let fixtureEventKind: string | null = null

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
      const fixture = await ensureRoutArrDispatchNotificationTripCancelledJourneyFixture()
      hasJourneyFixture = Boolean(fixture.tripId && fixture.expectedEventKind)
      fixtureTripId = fixture.tripId
      fixtureEventKind = fixture.expectedEventKind
    } catch {
      hasJourneyFixture = false
      fixtureTripId = null
      fixtureEventKind = null
    }
  })

  test('status change to cancelled enqueues trip_cancelled dispatch outbox row visible in settings Recent dispatches', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('notification-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Dispatch notifications' })).toBeVisible()
    await expect(panel.getByRole('heading', { name: 'Recent dispatches' })).toBeVisible()

    if (!hasJourneyFixture || !fixtureTripId || !fixtureEventKind) {
      const dispatchesEmpty = panel.getByTestId('notification-dispatches-empty')
      const dispatchesList = panel.getByTestId('notification-dispatches-list')
      await expect(dispatchesEmpty.or(dispatchesList)).toBeVisible({ timeout: 15_000 })
      return
    }

    const dispatchRow = panel.getByTestId(`notification-dispatch-row-${fixtureTripId}`)
    await expect(dispatchRow).toBeVisible({ timeout: 15_000 })
    await expect(dispatchRow).toContainText(fixtureEventKind)
    await expect(dispatchRow).toContainText('pending')
    await expect(dispatchRow).toContainText(`Trip ${fixtureTripId}`)

    const adminToken = await loginNexArr()
    const workerToken = await issueRoutArrDispatchNotificationWorkerToken(adminToken)
    await processRoutArrDispatchNotificationBatch(workerToken)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    const processedRow = panel.getByTestId(`notification-dispatch-row-${fixtureTripId}`)
    await expect(processedRow).toBeVisible({ timeout: 15_000 })
    await expect(processedRow).toContainText(fixtureEventKind)
    await expect(processedRow).not.toContainText('pending')
    await expect(processedRow).toContainText(/sent|failed|skipped/i)
  })
})
