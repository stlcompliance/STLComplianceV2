import { test, expect } from '@playwright/test'

import { ensureSupplyArrProcurementExceptionsFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr purchasing procurement exceptions @requires-live', () => {
  let hasExceptionFixture = false
  let purchaseRequestId = ''
  let requestKey = ''
  let overdueExceptionId: string | null = null
  let openExceptionId: string | null = null

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
      const fixture = await ensureSupplyArrProcurementExceptionsFixture()
      hasExceptionFixture = fixture.exceptionIds.length > 0
      purchaseRequestId = fixture.purchaseRequestId
      requestKey = fixture.requestKey
      overdueExceptionId = fixture.overdueExceptionId
      openExceptionId = fixture.openExceptionId
    } catch {
      hasExceptionFixture = false
      purchaseRequestId = ''
      requestKey = ''
      overdueExceptionId = null
      openExceptionId = null
    }
  })

  test('handoff opens procurement exceptions panel with SLA badges and resolver controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    await page.goto(new URL('/purchasing', page.url()).toString())

    const panel = page.getByTestId('procurement-exceptions-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Procurement exceptions' })).toBeVisible()

    await expect(panel.getByTestId('procurement-exceptions-active-count')).toBeVisible()

    const templateSelect = panel.getByTestId('procurement-exception-resolution-template')
    await expect(templateSelect).toBeVisible()
    await expect(templateSelect.locator('option')).not.toHaveCount(0)
    await templateSelect.selectOption({ label: 'PR resubmit' })
    await expect(templateSelect).toHaveValue('pr_resubmit')

    if (hasExceptionFixture && purchaseRequestId) {
      const subjectSelect = panel.getByTestId('procurement-exception-subject-record')
      await subjectSelect.selectOption(purchaseRequestId)

      if (overdueExceptionId) {
        const overdueRow = panel.getByTestId(`procurement-exception-row-${overdueExceptionId}`)
        await expect(overdueRow).toBeVisible({ timeout: 15_000 })
        await expect(
          panel.getByTestId(`procurement-exception-sla-breached-${overdueExceptionId}`),
        ).toBeVisible()
      }

      if (openExceptionId) {
        const openRow = panel.getByTestId(`procurement-exception-row-${openExceptionId}`)
        await expect(openRow).toBeVisible({ timeout: 15_000 })
        await expect(
          panel.getByTestId(`procurement-exception-investigate-${openExceptionId}`),
        ).toBeVisible()

        await panel.getByTestId(`procurement-exception-key-${openExceptionId}`).click()
        const detail = panel.getByTestId('procurement-exception-detail')
        await expect(detail).toBeVisible({ timeout: 15_000 })
        await expect(
          panel.getByTestId(`procurement-exception-assign-${openExceptionId}`),
        ).toBeVisible()
      }

      if (requestKey) {
        await expect(panel.getByTestId('procurement-exception-subject-record')).toContainText(
          requestKey,
        )
      }
    }
  })
})
