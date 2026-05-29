import { test, expect, type Page } from '@playwright/test'

import {
  ensureComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture,
  issueRoutArrDispatchNotificationWorkerToken,
  loginNexArr,
  processRoutArrDispatchNotificationBatch,
  type ComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite, returnToSuiteApp } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

async function runComplianceCoreDispatchGateCheck(
  page: Page,
  fixture: ComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'compliancecore')

  await page.goto(new URL('/findings', page.url()).toString())
  await expect(page.getByRole('heading', { name: 'Findings' })).toBeVisible({ timeout: 15_000 })

  const panel = page.getByTestId('findings-workflow-gates-panel')
  await expect(panel).toBeVisible({ timeout: 15_000 })

  await panel.getByTestId('findings-workflow-gate-select').selectOption(fixture.dispatchGateKey)

  const licenseFact = panel.locator('label').filter({ hasText: 'driver_license_valid' })
  if (await licenseFact.isVisible()) {
    await licenseFact.getByRole('checkbox').uncheck()
  }

  await panel.getByTestId('findings-workflow-gate-check').click()

  const result = panel.getByTestId('findings-workflow-gate-latest-result')
  await expect(result).toBeVisible({ timeout: 15_000 })
  await expect(result).toHaveAttribute('data-outcome', 'block')
  await expect(result).toContainText(/block/i)
}

async function overrideBulkDispatchDriverWithGateDialog(
  page: Page,
  fixture: ComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'routarr')

  await page.goto(new URL('/dispatch', page.url()).toString())

  const panel = page.getByTestId('bulk-dispatch-panel')
  await panel.scrollIntoViewIfNeeded()
  await expect(panel).toBeVisible({ timeout: 15_000 })

  const tripCheckbox = panel.getByTestId(`bulk-trip-${fixture.tripId}`)
  await expect(tripCheckbox).toBeVisible({ timeout: 15_000 })
  await tripCheckbox.check()

  await panel.getByLabel('Driver person id', { exact: false }).fill(fixture.driverPersonId)
  await panel.getByTestId('bulk-dispatch-preview').click()

  const previewSummary = panel.getByTestId(`bulk-preview-summary-${fixture.tripId}`)
  await expect(previewSummary).toBeVisible({ timeout: 15_000 })
  await expect(previewSummary).toContainText(/workflow gate/i)

  page.once('dialog', (dialog) => {
    expect(dialog.message()).toMatch(/workflow gate block/i)
    void dialog.accept()
  })

  await panel.getByTestId('bulk-dispatch-apply').click()

  await expect(panel.getByTestId('bulk-dispatch-status')).toContainText(
    /Applied 1\/1 trip updates\./,
    { timeout: 15_000 },
  )
  await expect(panel.getByTestId(`bulk-preview-${fixture.tripId}`)).not.toBeVisible({
    timeout: 15_000,
  })
}

test.describe('Cross-product Compliance Core gate to RoutArr bulk dispatch trip-assigned notification @requires-live', () => {
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
  })

  test('operator overrides gate block via bulk dispatch then trip_assigned notification appears in settings', async ({
    page,
  }, testInfo) => {
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: ComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture
    try {
      fixture =
        await ensureComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture()
    } catch {
      testInfo.skip(
        true,
        'Cross-product dispatch gate bulk dispatch trip-assigned notification fixture could not be created.',
      )
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!)
    await returnToSuiteApp(page)
    await overrideBulkDispatchDriverWithGateDialog(page, fixture!)

    await launchProductHandoffFromSuite(page, 'routarr')
    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('notification-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Dispatch notifications' })).toBeVisible()
    await expect(panel.getByRole('heading', { name: 'Recent dispatches' })).toBeVisible()

    const dispatchRow = panel.getByTestId(`notification-dispatch-row-${fixture!.tripId}`)
    await expect(dispatchRow).toBeVisible({ timeout: 15_000 })
    await expect(dispatchRow).toContainText(fixture!.expectedEventKind)
    await expect(dispatchRow).toContainText('pending')
    await expect(dispatchRow).toContainText(`Trip ${fixture!.tripId}`)

    const adminToken = await loginNexArr()
    const workerToken = await issueRoutArrDispatchNotificationWorkerToken(adminToken)
    await processRoutArrDispatchNotificationBatch(workerToken)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    const processedRow = panel.getByTestId(`notification-dispatch-row-${fixture!.tripId}`)
    await expect(processedRow).toBeVisible({ timeout: 15_000 })
    await expect(processedRow).toContainText(fixture!.expectedEventKind)
    await expect(processedRow).not.toContainText('pending')
    await expect(processedRow).toContainText(/sent|failed|skipped/i)
  })
})
