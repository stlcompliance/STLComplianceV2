import { test, expect } from '@playwright/test'

import {
  assertSupplyArrProcurementExceptionPostRejectResolvedWithTemplateFromHandoff,
  assertSupplyArrProcurementExceptionRejectedWaiveFromHandoff,
  assertSupplyArrProcurementExceptionStatusFromHandoff,
  ensureSupplyArrProcurementExceptionPostRejectResolveJourneyFixture,
  supplyArrProcurementExceptionRejectWaiveDefaultReason,
  supplyArrProcurementExceptionRejectWaiveJourneyJustification,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const resolutionTemplateKey = 'pr_resubmit'

test.describe(
  'SupplyArr purchasing procurement exception post-reject resolve journey @requires-live',
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
        const fixture = await ensureSupplyArrProcurementExceptionPostRejectResolveJourneyFixture()
        hasJourneyFixture = Boolean(fixture.openExceptionId)
        purchaseRequestId = fixture.purchaseRequestId
        openExceptionId = fixture.openExceptionId
      } catch {
        hasJourneyFixture = false
        purchaseRequestId = ''
        openExceptionId = ''
      }
    })

    test('handoff investigates requests waive rejects waive then resolves with PR resubmit template', async ({
      page,
    }) => {
      test.skip(
        !hasJourneyFixture,
        'Post-reject resolve journey fixture could not be seeded.',
      )

      await signInFromSuite(page)
      await launchProductHandoffFromSuite(page, 'supplyarr')

      await page.goto(new URL('/purchasing', page.url()).toString())

      const panel = page.getByTestId('procurement-exceptions-panel')
      await panel.scrollIntoViewIfNeeded()
      await expect(panel).toBeVisible({ timeout: 15_000 })

      await panel.getByTestId('procurement-exception-subject-record').selectOption(purchaseRequestId)

      const templateSelect = panel.getByTestId('procurement-exception-resolution-template')
      await templateSelect.selectOption({ label: 'PR resubmit' })
      await expect(templateSelect).toHaveValue(resolutionTemplateKey)

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
        .getByTestId('procurement-exception-waive-justification')
        .fill(supplyArrProcurementExceptionRejectWaiveJourneyJustification)

      const requestWaiveButton = panel.getByTestId(
        `procurement-exception-request-waive-${openExceptionId}`,
      )
      await expect(requestWaiveButton).toBeVisible()
      await requestWaiveButton.click()

      await expect(statusBadge).toHaveText('waive_pending', { timeout: 15_000 })
      await assertSupplyArrProcurementExceptionStatusFromHandoff(openExceptionId, 'waive_pending')

      const rejectWaiveButton = panel.getByTestId(
        `procurement-exception-reject-waive-${openExceptionId}`,
      )
      await expect(rejectWaiveButton).toBeVisible()
      await rejectWaiveButton.click()

      await expect(statusBadge).toHaveText('investigating', { timeout: 15_000 })
      await assertSupplyArrProcurementExceptionRejectedWaiveFromHandoff(
        openExceptionId,
        supplyArrProcurementExceptionRejectWaiveDefaultReason,
      )

      const resolveButton = panel.getByTestId(`procurement-exception-resolve-${openExceptionId}`)
      await expect(resolveButton).toBeVisible()
      await resolveButton.click()

      await expect(statusBadge).toHaveText('resolved', { timeout: 15_000 })
      await assertSupplyArrProcurementExceptionPostRejectResolvedWithTemplateFromHandoff(
        openExceptionId,
        resolutionTemplateKey,
        supplyArrProcurementExceptionRejectWaiveDefaultReason,
      )

      await expect(
        panel.getByTestId(`procurement-exception-investigate-${openExceptionId}`),
      ).toHaveCount(0)
      await expect(
        panel.getByTestId(`procurement-exception-resolve-${openExceptionId}`),
      ).toHaveCount(0)
      await expect(
        panel.getByTestId(`procurement-exception-request-waive-${openExceptionId}`),
      ).toHaveCount(0)
    })
  },
)
