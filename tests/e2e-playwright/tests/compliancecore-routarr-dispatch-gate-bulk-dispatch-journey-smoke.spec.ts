import { test, expect, type Page } from '@playwright/test'

import {
  ensureComplianceCoreRoutArrDispatchGateBlockFixture,
  ensureComplianceCoreRoutArrDispatchGateBulkDispatchBlockFixture,
  ensureComplianceCoreRoutArrDispatchGateBulkDispatchWarnFixture,
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

async function bulkDispatchDriverWithGateDialog(
  page: Page,
  fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture,
  confirmDialog: boolean,
  dialogPattern: RegExp,
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
    expect(dialog.message()).toMatch(dialogPattern)
    if (confirmDialog) {
      void dialog.accept()
    } else {
      void dialog.dismiss()
    }
  })

  await panel.getByTestId('bulk-dispatch-apply').click()
}

test.describe('Cross-product Compliance Core gate to RoutArr bulk dispatch @requires-live', () => {
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

  test('operator confirms Compliance Core block then bulk dispatch preview shows workflow gate block', async ({
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
      fixture = await ensureComplianceCoreRoutArrDispatchGateBulkDispatchBlockFixture()
    } catch {
      testInfo.skip(true, 'Cross-product bulk dispatch gate block fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'block', false)
    await returnToSuiteApp(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('bulk-dispatch-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    await panel.getByTestId(`bulk-trip-${fixture!.tripId}`).check()
    await panel.getByLabel('Driver person id', { exact: false }).fill(fixture!.driverPersonId)
    await panel.getByTestId('bulk-dispatch-preview').click()

    const previewSummary = panel.getByTestId(`bulk-preview-summary-${fixture!.tripId}`)
    await expect(previewSummary).toBeVisible({ timeout: 15_000 })
    await expect(previewSummary).toContainText(/workflow gate/i)
  })

  test('operator cancels bulk dispatch workflow gate block override', async ({ page }, testInfo) => {
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
    await bulkDispatchDriverWithGateDialog(page, fixture!, false, /workflow gate block/i)

    const panel = page.getByTestId('bulk-dispatch-panel')
    await expect(panel.getByTestId('bulk-dispatch-status')).toContainText('Bulk apply cancelled.', {
      timeout: 15_000,
    })
    await expect(panel.getByTestId(`bulk-trip-${fixture!.tripId}`)).toBeChecked()
  })

  test('operator overrides bulk dispatch workflow gate block and assigns driver', async ({
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
      fixture = await ensureComplianceCoreRoutArrDispatchGateBulkDispatchBlockFixture()
    } catch {
      testInfo.skip(true, 'Cross-product bulk dispatch gate block fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'block', false)
    await returnToSuiteApp(page)
    await bulkDispatchDriverWithGateDialog(page, fixture!, true, /workflow gate block/i)

    const panel = page.getByTestId('bulk-dispatch-panel')
    await expect(panel.getByTestId('bulk-dispatch-status')).toContainText(
      /Applied 1\/1 trip updates\./,
      { timeout: 15_000 },
    )
    await expect(panel.getByTestId(`bulk-preview-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })

  test('operator dismisses bulk dispatch workflow gate warn on apply', async ({ page }, testInfo) => {
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: ComplianceCoreRoutArrDispatchGateJourneyFixture
    try {
      fixture = await ensureComplianceCoreRoutArrDispatchGateBulkDispatchWarnFixture()
    } catch {
      testInfo.skip(true, 'Cross-product bulk dispatch gate warn fixture could not be created.')
    }

    await signInFromSuite(page)
    await runComplianceCoreDispatchGateCheck(page, fixture!, 'warn')
    await returnToSuiteApp(page)
    await bulkDispatchDriverWithGateDialog(
      page,
      fixture!,
      false,
      /Compliance workflow gate warning/i,
    )

    const panel = page.getByTestId('bulk-dispatch-panel')
    await expect(panel.getByTestId('bulk-dispatch-status')).toContainText('Bulk apply cancelled.', {
      timeout: 15_000,
    })
    await expect(panel.getByTestId(`bulk-trip-${fixture!.tripId}`)).toBeChecked()
  })

  test('operator confirms bulk dispatch workflow gate warn and assigns driver', async ({
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
    await bulkDispatchDriverWithGateDialog(
      page,
      fixture!,
      true,
      /Compliance workflow gate warning/i,
    )

    const panel = page.getByTestId('bulk-dispatch-panel')
    await expect(panel.getByTestId('bulk-dispatch-status')).toContainText(
      /Applied 1\/1 trip updates\./,
      { timeout: 15_000 },
    )
    await expect(panel.getByTestId(`bulk-preview-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })
})
