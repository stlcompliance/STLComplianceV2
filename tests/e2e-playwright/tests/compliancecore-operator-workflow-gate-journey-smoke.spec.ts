import { test, expect } from '@playwright/test'

import { seedComplianceCoreJourney } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('Compliance Core operator workflow gate journey @requires-live', () => {
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

  test('operator runs dispatch qualification gate check after journey seed', async ({ page }) => {
    await seedComplianceCoreJourney()
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/findings', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Findings' })).toBeVisible({ timeout: 15_000 })

    const panel = page.getByTestId('findings-workflow-gates-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })

    await expect(panel.getByText(/Workflow gates \(\d+\)/)).toBeVisible({ timeout: 15_000 })

    const gateSelect = panel.getByTestId('findings-workflow-gate-select')
    await gateSelect.selectOption('dispatch_driver_qualification')

    const licenseFact = panel.locator('label').filter({ hasText: 'driver_license_valid' })
    if (await licenseFact.isVisible()) {
      await licenseFact.getByRole('checkbox').check()
    }

    await panel.getByTestId('findings-workflow-gate-check').click()

    const result = panel.getByTestId('findings-workflow-gate-latest-result')
    await expect(result).toBeVisible({ timeout: 15_000 })
    await expect(result).toHaveAttribute('data-outcome', 'allow')
    await expect(result).toContainText(/allow/i)
  })

  test('operator dashboard loads summary sections after journey seed', async ({ page }) => {
    await seedComplianceCoreJourney()
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/operator', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Operator dashboard' })).toBeVisible({
      timeout: 15_000,
    })

    const dashboard = page.getByTestId('compliancecore-operator-dashboard-panel')
    await expect(dashboard).toBeVisible({ timeout: 15_000 })
    await expect(dashboard.getByText('Operator overview')).toBeVisible()
    await expect(dashboard.getByText('Total findings')).toBeVisible()
    await expect(dashboard.getByText('Total runs')).toBeVisible()
    await expect(dashboard.getByText('Gate definitions')).toBeVisible()
    await expect(dashboard.getByText('Loading operator dashboard…')).not.toBeVisible()
  })
})
