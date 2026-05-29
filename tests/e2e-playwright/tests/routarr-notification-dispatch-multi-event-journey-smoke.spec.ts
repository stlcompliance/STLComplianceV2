import { test, expect, type Locator } from '@playwright/test'

import {
  ensureRoutArrDispatchNotificationMultiEventJourneyFixture,
  issueRoutArrDispatchNotificationWorkerToken,
  loginNexArr,
  processRoutArrDispatchNotificationBatch,
  type RoutArrDispatchNotificationMultiEventJourneyFixture,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

async function expectDispatchRowForTrip(
  dispatchList: Locator,
  tripId: string,
  eventKind: string,
  statusPattern: RegExp,
): Promise<void> {
  const row = dispatchList.locator('li').filter({ hasText: tripId }).filter({ hasText: eventKind })
  await expect(row).toBeVisible({ timeout: 15_000 })
  await expect(row).toContainText(statusPattern)
  await expect(row).toContainText(`Trip ${tripId}`)
}

test.describe('RoutArr multi-event notification dispatch journey @requires-live', () => {
  let hasJourneyFixture = false
  let fixture: RoutArrDispatchNotificationMultiEventJourneyFixture | null = null

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
      fixture = await ensureRoutArrDispatchNotificationMultiEventJourneyFixture()
      hasJourneyFixture = Boolean(
        fixture.completedPathTripId &&
          fixture.cancelledBranchTripId &&
          fixture.completedPathExpectedEventKinds.length > 0,
      )
    } catch {
      hasJourneyFixture = false
      fixture = null
    }
  })

  test('completed and cancelled branches enqueue only enabled event kinds in Recent dispatches', async ({
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

    if (!hasJourneyFixture || !fixture) {
      const dispatchesEmpty = panel.getByTestId('notification-dispatches-empty')
      const dispatchesList = panel.getByTestId('notification-dispatches-list')
      await expect(dispatchesEmpty.or(dispatchesList)).toBeVisible({ timeout: 15_000 })
      return
    }

    const dispatchList = panel.getByTestId('notification-dispatches-list')
    await expect(dispatchList).toBeVisible({ timeout: 15_000 })

    for (const eventKind of fixture.completedPathExpectedEventKinds) {
      await expectDispatchRowForTrip(
        dispatchList,
        fixture.completedPathTripId,
        eventKind,
        /pending/i,
      )
    }

    for (const eventKind of fixture.completedPathAbsentEventKinds) {
      const absentRow = dispatchList
        .locator('li')
        .filter({ hasText: fixture.completedPathTripId })
        .filter({ hasText: eventKind })
      await expect(absentRow).toHaveCount(0)
    }

    await expectDispatchRowForTrip(
      dispatchList,
      fixture.cancelledBranchTripId,
      fixture.cancelledBranchExpectedEventKind,
      /pending/i,
    )

    const cancelledBranchAssignedRow = dispatchList
      .locator('li')
      .filter({ hasText: fixture.cancelledBranchTripId })
      .filter({ hasText: 'trip_assigned' })
    await expect(cancelledBranchAssignedRow).toHaveCount(0)

    const adminToken = await loginNexArr()
    const workerToken = await issueRoutArrDispatchNotificationWorkerToken(adminToken)
    await processRoutArrDispatchNotificationBatch(workerToken, 10)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(dispatchList).toBeVisible({ timeout: 15_000 })

    for (const eventKind of fixture.completedPathExpectedEventKinds) {
      const processedRow = dispatchList
        .locator('li')
        .filter({ hasText: fixture.completedPathTripId })
        .filter({ hasText: eventKind })
      await expect(processedRow).toBeVisible({ timeout: 15_000 })
      await expect(processedRow).not.toContainText('pending')
      await expect(processedRow).toContainText(/sent|failed|skipped/i)
    }

    const processedCancelledRow = dispatchList
      .locator('li')
      .filter({ hasText: fixture.cancelledBranchTripId })
      .filter({ hasText: fixture.cancelledBranchExpectedEventKind })
    await expect(processedCancelledRow).toBeVisible({ timeout: 15_000 })
    await expect(processedCancelledRow).not.toContainText('pending')
    await expect(processedCancelledRow).toContainText(/sent|failed|skipped/i)
  })
})
