import { test, expect } from '@playwright/test'

import { ensureRoutArrDispatchExceptionTriageFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch exception triage depth @requires-live', () => {
  let hasExceptionFixture = false
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
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    try {
      const fixture = await ensureRoutArrDispatchExceptionTriageFixture()
      hasExceptionFixture = fixture.exceptionIds.length > 0
      overdueExceptionId = fixture.overdueExceptionId
      openExceptionId = fixture.openExceptionId
    } catch {
      hasExceptionFixture = false
      overdueExceptionId = null
      openExceptionId = null
    }
  })

  test('handoff opens exception queue bulk/template controls without triage mutations', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('dispatch-exception-queue-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Exception queue' })).toBeVisible()

    const bulkActions = panel.getByTestId('exception-bulk-actions')
    const templateSelect = panel.getByTestId('exception-resolution-template')
    const overdueFilter = panel.getByTestId('exception-overdue-filter')
    const bulkAssignButton = panel.getByTestId('exception-bulk-assign')
    const bulkResolveButton = panel.getByTestId('exception-bulk-resolve')

    const hasBulkPanel = (await bulkActions.count()) > 0
    if (hasBulkPanel) {
      await expect(bulkActions).toBeVisible()
      await expect(templateSelect).toBeVisible()
      await expect(templateSelect.locator('option')).not.toHaveCount(0)

      await expect(bulkAssignButton).toBeDisabled()
      await expect(bulkResolveButton).toBeDisabled()

      const targetExceptionId = openExceptionId ?? overdueExceptionId
      if (hasExceptionFixture && targetExceptionId) {
        const rowCheckbox = panel.getByTestId(`exception-select-${targetExceptionId}`)
        if ((await rowCheckbox.count()) > 0) {
          await rowCheckbox.check()
          await expect(bulkAssignButton).toBeEnabled()
          await expect(bulkResolveButton).toBeEnabled()
          await expect(bulkActions).toContainText('1 selected')
        }
      } else {
        const firstRowCheckbox = panel.locator('[data-testid^="exception-select-"]').first()
        if ((await firstRowCheckbox.count()) > 0) {
          await firstRowCheckbox.check()
          await expect(bulkAssignButton).toBeEnabled()
          await expect(bulkResolveButton).toBeEnabled()
        }
      }

      await templateSelect.selectOption({ label: 'Reschedule departure' })
      await expect(templateSelect).toHaveValue('reschedule_departure')
    }

    if ((await overdueFilter.count()) > 0) {
      await expect(overdueFilter).toBeVisible()
      if (hasExceptionFixture && overdueExceptionId) {
        await overdueFilter.check()
        await expect(panel.getByTestId(`exception-row-${overdueExceptionId}`)).toBeVisible({
          timeout: 15_000,
        })
        await overdueFilter.uncheck()
      }
    }

    if (hasExceptionFixture && overdueExceptionId) {
      const breachedBadge = panel.getByTestId(`exception-sla-breached-${overdueExceptionId}`)
      if ((await breachedBadge.count()) > 0) {
        await expect(breachedBadge).toBeVisible()
      }
    }
  })
})
