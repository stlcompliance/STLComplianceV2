import { test, expect, type Page } from '@playwright/test'

import {
  ensureComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture,
  issueRoutArrDispatchNotificationWorkerToken,
  loginNexArr,
  processRoutArrDispatchNotificationBatch,
  type ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture,
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
  fixture: ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture,
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

async function overrideAssignDriverOnUnassignedTrip(
  page: Page,
  fixture: ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'routarr')

  await page.goto(new URL('/dispatch', page.url()).toString())

  const panel = page.getByTestId('unassigned-work-queue-panel')
  await panel.scrollIntoViewIfNeeded()
  await expect(panel).toBeVisible({ timeout: 15_000 })

  const tripRow = panel.getByTestId(`unassigned-trip-${fixture.tripId}`)
  await expect(tripRow).toBeVisible({ timeout: 15_000 })

  const driverSelect = tripRow.getByRole('combobox')
  await expect(driverSelect).toBeVisible()
  await driverSelect.selectOption({ value: fixture.driverPersonId })

  page.once('dialog', (dialog) => {
    expect(dialog.message()).toMatch(/workflow gate/i)
    void dialog.accept()
  })

  await tripRow.getByRole('button', { name: 'Assign' }).click()

  await expect(panel.getByTestId('unassigned-queue-status')).toContainText('Driver assigned.', {
    timeout: 15_000,
  })
  await expect(panel.getByTestId(`unassigned-trip-${fixture.tripId}`)).not.toBeVisible({
    timeout: 15_000,
  })
}

async function dispatchAssignedTripFromCommandCenter(
  page: Page,
  fixture: ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture,
): Promise<void> {
  const commandCenter = page.getByTestId('dispatch-command-center-panel')
  await commandCenter.scrollIntoViewIfNeeded()
  await expect(commandCenter).toBeVisible({ timeout: 15_000 })

  const tripCard = commandCenter.getByTestId(`command-center-trip-${fixture.tripId}`)
  await expect(tripCard).toBeVisible({ timeout: 15_000 })

  const dispatchButton = tripCard.getByTestId(`command-center-dispatch-${fixture.tripId}`)
  await expect(dispatchButton).toBeVisible({ timeout: 15_000 })
  await dispatchButton.click()

  await expect(commandCenter.getByTestId('command-center-status')).toContainText('Trip dispatched.', {
    timeout: 15_000,
  })
}

test.describe('Cross-product Compliance Core gate to RoutArr trip-dispatched notification @requires-live', () => {
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

  test('operator overrides gate block assign, dispatches trip, then trip_dispatched notification appears in settings', async ({
    page,
  }, testInfo) => {
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture
    try {
      fixture = await ensureComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture()
    } catch {
      testInfo.skip(
        true,
        'Cross-product dispatch gate trip-dispatched notification fixture could not be created.',
      )
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!)
    await returnToSuiteApp(page)
    await overrideAssignDriverOnUnassignedTrip(page, fixture!)
    await dispatchAssignedTripFromCommandCenter(page, fixture!)

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
    await expect(dispatchRow).not.toContainText('trip_assigned')
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
