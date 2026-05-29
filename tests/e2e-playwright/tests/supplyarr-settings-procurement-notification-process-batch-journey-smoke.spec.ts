import { test, expect } from '@playwright/test'

import {
  assertSupplyArrProcurementExceptionEscalationEventsContainFromHandoff,
  assertSupplyArrProcurementExceptionEscalationRunEscalatedFromHandoff,
  assertSupplyArrProcurementNotificationDispatchPendingFromHandoff,
  assertSupplyArrProcurementNotificationDispatchProcessedFromHandoff,
  ensureSupplyArrProcurementExceptionEscalationJourneyFixture,
  issueSupplyArrProcurementExceptionEscalationWorkerToken,
  issueSupplyArrProcurementNotificationWorkerToken,
  loginNexArr,
  processSupplyArrProcurementExceptionEscalationBatch,
  processSupplyArrProcurementNotificationBatch,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const procurementExceptionSlaEscalationEventKind = 'procurement_exception_sla_escalation'

test.describe(
  'SupplyArr procurement notification process-batch journey @requires-live',
  () => {
    let overdueExceptionKey = ''
    let overdueExceptionId = ''
    let hasJourneyFixture = false

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
      if (!(await isHandoffFrontendReachable('supplyarr'))) {
        testInfo.skip(true, 'SupplyArr frontend (5179) is unreachable.')
      }

      try {
        const fixture = await ensureSupplyArrProcurementExceptionEscalationJourneyFixture()
        overdueExceptionKey = fixture.overdueExceptionKey
        overdueExceptionId = fixture.overdueExceptionId
        hasJourneyFixture = true
      } catch {
        hasJourneyFixture = false
        overdueExceptionKey = ''
        overdueExceptionId = ''
      }
    })

    test('escalation enqueue then notification process-batch delivers SLA escalation dispatch', async ({
      page,
    }) => {
      test.skip(!hasJourneyFixture, 'Escalation journey fixture could not be seeded.')

      await signInFromSuite(page)
      await launchProductHandoffFromSuite(page, 'supplyarr')

      await page.goto(new URL('/settings', page.url()).toString())

      const notificationPanel = page.getByTestId('notification-settings-panel')
      await notificationPanel.scrollIntoViewIfNeeded()
      await expect(notificationPanel).toBeVisible({ timeout: 15_000 })
      await expect(notificationPanel.getByTestId('notification-settings-enabled')).toHaveJSProperty(
        'checked',
        true,
      )

      const adminToken = await loginNexArr()
      const escalationWorkerToken =
        await issueSupplyArrProcurementExceptionEscalationWorkerToken(adminToken)
      const escalationBatch =
        await processSupplyArrProcurementExceptionEscalationBatch(escalationWorkerToken)
      expect(escalationBatch.escalatedCount).toBeGreaterThanOrEqual(1)

      await assertSupplyArrProcurementExceptionEscalationEventsContainFromHandoff(
        overdueExceptionKey,
      )
      await assertSupplyArrProcurementExceptionEscalationRunEscalatedFromHandoff(1)
      await assertSupplyArrProcurementNotificationDispatchPendingFromHandoff(
        procurementExceptionSlaEscalationEventKind,
        overdueExceptionId,
      )

      await page.reload()
      await expect(notificationPanel).toBeVisible({ timeout: 15_000 })

      const dispatchList = notificationPanel.getByTestId('notification-dispatches-list')
      await expect(dispatchList).toBeVisible({ timeout: 15_000 })

      const pendingRow = notificationPanel.getByTestId(
        `notification-dispatch-row-${overdueExceptionId}`,
      )
      await expect(pendingRow).toBeVisible({ timeout: 15_000 })
      await expect(pendingRow).toContainText(procurementExceptionSlaEscalationEventKind)
      await expect(pendingRow).toContainText(/pending/i)

      const notificationWorkerToken =
        await issueSupplyArrProcurementNotificationWorkerToken(adminToken)
      const notificationBatch =
        await processSupplyArrProcurementNotificationBatch(notificationWorkerToken)
      expect(notificationBatch.dispatchedCount).toBeGreaterThanOrEqual(1)

      await assertSupplyArrProcurementNotificationDispatchProcessedFromHandoff(
        procurementExceptionSlaEscalationEventKind,
        overdueExceptionId,
      )

      await page.reload()
      await expect(notificationPanel).toBeVisible({ timeout: 15_000 })
      await expect(dispatchList).toBeVisible({ timeout: 15_000 })

      const processedRow = notificationPanel.getByTestId(
        `notification-dispatch-row-${overdueExceptionId}`,
      )
      await expect(processedRow).toBeVisible({ timeout: 15_000 })
      await expect(processedRow).not.toContainText(/pending/i)
      await expect(processedRow).toContainText(/sent|failed|skipped/i)
    })
  },
)
