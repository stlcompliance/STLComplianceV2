import { test, expect } from '@playwright/test'

import { seedComplianceCoreJourney } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const dispatchGateKeys = [
  'dispatch_driver_qualification',
  'dispatch_hazmat',
  'dispatch_hours_of_service',
] as const

test.describe('Compliance Core operator batch workflow gate check journey @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
  })

  test('operator runs batch gate check with shared facts after journey seed', async ({ page }) => {
    await seedComplianceCoreJourney()
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/findings', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Findings' })).toBeVisible({ timeout: 15_000 })

    const gatesPanel = page.getByTestId('findings-workflow-gates-panel')
    await expect(gatesPanel).toBeVisible({ timeout: 15_000 })

    const batchPanel = gatesPanel.getByTestId('batch-workflow-gate-check-panel')
    await expect(batchPanel).toBeVisible({ timeout: 15_000 })

    for (const gateKey of dispatchGateKeys) {
      await batchPanel.getByTestId(`batch-workflow-gate-gate-${gateKey}`).check()
    }

    const licenseFact = batchPanel.locator('label').filter({ hasText: 'driver_license_valid' })
    if (await licenseFact.isVisible()) {
      await licenseFact.getByRole('checkbox').check()
    }

    await batchPanel.getByTestId('batch-workflow-gate-run').click()

    const result = batchPanel.getByTestId('batch-workflow-gate-latest-result')
    await expect(result).toBeVisible({ timeout: 15_000 })
    await expect(result).toHaveAttribute('data-allow-count', String(dispatchGateKeys.length))
    await expect(result).toHaveAttribute('data-block-count', '0')
    await expect(result).toContainText(/allow/i)
  })

  test('operator batch gate check emits findings when all gates blocked', async ({ page }) => {
    await seedComplianceCoreJourney()
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/findings', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Findings' })).toBeVisible({ timeout: 15_000 })

    const gatesPanel = page.getByTestId('findings-workflow-gates-panel')
    await expect(gatesPanel).toBeVisible({ timeout: 15_000 })

    const batchPanel = gatesPanel.getByTestId('batch-workflow-gate-check-panel')
    await expect(batchPanel).toBeVisible({ timeout: 15_000 })

    for (const gateKey of dispatchGateKeys) {
      await batchPanel.getByTestId(`batch-workflow-gate-gate-${gateKey}`).check()
    }

    const licenseFact = batchPanel.locator('label').filter({ hasText: 'driver_license_valid' })
    if (await licenseFact.isVisible()) {
      await licenseFact.getByRole('checkbox').uncheck()
    }

    await batchPanel.getByTestId('batch-workflow-gate-emit-findings').check()
    await batchPanel.getByTestId('batch-workflow-gate-run').click()

    const result = batchPanel.getByTestId('batch-workflow-gate-latest-result')
    await expect(result).toBeVisible({ timeout: 15_000 })
    await expect(result).toHaveAttribute('data-block-count', String(dispatchGateKeys.length))
    await expect(result).toContainText(/block/i)

    const findingsSection = gatesPanel.getByTestId('findings-workflow-gate-findings-section')
    await expect(findingsSection).not.toContainText(/No findings yet/i, { timeout: 15_000 })
    await expect(findingsSection).toContainText(/Findings \(\d+\)/)
  })
})
