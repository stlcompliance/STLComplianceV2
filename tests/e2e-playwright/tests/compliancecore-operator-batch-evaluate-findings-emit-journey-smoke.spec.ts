import { test, expect } from '@playwright/test'

import { seedComplianceCoreJourney } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('Compliance Core operator batch evaluate and findings emit @requires-live', () => {
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

  test('operator runs batch rule evaluation after journey seed', async ({ page }) => {
    const journey = await seedComplianceCoreJourney()
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/evaluation', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Rule evaluation' })).toBeVisible({
      timeout: 15_000,
    })

    const batchPanel = page.getByTestId('batch-rule-evaluation-panel')
    await expect(batchPanel).toBeVisible({ timeout: 15_000 })

    await batchPanel.getByTestId(`batch-rule-evaluation-pack-${journey.rulePackKey}`).check()

    const licenseFact = batchPanel.locator('label').filter({ hasText: 'driver_license_valid' })
    if (await licenseFact.isVisible()) {
      await licenseFact.getByRole('checkbox').check()
    }

    await batchPanel.getByTestId('batch-rule-evaluation-run').click()

    const result = batchPanel.getByTestId('batch-rule-evaluation-latest-result')
    await expect(result).toBeVisible({ timeout: 15_000 })
    await expect(result).toHaveAttribute('data-allow-count', '1')
    await expect(result).toHaveAttribute('data-block-count', '0')
    await expect(result).toContainText(/allow/i)
  })

  test('operator gate check emits finding when blocked with emit enabled', async ({ page }) => {
    await seedComplianceCoreJourney()
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/findings', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Findings' })).toBeVisible({ timeout: 15_000 })

    const panel = page.getByTestId('findings-workflow-gates-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })

    await panel.getByTestId('findings-workflow-gate-select').selectOption('dispatch_driver_qualification')
    await expect(panel.getByTestId('findings-workflow-gate-emit-findings')).toBeChecked()

    const licenseFact = panel.locator('label').filter({ hasText: 'driver_license_valid' })
    if (await licenseFact.isVisible()) {
      await licenseFact.getByRole('checkbox').uncheck()
    }

    await panel.getByTestId('findings-workflow-gate-check').click()

    const result = panel.getByTestId('findings-workflow-gate-latest-result')
    await expect(result).toBeVisible({ timeout: 15_000 })
    await expect(result).toHaveAttribute('data-outcome', 'block')
    await expect(result).toContainText(/block/i)

    await expect(panel.getByTestId('findings-workflow-gate-emitted-notice')).toBeVisible({
      timeout: 15_000,
    })

    const findingsSection = panel.getByTestId('findings-workflow-gate-findings-section')
    await expect(findingsSection).not.toContainText(/No findings yet/i, { timeout: 15_000 })
    await expect(findingsSection).toContainText(/Findings \(\d+\)/)
  })
})
