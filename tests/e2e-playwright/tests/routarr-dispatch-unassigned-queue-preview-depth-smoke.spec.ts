import { test, expect } from '@playwright/test'

import { ensureRoutArrUnassignedQueuePreviewFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch unassigned queue preview depth @requires-live', () => {
  let hasUnassignedFixture = false
  let lateTripId: string | null = null
  let onTrackTripId: string | null = null

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
      const fixture = await ensureRoutArrUnassignedQueuePreviewFixture()
      hasUnassignedFixture = fixture.tripIds.length > 0
      lateTripId = fixture.lateTripId
      onTrackTripId = fixture.onTrackTripId
    } catch {
      hasUnassignedFixture = false
      lateTripId = null
      onTrackTripId = null
    }
  })

  test('handoff opens unassigned queue preview controls without assign mutations', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Unassigned work queue' })).toBeVisible()

    const attentionFilter = panel.getByTestId('unassigned-attention-filter')
    const bulkAssignButton = panel.getByTestId('bulk-assign-unassigned')
    const bulkDriverSelect = panel.getByLabel('Bulk assign driver', { exact: false })

    await expect(attentionFilter).toBeVisible()
    await expect(panel.getByText(/urgent \(late\/at-risk\)/)).toBeVisible()

    const targetTripId = lateTripId ?? onTrackTripId
    const targetRow = targetTripId
      ? panel.getByTestId(`unassigned-trip-${targetTripId}`)
      : panel.locator('[data-testid^="unassigned-trip-"]').first()

    if (hasUnassignedFixture && targetTripId) {
      await expect(targetRow).toBeVisible({ timeout: 15_000 })
      await expect(targetRow).toContainText(/min ago|In \d+ min|Starting now|No start time/)
    }

    const hasBulkBar = (await bulkDriverSelect.count()) > 0
    if (hasBulkBar) {
      await expect(bulkAssignButton).toBeDisabled()

      const rowCheckbox = targetTripId
        ? targetRow.getByRole('checkbox')
        : panel.locator('[data-testid^="unassigned-trip-"]').first().getByRole('checkbox')

      if ((await rowCheckbox.count()) > 0) {
        await rowCheckbox.check()
        await expect(bulkAssignButton).toBeEnabled()

        const driverOptions = bulkDriverSelect.locator('option')
        const optionCount = await driverOptions.count()
        if (optionCount > 1) {
          await bulkDriverSelect.selectOption({ index: 1 })
        }
      }
    }

    if (hasUnassignedFixture && lateTripId && onTrackTripId) {
      await attentionFilter.check()
      await expect(panel.getByTestId(`unassigned-trip-${lateTripId}`)).toBeVisible({
        timeout: 15_000,
      })
      await expect(panel.getByTestId(`unassigned-trip-${onTrackTripId}`)).not.toBeVisible()
      await attentionFilter.uncheck()
      await expect(panel.getByTestId(`unassigned-trip-${onTrackTripId}`)).toBeVisible({
        timeout: 15_000,
      })
    } else if ((await attentionFilter.count()) > 0) {
      await attentionFilter.check()
      await attentionFilter.uncheck()
    }

    const perTripRow = targetTripId
      ? panel.getByTestId(`unassigned-trip-${targetTripId}`)
      : panel.locator('[data-testid^="unassigned-trip-"]').first()
    const perTripAssign = perTripRow.getByRole('button', { name: 'Assign' })
    const perTripDriverSelect = perTripRow.getByRole('combobox')

    if ((await perTripAssign.count()) > 0 && (await perTripDriverSelect.count()) > 0) {
      const perTripOptions = perTripDriverSelect.locator('option')
      if ((await perTripOptions.count()) > 1) {
        await perTripDriverSelect.selectOption({ index: 1 })
      }

      page.once('dialog', (dialog) => dialog.dismiss())
      await perTripAssign.click()
      await expect(panel.getByTestId('unassigned-queue-status')).toContainText(
        'Assignment cancelled.',
        { timeout: 15_000 },
      )
    }
  })
})
