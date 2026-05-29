import { test, expect, type Page } from '@playwright/test'

import {
  ensureComplianceCoreRoutArrDispatchGateAllowFixture,
  ensureComplianceCoreRoutArrDispatchGateBlockFixture,
  ensureComplianceCoreRoutArrDispatchGateWarnFixture,
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
  expectedOutcome: 'allow' | 'block' | 'warn',
  licenseValid?: boolean,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'compliancecore')

  await page.goto(new URL('/findings', page.url()).toString())
  await expect(page.getByRole('heading', { name: 'Findings' })).toBeVisible({ timeout: 15_000 })

  const panel = page.getByTestId('findings-workflow-gates-panel')
  await expect(panel).toBeVisible({ timeout: 15_000 })

  await panel.getByTestId('findings-workflow-gate-select').selectOption(fixture.dispatchGateKey)

  if (licenseValid !== undefined) {
    const licenseFact = panel.locator('label').filter({ hasText: 'driver_license_valid' })
    if (await licenseFact.isVisible()) {
      if (licenseValid) {
        await licenseFact.getByRole('checkbox').check()
      } else {
        await licenseFact.getByRole('checkbox').uncheck()
      }
    }
  }

  await panel.getByTestId('findings-workflow-gate-check').click()

  const result = panel.getByTestId('findings-workflow-gate-latest-result')
  await expect(result).toBeVisible({ timeout: 15_000 })
  await expect(result).toHaveAttribute('data-outcome', expectedOutcome)
  await expect(result).toContainText(new RegExp(expectedOutcome, 'i'))
}

async function dragAssignDriverOnCommandCenterTrip(
  page: Page,
  fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture,
  confirmDialog: boolean,
  dialogPattern: RegExp,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'routarr')

  await page.goto(new URL('/dispatch', page.url()).toString())

  const panel = page.getByTestId('dispatch-command-center-panel')
  await panel.scrollIntoViewIfNeeded()
  await expect(panel).toBeVisible({ timeout: 15_000 })

  const tripCard = panel.getByTestId(`command-center-trip-${fixture.tripId}`)
  await expect(tripCard).toBeVisible({ timeout: 15_000 })

  const driverChip = panel.getByTestId(`command-center-driver-chip-${fixture.driverPersonId}`)
  await expect(driverChip).toBeVisible({ timeout: 15_000 })

  page.once('dialog', (dialog) => {
    expect(dialog.message()).toMatch(dialogPattern)
    if (confirmDialog) {
      void dialog.accept()
    } else {
      void dialog.dismiss()
    }
  })

  await driverChip.dragTo(tripCard)
}

test.describe('Cross-product Compliance Core gate to RoutArr command center drag assign @requires-live', () => {
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

  test('operator confirms Compliance Core block then command center drag assign is blocked', async ({
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
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'block', false)
    await returnToSuiteApp(page)
    await dragAssignDriverOnCommandCenterTrip(page, fixture!, false, /workflow gate/i)

    const panel = page.getByTestId('dispatch-command-center-panel')
    await expect(panel.getByTestId('command-center-status')).toContainText(
      'Assignment cancelled.',
      { timeout: 15_000 },
    )
    await expect(panel.getByTestId(`command-center-trip-${fixture!.tripId}`)).toBeVisible()
  })

  test('operator overrides command center workflow gate block via drag-and-drop assign', async ({
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
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'block', false)
    await returnToSuiteApp(page)
    await dragAssignDriverOnCommandCenterTrip(page, fixture!, true, /workflow gate/i)

    const panel = page.getByTestId('dispatch-command-center-panel')
    await expect(panel.getByTestId('command-center-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    await expect(panel.getByTestId(`command-center-trip-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })

  test('operator confirms Compliance Core allow then command center drag assigns without gate block', async ({
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
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'allow', true)
    await returnToSuiteApp(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('dispatch-command-center-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    const tripCard = panel.getByTestId(`command-center-trip-${fixture!.tripId}`)
    await expect(tripCard).toBeVisible({ timeout: 15_000 })

    const driverChip = panel.getByTestId(`command-center-driver-chip-${fixture!.driverPersonId}`)
    await expect(driverChip).toBeVisible({ timeout: 15_000 })

    let dialogShown = false
    page.once('dialog', () => {
      dialogShown = true
    })

    await driverChip.dragTo(tripCard)

    await expect(panel.getByTestId('command-center-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    expect(dialogShown).toBe(false)
    await expect(panel.getByTestId(`command-center-trip-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })

  test('operator dismisses command center workflow gate warn on drag assign', async ({
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
      fixture = await ensureComplianceCoreRoutArrDispatchGateWarnFixture()
    } catch {
      testInfo.skip(true, 'Cross-product dispatch gate warn fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'warn')
    await returnToSuiteApp(page)
    await dragAssignDriverOnCommandCenterTrip(
      page,
      fixture!,
      false,
      /Compliance workflow gate warning/i,
    )

    const panel = page.getByTestId('dispatch-command-center-panel')
    await expect(panel.getByTestId('command-center-status')).toContainText(
      'Assignment cancelled.',
      { timeout: 15_000 },
    )
    await expect(panel.getByTestId(`command-center-trip-${fixture!.tripId}`)).toBeVisible()
  })

  test('operator confirms command center workflow gate warn and assigns driver', async ({
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
      fixture = await ensureComplianceCoreRoutArrDispatchGateWarnFixture()
    } catch {
      testInfo.skip(true, 'Cross-product dispatch gate warn fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'warn')
    await returnToSuiteApp(page)
    await dragAssignDriverOnCommandCenterTrip(
      page,
      fixture!,
      true,
      /Compliance workflow gate warning/i,
    )

    const panel = page.getByTestId('dispatch-command-center-panel')
    await expect(panel.getByTestId('command-center-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    await expect(panel.getByTestId(`command-center-trip-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })
})
