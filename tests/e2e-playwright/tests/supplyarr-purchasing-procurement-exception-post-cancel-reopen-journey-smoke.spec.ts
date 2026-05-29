import { test, expect } from '@playwright/test'

import {
  assertSupplyArrProcurementExceptionCancelledWithReasonFromHandoff,
  assertSupplyArrProcurementExceptionReopenedFromHandoff,
  assertSupplyArrProcurementExceptionStatusFromHandoff,
  ensureSupplyArrProcurementExceptionPostCancelReopenJourneyFixture,
  supplyArrProcurementExceptionCancelJourneyReason,
  supplyArrProcurementExceptionPostCancelReopenJourneyReason,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe(
  'SupplyArr purchasing procurement exception post-cancel reopen journey @requires-live',
  () => {
    let hasJourneyFixture = false
    let purchaseRequestId = ''
    let openExceptionId = ''

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
        const fixture = await ensureSupplyArrProcurementExceptionPostCancelReopenJourneyFixture()
        hasJourneyFixture = Boolean(fixture.openExceptionId)
        purchaseRequestId = fixture.purchaseRequestId
        openExceptionId = fixture.openExceptionId
      } catch {
        hasJourneyFixture = false
        purchaseRequestId = ''
        openExceptionId = ''
      }
    })

    test('handoff investigates, cancels, then reopens with reason', async ({ page }) => {
      test.skip(!hasJourneyFixture, 'Post-cancel reopen journey fixture could not be seeded.')

      await signInFromSuite(page)
      await launchProductHandoffFromSuite(page, 'supplyarr')

      await page.goto(new URL('/purchasing', page.url()).toString())

      const panel = page.getByTestId('procurement-exceptions-panel')
      await panel.scrollIntoViewIfNeeded()
      await expect(panel).toBeVisible({ timeout: 15_000 })

      await panel.getByTestId('procurement-exception-subject-record').selectOption(purchaseRequestId)

      const openRow = panel.getByTestId(`procurement-exception-row-${openExceptionId}`)
      await expect(openRow).toBeVisible({ timeout: 15_000 })

      const statusBadge = panel.getByTestId(`procurement-exception-status-${openExceptionId}`)
      await expect(statusBadge).toHaveText('open')

      const investigateButton = panel.getByTestId(
        `procurement-exception-investigate-${openExceptionId}`,
      )
      await expect(investigateButton).toBeVisible()
      await investigateButton.click()

      await expect(statusBadge).toHaveText('investigating', { timeout: 15_000 })
      await assertSupplyArrProcurementExceptionStatusFromHandoff(openExceptionId, 'investigating')

      await panel
        .getByTestId('procurement-exception-cancel-reason')
        .fill(supplyArrProcurementExceptionCancelJourneyReason)

      const cancelButton = panel.getByTestId(`procurement-exception-cancel-${openExceptionId}`)
      await expect(cancelButton).toBeVisible()
      await cancelButton.click()

      await expect(statusBadge).toHaveText('cancelled', { timeout: 15_000 })
      await assertSupplyArrProcurementExceptionCancelledWithReasonFromHandoff(
        openExceptionId,
        supplyArrProcurementExceptionCancelJourneyReason,
      )

      await expect(
        panel.getByTestId(`procurement-exception-cancel-${openExceptionId}`),
      ).toHaveCount(0)

      await panel
        .getByTestId('procurement-exception-reopen-reason')
        .fill(supplyArrProcurementExceptionPostCancelReopenJourneyReason)

      const reopenButton = panel.getByTestId(`procurement-exception-reopen-${openExceptionId}`)
      await expect(reopenButton).toBeVisible()
      await reopenButton.click()

      await expect(statusBadge).toHaveText('investigating', { timeout: 15_000 })
      await assertSupplyArrProcurementExceptionReopenedFromHandoff(
        openExceptionId,
        supplyArrProcurementExceptionPostCancelReopenJourneyReason,
      )

      await expect(
        panel.getByTestId(`procurement-exception-reopen-${openExceptionId}`),
      ).toHaveCount(0)
      await expect(
        panel.getByTestId(`procurement-exception-resolve-${openExceptionId}`),
      ).toBeVisible()
    })
  },
)
