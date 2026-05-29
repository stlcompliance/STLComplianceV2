import { test, expect } from '@playwright/test'

import {
  assertSupplyArrProcurementExceptionAssignedAndLinkedFromHandoff,
  ensureSupplyArrProcurementExceptionAssignLinkJourneyFixture,
  getSupplyArrMeFromHandoff,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe(
  'SupplyArr purchasing procurement exception assign link journey @requires-live',
  () => {
    let hasJourneyFixture = false
    let currentUserId = ''
    let purchaseRequestId = ''
    let followUpPurchaseRequestId = ''
    let followUpRequestKey = ''
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
        const me = await getSupplyArrMeFromHandoff()
        currentUserId = me.userId
        const fixture = await ensureSupplyArrProcurementExceptionAssignLinkJourneyFixture()
        hasJourneyFixture = Boolean(fixture.openExceptionId)
        purchaseRequestId = fixture.purchaseRequestId
        followUpPurchaseRequestId = fixture.followUpPurchaseRequestId
        followUpRequestKey = fixture.followUpRequestKey
        openExceptionId = fixture.openExceptionId
      } catch {
        hasJourneyFixture = false
        currentUserId = ''
        purchaseRequestId = ''
        followUpPurchaseRequestId = ''
        followUpRequestKey = ''
        openExceptionId = ''
      }
    })

    test('handoff assigns resolver and links follow-up PR on exception detail', async ({ page }) => {
      test.skip(!hasJourneyFixture, 'Assign/link journey fixture could not be seeded.')

      await signInFromSuite(page)
      await launchProductHandoffFromSuite(page, 'supplyarr')

      await page.goto(new URL('/purchasing', page.url()).toString())

      const panel = page.getByTestId('procurement-exceptions-panel')
      await panel.scrollIntoViewIfNeeded()
      await expect(panel).toBeVisible({ timeout: 15_000 })

      await panel.getByTestId('procurement-exception-subject-record').selectOption(purchaseRequestId)

      const openRow = panel.getByTestId(`procurement-exception-row-${openExceptionId}`)
      await expect(openRow).toBeVisible({ timeout: 15_000 })
      await expect(openRow).toContainText('unassigned')

      await panel.getByTestId(`procurement-exception-key-${openExceptionId}`).click()

      const detail = panel.getByTestId('procurement-exception-detail')
      await expect(detail).toBeVisible({ timeout: 15_000 })
      await expect(detail).toContainText('unassigned')

      const assignButton = panel.getByTestId(`procurement-exception-assign-${openExceptionId}`)
      await expect(assignButton).toBeVisible()
      await assignButton.click()

      await expect(detail).toContainText(currentUserId, { timeout: 15_000 })
      await expect(openRow).toContainText('assigned')

      await panel.getByTestId('procurement-exception-link-pr').selectOption(followUpPurchaseRequestId)

      const saveLinksButton = panel.getByTestId(
        `procurement-exception-save-links-${openExceptionId}`,
      )
      await expect(saveLinksButton).toBeVisible()
      await saveLinksButton.click()

      const linkedActions = panel.getByTestId('procurement-exception-linked-actions')
      await expect(linkedActions).toBeVisible({ timeout: 15_000 })
      await expect(linkedActions).toContainText(`PR ${followUpRequestKey}`)

      await assertSupplyArrProcurementExceptionAssignedAndLinkedFromHandoff(
        openExceptionId,
        currentUserId,
        followUpPurchaseRequestId,
        followUpRequestKey,
      )
    })
  },
)
