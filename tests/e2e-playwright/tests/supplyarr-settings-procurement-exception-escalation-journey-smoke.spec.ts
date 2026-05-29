import { test, expect } from '@playwright/test'

import {
  assertSupplyArrProcurementExceptionEscalationEventsContainFromHandoff,
  assertSupplyArrProcurementExceptionEscalationPendingContainsFromHandoff,
  assertSupplyArrProcurementExceptionEscalationRunEscalatedFromHandoff,
  assertSupplyArrProcurementNotificationDispatchPendingFromHandoff,
  ensureSupplyArrProcurementExceptionEscalationJourneyFixture,
  issueSupplyArrProcurementExceptionEscalationWorkerToken,
  loginNexArr,
  processSupplyArrProcurementExceptionEscalationBatch,
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
  'SupplyArr settings procurement exception escalation journey @requires-live',
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

    test('pending preview then process-batch shows escalation event and notification dispatch', async ({
      page,
    }) => {
      test.skip(!hasJourneyFixture, 'Escalation journey fixture could not be seeded.')

      await signInFromSuite(page)
      await launchProductHandoffFromSuite(page, 'supplyarr')

      await page.goto(new URL('/settings', page.url()).toString())

      const panel = page.getByTestId('procurement-exception-escalation-settings-panel')
      await panel.scrollIntoViewIfNeeded()
      await expect(panel).toBeVisible({ timeout: 15_000 })

      await expect(panel.getByTestId('procurement-exception-escalation-enabled')).toHaveJSProperty(
        'checked',
        true,
      )

      await assertSupplyArrProcurementExceptionEscalationPendingContainsFromHandoff(
        overdueExceptionKey,
      )
      await expect(
        panel.getByTestId(`procurement-exception-escalation-pending-${overdueExceptionKey}`),
      ).toBeVisible({ timeout: 15_000 })

      const adminToken = await loginNexArr()
      const workerToken = await issueSupplyArrProcurementExceptionEscalationWorkerToken(adminToken)
      const batch = await processSupplyArrProcurementExceptionEscalationBatch(workerToken)
      expect(batch.escalatedCount).toBeGreaterThanOrEqual(1)

      await assertSupplyArrProcurementExceptionEscalationEventsContainFromHandoff(
        overdueExceptionKey,
      )
      await assertSupplyArrProcurementExceptionEscalationRunEscalatedFromHandoff(1)
      await assertSupplyArrProcurementNotificationDispatchPendingFromHandoff(
        procurementExceptionSlaEscalationEventKind,
        overdueExceptionId,
      )

      await page.reload()
      await expect(panel).toBeVisible({ timeout: 15_000 })
      await expect(
        panel.getByTestId(`procurement-exception-escalation-event-${overdueExceptionKey}`),
      ).toBeVisible({ timeout: 15_000 })
      await expect(
        panel.getByTestId(`procurement-exception-escalation-event-${overdueExceptionKey}`),
      ).toContainText('Level 1')

      const runsList = panel.getByTestId('procurement-exception-escalation-runs-list')
      await expect(runsList).toBeVisible({ timeout: 15_000 })
      await expect(panel.getByTestId('procurement-exception-escalation-run-summary').first()).toContainText(
        '1 escalated',
      )
    })
  },
)
