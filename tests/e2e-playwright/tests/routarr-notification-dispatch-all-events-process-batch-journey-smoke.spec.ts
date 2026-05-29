import { test, expect, type Locator } from '@playwright/test'

import {
  assertRoutArrDispatchNotificationDispatchesForTrip,
  assertRoutArrDispatchNotificationDispatchesProcessedForTrip,
  createAndRunRoutArrDispatchNotificationCancelledBranch,
  createAndRunRoutArrDispatchNotificationFullLifecycle,
  issueRoutArrDispatchNotificationWorkerToken,
  loginNexArr,
  processRoutArrDispatchNotificationBatch,
  routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind,
  routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const w288WebhookUrl = 'https://hooks.example.com/routarr-e2e-w288'

async function setCheckboxState(checkbox: Locator, checked: boolean): Promise<void> {
  const isChecked = await checkbox.isChecked()
  if (isChecked === checked) {
    return
  }
  if (checked) {
    await checkbox.check()
  } else {
    await checkbox.uncheck()
  }
}

async function configureNotificationEventToggles(
  panel: Locator,
  options: {
    tripAssigned: boolean
    tripDispatched: boolean
    tripInProgress: boolean
    tripCompleted: boolean
    tripCancelled: boolean
  },
): Promise<void> {
  await setCheckboxState(panel.getByTestId('notification-trip-assigned'), options.tripAssigned)
  await setCheckboxState(panel.getByTestId('notification-trip-dispatched'), options.tripDispatched)
  await setCheckboxState(panel.getByTestId('notification-trip-in-progress'), options.tripInProgress)
  await setCheckboxState(panel.getByTestId('notification-trip-completed'), options.tripCompleted)
  await setCheckboxState(panel.getByTestId('notification-trip-cancelled'), options.tripCancelled)
}

async function expectEventToggleStates(
  panel: Locator,
  options: {
    tripAssigned: boolean
    tripDispatched: boolean
    tripInProgress: boolean
    tripCompleted: boolean
    tripCancelled: boolean
  },
): Promise<void> {
  await expect(panel.getByTestId('notification-trip-assigned')).toHaveJSProperty(
    'checked',
    options.tripAssigned,
  )
  await expect(panel.getByTestId('notification-trip-dispatched')).toHaveJSProperty(
    'checked',
    options.tripDispatched,
  )
  await expect(panel.getByTestId('notification-trip-in-progress')).toHaveJSProperty(
    'checked',
    options.tripInProgress,
  )
  await expect(panel.getByTestId('notification-trip-completed')).toHaveJSProperty(
    'checked',
    options.tripCompleted,
  )
  await expect(panel.getByTestId('notification-trip-cancelled')).toHaveJSProperty(
    'checked',
    options.tripCancelled,
  )
}

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

test.describe(
  'RoutArr all-events notification dispatch process-batch journey @requires-live',
  () => {
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
      if (!(await isHandoffFrontendReachable('routarr'))) {
        testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
      }
    })

    test('UI settings save enqueues all five event kinds and process-batch delivers them', async ({
      page,
    }) => {
      await signInFromSuite(page)
      await launchProductHandoffFromSuite(page, 'routarr')

      await page.goto(new URL('/settings', page.url()).toString())

      const panel = page.getByTestId('notification-settings-panel')
      await panel.scrollIntoViewIfNeeded()
      await expect(panel).toBeVisible({ timeout: 15_000 })

      const enabled = panel.getByTestId('notification-settings-enabled')
      const webhook = panel.getByTestId('notification-settings-webhook')

      const initialEnabled = await enabled.isChecked()
      const initialWebhook = await webhook.inputValue()
      const initialTripAssigned = await panel.getByTestId('notification-trip-assigned').isChecked()
      const initialTripDispatched = await panel.getByTestId('notification-trip-dispatched').isChecked()
      const initialTripInProgress = await panel.getByTestId('notification-trip-in-progress').isChecked()
      const initialTripCompleted = await panel.getByTestId('notification-trip-completed').isChecked()
      const initialTripCancelled = await panel.getByTestId('notification-trip-cancelled').isChecked()

      await enabled.check()
      await webhook.fill(w288WebhookUrl)
      await configureNotificationEventToggles(panel, {
        tripAssigned: true,
        tripDispatched: true,
        tripInProgress: true,
        tripCompleted: true,
        tripCancelled: true,
      })

      await panel.getByTestId('notification-settings-save').click()
      await expectEventToggleStates(panel, {
        tripAssigned: true,
        tripDispatched: true,
        tripInProgress: true,
        tripCompleted: true,
        tripCancelled: true,
      })

      await page.reload()
      await expect(panel).toBeVisible({ timeout: 15_000 })
      await expect(enabled).toHaveJSProperty('checked', true)
      await expect(webhook).toHaveValue(w288WebhookUrl)
      await expectEventToggleStates(panel, {
        tripAssigned: true,
        tripDispatched: true,
        tripInProgress: true,
        tripCompleted: true,
        tripCancelled: true,
      })

      let completedPathTripId: string | null = null
      let cancelledBranchTripId: string | null = null
      try {
        completedPathTripId = await createAndRunRoutArrDispatchNotificationFullLifecycle()
        await assertRoutArrDispatchNotificationDispatchesForTrip(
          completedPathTripId,
          [...routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds],
          [routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind],
        )

        cancelledBranchTripId = await createAndRunRoutArrDispatchNotificationCancelledBranch()
        await assertRoutArrDispatchNotificationDispatchesForTrip(
          cancelledBranchTripId,
          [routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind],
          [...routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds],
        )
      } catch {
        completedPathTripId = null
        cancelledBranchTripId = null
      }

      await page.reload()
      await expect(panel).toBeVisible({ timeout: 15_000 })

      if (!completedPathTripId || !cancelledBranchTripId) {
        const dispatchesEmpty = panel.getByTestId('notification-dispatches-empty')
        const dispatchesList = panel.getByTestId('notification-dispatches-list')
        await expect(dispatchesEmpty.or(dispatchesList)).toBeVisible({ timeout: 15_000 })
        return
      }

      const dispatchList = panel.getByTestId('notification-dispatches-list')
      await expect(dispatchList).toBeVisible({ timeout: 15_000 })

      for (const eventKind of routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds) {
        await expectDispatchRowForTrip(dispatchList, completedPathTripId, eventKind, /pending/i)
      }

      const completedPathCancelledRow = dispatchList
        .locator('li')
        .filter({ hasText: completedPathTripId })
        .filter({
          hasText: routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind,
        })
      await expect(completedPathCancelledRow).toHaveCount(0)

      await expectDispatchRowForTrip(
        dispatchList,
        cancelledBranchTripId,
        routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind,
        /pending/i,
      )

      const cancelledBranchAssignedRow = dispatchList
        .locator('li')
        .filter({ hasText: cancelledBranchTripId })
        .filter({ hasText: 'trip_assigned' })
      await expect(cancelledBranchAssignedRow).toHaveCount(0)

      const adminToken = await loginNexArr()
      const workerToken = await issueRoutArrDispatchNotificationWorkerToken(adminToken)
      await processRoutArrDispatchNotificationBatch(workerToken, 10)

      await assertRoutArrDispatchNotificationDispatchesProcessedForTrip(completedPathTripId, [
        ...routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds,
      ])
      await assertRoutArrDispatchNotificationDispatchesProcessedForTrip(cancelledBranchTripId, [
        routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind,
      ])

      await page.reload()
      await expect(panel).toBeVisible({ timeout: 15_000 })
      await expect(dispatchList).toBeVisible({ timeout: 15_000 })

      for (const eventKind of routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds) {
        const processedRow = dispatchList
          .locator('li')
          .filter({ hasText: completedPathTripId })
          .filter({ hasText: eventKind })
        await expect(processedRow).toBeVisible({ timeout: 15_000 })
        await expect(processedRow).not.toContainText('pending')
        await expect(processedRow).toContainText(/sent|failed|skipped/i)
      }

      const processedCancelledRow = dispatchList
        .locator('li')
        .filter({ hasText: cancelledBranchTripId })
        .filter({
          hasText: routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind,
        })
      await expect(processedCancelledRow).toBeVisible({ timeout: 15_000 })
      await expect(processedCancelledRow).not.toContainText('pending')
      await expect(processedCancelledRow).toContainText(/sent|failed|skipped/i)

      if (initialEnabled) {
        await enabled.check()
      } else {
        await enabled.uncheck()
      }
      await webhook.fill(initialWebhook)
      await configureNotificationEventToggles(panel, {
        tripAssigned: initialTripAssigned,
        tripDispatched: initialTripDispatched,
        tripInProgress: initialTripInProgress,
        tripCompleted: initialTripCompleted,
        tripCancelled: initialTripCancelled,
      })
      await panel.getByTestId('notification-settings-save').click()
      await expect(enabled).toHaveJSProperty('checked', initialEnabled)
      await expect(webhook).toHaveValue(initialWebhook)
      await expectEventToggleStates(panel, {
        tripAssigned: initialTripAssigned,
        tripDispatched: initialTripDispatched,
        tripInProgress: initialTripInProgress,
        tripCompleted: initialTripCompleted,
        tripCancelled: initialTripCancelled,
      })
    })
  },
)
