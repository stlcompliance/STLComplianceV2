import { test, expect, type Page } from '@playwright/test'

import {
  ensureComplianceCoreRoutArrDispatchGateAllowFixture,
  ensureComplianceCoreRoutArrDispatchGateBlockFixture,
  type ComplianceCoreRoutArrDispatchGateJourneyFixture,
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
  fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture,
  licenseValid: boolean,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'compliancecore')

  await page.goto(new URL('/findings', page.url()).toString())
  await expect(page.getByRole('heading', { name: 'Findings' })).toBeVisible({ timeout: 15_000 })

  const panel = page.getByTestId('findings-workflow-gates-panel')
  await expect(panel).toBeVisible({ timeout: 15_000 })

  await panel.getByTestId('findings-workflow-gate-select').selectOption(fixture.dispatchGateKey)

  const licenseFact = panel.locator('label').filter({ hasText: 'driver_license_valid' })
  if (await licenseFact.isVisible()) {
    if (licenseValid) {
      await licenseFact.getByRole('checkbox').check()
    } else {
      await licenseFact.getByRole('checkbox').uncheck()
    }
  }

  await panel.getByTestId('findings-workflow-gate-check').click()

  const result = panel.getByTestId('findings-workflow-gate-latest-result')
  await expect(result).toBeVisible({ timeout: 15_000 })
  await expect(result).toHaveAttribute('data-outcome', licenseValid ? 'allow' : 'block')
  await expect(result).toContainText(new RegExp(licenseValid ? 'allow' : 'block', 'i'))
}

async function assignDriverOnUnassignedTrip(
  page: Page,
  fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture,
  confirmDialog: boolean,
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
    if (confirmDialog) {
      void dialog.accept()
    } else {
      void dialog.dismiss()
    }
  })

  await tripRow.getByRole('button', { name: 'Assign' }).click()
}

test.describe('Cross-product Compliance Core gate to RoutArr dispatch assign @requires-live', () => {
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

  test('operator confirms Compliance Core block then RoutArr assign is blocked', async ({
    page,
  }, testInfo) => {
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture
    try {
      fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
    } catch {
      testInfo.skip(true, 'Cross-product dispatch gate block fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, false)
    await returnToSuiteApp(page)
    await assignDriverOnUnassignedTrip(page, fixture!, false)

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await expect(panel.getByTestId('unassigned-queue-status')).toContainText(
      'Assignment cancelled.',
      { timeout: 15_000 },
    )
    await expect(panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)).toBeVisible()
  })

  test('operator overrides RoutArr workflow gate block and assigns driver', async ({
    page,
  }, testInfo) => {
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture
    try {
      fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
    } catch {
      testInfo.skip(true, 'Cross-product dispatch gate block fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, false)
    await returnToSuiteApp(page)
    await assignDriverOnUnassignedTrip(page, fixture!, true)

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await expect(panel.getByTestId('unassigned-queue-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    await expect(panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })

  test('operator confirms Compliance Core allow then RoutArr assigns without gate block', async ({
    page,
  }, testInfo) => {
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture
    try {
      fixture = await ensureComplianceCoreRoutArrDispatchGateAllowFixture()
    } catch {
      testInfo.skip(true, 'Cross-product dispatch gate allow fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, true)
    await returnToSuiteApp(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    const tripRow = panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)
    await expect(tripRow).toBeVisible({ timeout: 15_000 })

    const driverSelect = tripRow.getByRole('combobox')
    await driverSelect.selectOption({ value: fixture!.driverPersonId })

    let dialogShown = false
    page.once('dialog', () => {
      dialogShown = true
    })

    await tripRow.getByRole('button', { name: 'Assign' }).click()

    await expect(panel.getByTestId('unassigned-queue-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    expect(dialogShown).toBe(false)
    await expect(panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })
})
